using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Overview.Server.Api.Contracts.Sync;
using Overview.Server.Domain.Entities;
using Overview.Server.Domain.Enums;
using Overview.Server.Infrastructure.Configuration;
using Overview.Server.Infrastructure.Persistence;

namespace Overview.Server.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/sync")]
public sealed class SyncController : ControllerBase
{
    private readonly OverviewDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly SyncOptions _syncOptions;

    public SyncController(
        OverviewDbContext dbContext,
        TimeProvider timeProvider,
        IOptions<SyncOptions> syncOptions)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _syncOptions = syncOptions.Value;
    }

    [HttpGet("pull")]
    [ProducesResponseType<SyncPullResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SyncPullResponse>> PullAsync(
        [FromQuery] DateTimeOffset? since,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var itemsQuery = _dbContext.Items
            .AsNoTracking()
            .Where(item => item.UserId == userId.Value);

        if (since is not null)
        {
            itemsQuery = itemsQuery.Where(item => item.LastModifiedAt > since.Value);
        }

        var items = await itemsQuery
            .OrderBy(item => item.LastModifiedAt)
            .Select(item => new SyncItemContract
            {
                Value = item
            })
            .ToListAsync(cancellationToken);

        var settingsQuery = _dbContext.UserSettings
            .AsNoTracking()
            .Where(settings => settings.UserId == userId.Value);

        if (since is not null)
        {
            settingsQuery = settingsQuery.Where(settings => settings.LastModifiedAt > since.Value);
        }

        var settings = await settingsQuery
            .OrderByDescending(settings => settings.LastModifiedAt)
            .Select(settings => new SyncSettingsContract
            {
                Value = settings
            })
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(new SyncPullResponse
        {
            ServerTime = _timeProvider.GetUtcNow(),
            Since = since,
            Items = items,
            Settings = settings
        });
    }

    [HttpPost("push")]
    [ProducesResponseType<SyncPushResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SyncPushResponse>> PushAsync(
        [FromBody] SyncPushRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            return ValidationProblem("DeviceId is required.");
        }

        if (request.ItemChanges.Count > _syncOptions.MaxBatchSize)
        {
            return ValidationProblem($"ItemChanges exceeds max batch size {_syncOptions.MaxBatchSize}.");
        }

        var now = _timeProvider.GetUtcNow();
        var conflicts = new List<SyncConflictContract>();
        var appliedChangeCount = 0;

        foreach (var change in request.ItemChanges.OrderBy(change => change.LastModifiedAt))
        {
            var validationError = ValidateItemChange(change);
            if (validationError is not null)
            {
                return ValidationProblem(validationError);
            }

            var alreadyProcessed = await _dbContext.SyncChanges
                .AsNoTracking()
                .AnyAsync(
                    syncChange => syncChange.UserId == userId.Value && syncChange.Id == change.ChangeId,
                    cancellationToken);

            if (alreadyProcessed)
            {
                continue;
            }

            var entityId = change.Item?.Id ?? change.EntityId!.Value;
            var existingItem = await _dbContext.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    item => item.UserId == userId.Value && item.Id == entityId,
                    cancellationToken);

            if (existingItem is not null && existingItem.LastModifiedAt > change.LastModifiedAt)
            {
                conflicts.Add(new SyncConflictContract
                {
                    ChangeId = change.ChangeId,
                    EntityType = SyncEntityType.Item,
                    EntityId = entityId,
                    Reason = "Server item is newer than client change.",
                    ServerLastModifiedAt = existingItem.LastModifiedAt,
                    ServerItem = existingItem
                });
                continue;
            }

            var normalizedItem = change.ChangeType switch
            {
                SyncChangeType.Delete => BuildDeletedItem(change, existingItem, userId.Value, request.DeviceId),
                _ => NormalizeItem(change.Item!, userId.Value, request.DeviceId)
            };

            _dbContext.Items.Update(normalizedItem);
            _dbContext.SyncChanges.Add(new SyncChange
            {
                Id = change.ChangeId,
                UserId = userId.Value,
                DeviceId = request.DeviceId.Trim(),
                EntityType = SyncEntityType.Item,
                ChangeType = change.ChangeType,
                EntityId = normalizedItem.Id,
                ItemSnapshot = normalizedItem,
                CreatedAt = change.CreatedAt,
                LastModifiedAt = change.LastModifiedAt,
                SyncedAt = now
            });
            appliedChangeCount++;
        }

        if (request.SettingsChange is not null)
        {
            if (request.SettingsChange.ChangeId == Guid.Empty)
            {
                return ValidationProblem("SettingsChange.ChangeId is required.");
            }

            var settingsAlreadyProcessed = await _dbContext.SyncChanges
                .AsNoTracking()
                .AnyAsync(
                    syncChange => syncChange.UserId == userId.Value && syncChange.Id == request.SettingsChange.ChangeId,
                    cancellationToken);

            if (!settingsAlreadyProcessed)
            {
                var existingSettings = await _dbContext.UserSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        settings => settings.UserId == userId.Value,
                        cancellationToken);

                if (existingSettings is not null && existingSettings.LastModifiedAt > request.SettingsChange.LastModifiedAt)
                {
                    conflicts.Add(new SyncConflictContract
                    {
                        ChangeId = request.SettingsChange.ChangeId,
                        EntityType = SyncEntityType.UserSettings,
                        EntityId = existingSettings.Id,
                        Reason = "Server settings are newer than client change.",
                        ServerLastModifiedAt = existingSettings.LastModifiedAt,
                        ServerSettings = existingSettings
                    });
                }
                else
                {
                    var normalizedSettings = NormalizeSettings(
                        request.SettingsChange.Value,
                        existingSettings?.Id,
                        userId.Value,
                        request.DeviceId);

                    _dbContext.UserSettings.Update(normalizedSettings);
                    _dbContext.SyncChanges.Add(new SyncChange
                    {
                        Id = request.SettingsChange.ChangeId,
                        UserId = userId.Value,
                        DeviceId = request.DeviceId.Trim(),
                        EntityType = SyncEntityType.UserSettings,
                        ChangeType = SyncChangeType.Upsert,
                        EntityId = normalizedSettings.Id,
                        SettingsSnapshot = normalizedSettings,
                        CreatedAt = request.SettingsChange.CreatedAt,
                        LastModifiedAt = request.SettingsChange.LastModifiedAt,
                        SyncedAt = now
                    });
                    appliedChangeCount++;
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new SyncPushResponse
        {
            Accepted = true,
            ServerTime = now,
            AppliedChangeCount = appliedChangeCount,
            Conflicts = conflicts
        });
    }

    private ActionResult ValidationProblem(string detail)
    {
        return new BadRequestObjectResult(new ProblemDetails
        {
            Title = "Validation failed.",
            Detail = detail
        });
    }

    private Guid? GetCurrentUserId()
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }

    private static string? ValidateItemChange(SyncItemChangeRequest change)
    {
        if (change.ChangeId == Guid.Empty)
        {
            return "ItemChanges[].ChangeId is required.";
        }

        if (change.LastModifiedAt == default)
        {
            return "ItemChanges[].LastModifiedAt is required.";
        }

        if (change.CreatedAt == default)
        {
            return "ItemChanges[].CreatedAt is required.";
        }

        if (change.ChangeType == SyncChangeType.Upsert && change.Item is null)
        {
            return "ItemChanges[].Item is required for upsert changes.";
        }

        if (change.Item is null && change.EntityId is null)
        {
            return "ItemChanges[].EntityId is required when Item is omitted.";
        }

        return null;
    }

    private static Item NormalizeItem(Item item, Guid userId, string deviceId)
    {
        return new Item
        {
            Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
            UserId = userId,
            Type = item.Type,
            Title = item.Title,
            Description = item.Description,
            Location = item.Location,
            Color = item.Color,
            IsImportant = item.IsImportant,
            IsCompleted = item.IsCompleted,
            ReminderConfig = item.ReminderConfig,
            RepeatRule = item.RepeatRule,
            TimeZoneId = item.TimeZoneId,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            DeletedAt = item.DeletedAt,
            LastModifiedAt = item.LastModifiedAt,
            SourceDeviceId = string.IsNullOrWhiteSpace(item.SourceDeviceId) ? deviceId.Trim() : item.SourceDeviceId,
            StartAt = item.StartAt,
            EndAt = item.EndAt,
            PlannedStartAt = item.PlannedStartAt,
            PlannedEndAt = item.PlannedEndAt,
            DeadlineAt = item.DeadlineAt,
            ExpectedDurationMinutes = item.ExpectedDurationMinutes,
            TargetDate = item.TargetDate
        };
    }

    private static Item BuildDeletedItem(
        SyncItemChangeRequest change,
        Item? existingItem,
        Guid userId,
        string deviceId)
    {
        var entityId = change.Item?.Id ?? change.EntityId!.Value;
        var source = change.Item ?? existingItem;

        return new Item
        {
            Id = entityId,
            UserId = userId,
            Type = source?.Type ?? ItemType.Task,
            Title = source?.Title ?? string.Empty,
            Description = source?.Description,
            Location = source?.Location,
            Color = source?.Color,
            IsImportant = source?.IsImportant ?? false,
            IsCompleted = source?.IsCompleted ?? false,
            ReminderConfig = source?.ReminderConfig ?? new(),
            RepeatRule = source?.RepeatRule ?? new(),
            TimeZoneId = source?.TimeZoneId ?? "UTC",
            CreatedAt = source?.CreatedAt ?? change.CreatedAt,
            UpdatedAt = source?.UpdatedAt ?? change.LastModifiedAt,
            DeletedAt = source?.DeletedAt ?? change.LastModifiedAt,
            LastModifiedAt = change.LastModifiedAt,
            SourceDeviceId = source?.SourceDeviceId ?? deviceId.Trim(),
            StartAt = source?.StartAt,
            EndAt = source?.EndAt,
            PlannedStartAt = source?.PlannedStartAt,
            PlannedEndAt = source?.PlannedEndAt,
            DeadlineAt = source?.DeadlineAt,
            ExpectedDurationMinutes = source?.ExpectedDurationMinutes,
            TargetDate = source?.TargetDate
        };
    }

    private static UserSettings NormalizeSettings(
        UserSettings settings,
        Guid? existingId,
        Guid userId,
        string deviceId)
    {
        return new UserSettings
        {
            Id = existingId ?? (settings.Id == Guid.Empty ? Guid.NewGuid() : settings.Id),
            UserId = userId,
            Language = settings.Language,
            ThemeMode = settings.ThemeMode,
            ThemePreset = settings.ThemePreset,
            WeekStartDay = settings.WeekStartDay,
            HomeViewMode = settings.HomeViewMode,
            DayPlanStartTime = settings.DayPlanStartTime,
            TimeBlockDurationMinutes = settings.TimeBlockDurationMinutes,
            TimeBlockGapMinutes = settings.TimeBlockGapMinutes,
            TimeBlockCount = settings.TimeBlockCount,
            ListPageDefaultTab = settings.ListPageDefaultTab,
            ListPageSortBy = settings.ListPageSortBy,
            ListPageTheme = settings.ListPageTheme,
            AiBaseUrl = settings.AiBaseUrl,
            AiApiKey = settings.AiApiKey,
            AiModel = settings.AiModel,
            SyncServerBaseUrl = settings.SyncServerBaseUrl,
            NotificationEnabled = settings.NotificationEnabled,
            WidgetPreferences = settings.WidgetPreferences,
            TimeZoneId = settings.TimeZoneId,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt,
            LastModifiedAt = settings.LastModifiedAt,
            SourceDeviceId = string.IsNullOrWhiteSpace(settings.SourceDeviceId) ? deviceId.Trim() : settings.SourceDeviceId
        };
    }
}
