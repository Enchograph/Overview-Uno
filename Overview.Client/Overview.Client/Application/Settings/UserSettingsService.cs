using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Settings;

namespace Overview.Client.Application.Settings;

public sealed class UserSettingsService : IUserSettingsService
{
    private readonly IUserSettingsRepository userSettingsRepository;
    private readonly ISyncChangeRepository syncChangeRepository;
    private readonly IDeviceIdStore deviceIdStore;

    public UserSettingsService(
        IUserSettingsRepository userSettingsRepository,
        ISyncChangeRepository syncChangeRepository,
        IDeviceIdStore deviceIdStore)
    {
        this.userSettingsRepository = userSettingsRepository;
        this.syncChangeRepository = syncChangeRepository;
        this.deviceIdStore = deviceIdStore;
    }

    public async Task<UserSettings> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var existingSettings = await userSettingsRepository.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        if (existingSettings is not null)
        {
            return existingSettings;
        }

        return new UserSettings
        {
            UserId = userId,
            TimeZoneId = ResolveTimeZoneId(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow,
            SourceDeviceId = await deviceIdStore.GetOrCreateAsync(cancellationToken).ConfigureAwait(false)
        };
    }

    public async Task<UserSettings> SaveAsync(
        Guid userId,
        UserSettingsUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        var existingSettings = await userSettingsRepository.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
        var settings = new UserSettings
        {
            Id = existingSettings?.Id ?? Guid.NewGuid(),
            UserId = userId,
            Language = NormalizeLanguage(request.Language),
            ThemeMode = request.ThemeMode,
            ThemePreset = NormalizeTextOrDefault(request.ThemePreset, "default"),
            WeekStartDay = request.WeekStartDay,
            HomeViewMode = request.HomeViewMode,
            DayPlanStartTime = request.DayPlanStartTime,
            TimeBlockDurationMinutes = request.TimeBlockDurationMinutes,
            TimeBlockGapMinutes = request.TimeBlockGapMinutes,
            TimeBlockCount = request.TimeBlockCount,
            ListPageDefaultTab = request.ListPageDefaultTab,
            ListPageSortBy = request.ListPageSortBy,
            ListPageTheme = NormalizeTextOrDefault(request.ListPageTheme, "default"),
            AiBaseUrl = NormalizeOptional(request.AiBaseUrl) ?? string.Empty,
            AiApiKey = NormalizeOptional(request.AiApiKey) ?? string.Empty,
            AiModel = NormalizeOptional(request.AiModel) ?? string.Empty,
            SyncServerBaseUrl = NormalizeOptional(request.SyncServerBaseUrl) ?? string.Empty,
            NotificationEnabled = request.NotificationEnabled,
            WidgetPreferences = request.WidgetPreferences,
            TimeZoneId = NormalizeTextOrDefault(request.TimeZoneId, ResolveTimeZoneId()),
            CreatedAt = existingSettings?.CreatedAt ?? now,
            UpdatedAt = now,
            LastModifiedAt = now,
            SourceDeviceId = await deviceIdStore.GetOrCreateAsync(cancellationToken).ConfigureAwait(false)
        };

        await userSettingsRepository.UpsertAsync(settings, cancellationToken).ConfigureAwait(false);
        await syncChangeRepository.UpsertAsync(new SyncChange
        {
            UserId = userId,
            DeviceId = settings.SourceDeviceId,
            EntityType = SyncEntityType.UserSettings,
            ChangeType = SyncChangeType.Upsert,
            EntityId = settings.Id,
            SettingsSnapshot = settings,
            CreatedAt = settings.CreatedAt,
            LastModifiedAt = settings.LastModifiedAt
        }, cancellationToken).ConfigureAwait(false);

        return settings;
    }

    private static void ValidateRequest(UserSettingsUpdateRequest request)
    {
        if (request.TimeBlockDurationMinutes <= 0)
        {
            throw new ArgumentException("Time block duration must be greater than zero.", nameof(request));
        }

        if (request.TimeBlockGapMinutes < 0)
        {
            throw new ArgumentException("Time block gap cannot be negative.", nameof(request));
        }

        if (request.TimeBlockCount <= 0)
        {
            throw new ArgumentException("Time block count must be greater than zero.", nameof(request));
        }
    }

    private static string NormalizeLanguage(string language)
    {
        return NormalizeTextOrDefault(language, "zh-CN");
    }

    private static string NormalizeTextOrDefault(string? value, string defaultValue)
    {
        return NormalizeOptional(value) ?? defaultValue;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string ResolveTimeZoneId()
    {
        return TimeZoneInfo.Local.Id;
    }
}
