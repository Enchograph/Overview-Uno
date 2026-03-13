using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Settings;

namespace Overview.Client.Application.Items;

public sealed class ItemService : IItemService
{
    private readonly IItemRepository itemRepository;
    private readonly ISyncChangeRepository syncChangeRepository;
    private readonly IDeviceIdStore deviceIdStore;

    public ItemService(
        IItemRepository itemRepository,
        ISyncChangeRepository syncChangeRepository,
        IDeviceIdStore deviceIdStore)
    {
        this.itemRepository = itemRepository;
        this.syncChangeRepository = syncChangeRepository;
        this.deviceIdStore = deviceIdStore;
    }

    public async Task<Item?> GetAsync(
        Guid userId,
        Guid itemId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var item = await itemRepository.GetAsync(userId, itemId, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            return null;
        }

        if (!includeDeleted && item.DeletedAt is not null)
        {
            return null;
        }

        return item;
    }

    public async Task<IReadOnlyList<Item>> ListAsync(
        Guid userId,
        ItemQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var items = await itemRepository.ListByUserAsync(userId, cancellationToken).ConfigureAwait(false);
        options ??= new ItemQueryOptions();

        return items
            .Where(item => options.IncludeDeleted || item.DeletedAt is null)
            .Where(item => options.Type is null || item.Type == options.Type)
            .OrderByDescending(item => item.LastModifiedAt)
            .ToArray();
    }

    public async Task<Item> CreateAsync(
        Guid userId,
        ItemUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = DateTimeOffset.UtcNow;
        var item = await BuildItemAsync(
            userId,
            request,
            itemId: Guid.NewGuid(),
            createdAt: now,
            cancellationToken).ConfigureAwait(false);

        await PersistItemAsync(item, SyncChangeType.Upsert, cancellationToken).ConfigureAwait(false);
        return item;
    }

    public async Task<Item> UpdateAsync(
        Guid userId,
        Guid itemId,
        ItemUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingItem = await itemRepository.GetAsync(userId, itemId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Item {itemId} was not found.");

        var item = await BuildItemAsync(
            userId,
            request,
            itemId,
            existingItem.CreatedAt,
            cancellationToken).ConfigureAwait(false);

        await PersistItemAsync(item, SyncChangeType.Upsert, cancellationToken).ConfigureAwait(false);
        return item;
    }

    public async Task<Item> SetCompletedAsync(
        Guid userId,
        Guid itemId,
        bool isCompleted,
        CancellationToken cancellationToken = default)
    {
        var existingItem = await itemRepository.GetAsync(userId, itemId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Item {itemId} was not found.");

        if (existingItem.DeletedAt is not null)
        {
            throw new InvalidOperationException("Deleted items cannot be updated.");
        }

        if (existingItem.IsCompleted == isCompleted)
        {
            return existingItem;
        }

        var now = DateTimeOffset.UtcNow;
        var updatedItem = new Item
        {
            Id = existingItem.Id,
            UserId = existingItem.UserId,
            Type = existingItem.Type,
            Title = existingItem.Title,
            Description = existingItem.Description,
            Location = existingItem.Location,
            Color = existingItem.Color,
            IsImportant = existingItem.IsImportant,
            IsCompleted = isCompleted,
            ReminderConfig = existingItem.ReminderConfig,
            RepeatRule = existingItem.RepeatRule,
            TimeZoneId = existingItem.TimeZoneId,
            CreatedAt = existingItem.CreatedAt,
            UpdatedAt = now,
            DeletedAt = existingItem.DeletedAt,
            LastModifiedAt = now,
            SourceDeviceId = await deviceIdStore.GetOrCreateAsync(cancellationToken).ConfigureAwait(false),
            StartAt = existingItem.StartAt,
            EndAt = existingItem.EndAt,
            PlannedStartAt = existingItem.PlannedStartAt,
            PlannedEndAt = existingItem.PlannedEndAt,
            DeadlineAt = existingItem.DeadlineAt,
            ExpectedDurationMinutes = existingItem.ExpectedDurationMinutes,
            TargetDate = existingItem.TargetDate
        };
        await PersistItemAsync(updatedItem, SyncChangeType.Upsert, cancellationToken).ConfigureAwait(false);
        return updatedItem;
    }

    public async Task<Item> SetImportantAsync(
        Guid userId,
        Guid itemId,
        bool isImportant,
        CancellationToken cancellationToken = default)
    {
        var existingItem = await itemRepository.GetAsync(userId, itemId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Item {itemId} was not found.");

        if (existingItem.DeletedAt is not null)
        {
            throw new InvalidOperationException("Deleted items cannot be updated.");
        }

        if (existingItem.IsImportant == isImportant)
        {
            return existingItem;
        }

        var now = DateTimeOffset.UtcNow;
        var updatedItem = new Item
        {
            Id = existingItem.Id,
            UserId = existingItem.UserId,
            Type = existingItem.Type,
            Title = existingItem.Title,
            Description = existingItem.Description,
            Location = existingItem.Location,
            Color = existingItem.Color,
            IsImportant = isImportant,
            IsCompleted = existingItem.IsCompleted,
            ReminderConfig = existingItem.ReminderConfig,
            RepeatRule = existingItem.RepeatRule,
            TimeZoneId = existingItem.TimeZoneId,
            CreatedAt = existingItem.CreatedAt,
            UpdatedAt = now,
            DeletedAt = existingItem.DeletedAt,
            LastModifiedAt = now,
            SourceDeviceId = await deviceIdStore.GetOrCreateAsync(cancellationToken).ConfigureAwait(false),
            StartAt = existingItem.StartAt,
            EndAt = existingItem.EndAt,
            PlannedStartAt = existingItem.PlannedStartAt,
            PlannedEndAt = existingItem.PlannedEndAt,
            DeadlineAt = existingItem.DeadlineAt,
            ExpectedDurationMinutes = existingItem.ExpectedDurationMinutes,
            TargetDate = existingItem.TargetDate
        };

        await PersistItemAsync(updatedItem, SyncChangeType.Upsert, cancellationToken).ConfigureAwait(false);
        return updatedItem;
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var existingItem = await itemRepository.GetAsync(userId, itemId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Item {itemId} was not found.");

        if (existingItem.DeletedAt is not null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var deletedItem = new Item
        {
            Id = existingItem.Id,
            UserId = existingItem.UserId,
            Type = existingItem.Type,
            Title = existingItem.Title,
            Description = existingItem.Description,
            Location = existingItem.Location,
            Color = existingItem.Color,
            IsImportant = existingItem.IsImportant,
            IsCompleted = existingItem.IsCompleted,
            ReminderConfig = existingItem.ReminderConfig,
            RepeatRule = existingItem.RepeatRule,
            TimeZoneId = existingItem.TimeZoneId,
            CreatedAt = existingItem.CreatedAt,
            UpdatedAt = now,
            DeletedAt = now,
            LastModifiedAt = now,
            SourceDeviceId = await deviceIdStore.GetOrCreateAsync(cancellationToken).ConfigureAwait(false),
            StartAt = existingItem.StartAt,
            EndAt = existingItem.EndAt,
            PlannedStartAt = existingItem.PlannedStartAt,
            PlannedEndAt = existingItem.PlannedEndAt,
            DeadlineAt = existingItem.DeadlineAt,
            ExpectedDurationMinutes = existingItem.ExpectedDurationMinutes,
            TargetDate = existingItem.TargetDate
        };

        await PersistItemAsync(deletedItem, SyncChangeType.Delete, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Item> BuildItemAsync(
        Guid userId,
        ItemUpsertRequest request,
        Guid itemId,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var now = DateTimeOffset.UtcNow;
        return new Item
        {
            Id = itemId,
            UserId = userId,
            Type = request.Type,
            Title = request.Title.Trim(),
            Description = NormalizeOptional(request.Description),
            Location = NormalizeOptional(request.Location),
            Color = NormalizeOptional(request.Color),
            IsImportant = request.IsImportant,
            IsCompleted = request.IsCompleted,
            ReminderConfig = request.ReminderConfig,
            RepeatRule = request.RepeatRule,
            TimeZoneId = NormalizeTimeZoneId(request.TimeZoneId),
            CreatedAt = createdAt,
            UpdatedAt = now,
            DeletedAt = null,
            LastModifiedAt = now,
            SourceDeviceId = await deviceIdStore.GetOrCreateAsync(cancellationToken).ConfigureAwait(false),
            StartAt = request.Type == ItemType.Schedule ? request.StartAt : null,
            EndAt = request.Type == ItemType.Schedule ? request.EndAt : null,
            PlannedStartAt = request.Type == ItemType.Task ? request.PlannedStartAt : null,
            PlannedEndAt = request.Type == ItemType.Task ? request.PlannedEndAt : null,
            DeadlineAt = request.Type == ItemType.Task ? request.DeadlineAt : null,
            ExpectedDurationMinutes = request.Type == ItemType.Note ? request.ExpectedDurationMinutes : null,
            TargetDate = request.Type == ItemType.Note ? request.TargetDate : null
        };
    }

    private async Task PersistItemAsync(
        Item item,
        SyncChangeType changeType,
        CancellationToken cancellationToken)
    {
        await itemRepository.UpsertAsync(item, cancellationToken).ConfigureAwait(false);
        await syncChangeRepository.UpsertAsync(new SyncChange
        {
            UserId = item.UserId,
            DeviceId = item.SourceDeviceId,
            EntityType = SyncEntityType.Item,
            ChangeType = changeType,
            EntityId = item.Id,
            ItemSnapshot = item,
            CreatedAt = item.CreatedAt,
            LastModifiedAt = item.LastModifiedAt
        }, cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateRequest(ItemUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Item title is required.", nameof(request));
        }

        switch (request.Type)
        {
            case ItemType.Schedule:
                RequireTimeRange(request.StartAt, request.EndAt, "schedule");
                break;
            case ItemType.Task:
                RequireTimeRange(request.PlannedStartAt, request.PlannedEndAt, "task");
                if (request.DeadlineAt is null)
                {
                    throw new ArgumentException("Task deadline is required.", nameof(request));
                }

                if (request.DeadlineAt < request.PlannedStartAt)
                {
                    throw new ArgumentException("Task deadline must be after the planned start time.", nameof(request));
                }

                break;
            case ItemType.Note:
                if (request.ExpectedDurationMinutes is null || request.ExpectedDurationMinutes <= 0)
                {
                    throw new ArgumentException("Note expected duration must be greater than zero.", nameof(request));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request), request.Type, "Unsupported item type.");
        }
    }

    private static void RequireTimeRange(
        DateTimeOffset? startAt,
        DateTimeOffset? endAt,
        string itemKind)
    {
        if (startAt is null || endAt is null)
        {
            throw new ArgumentException($"The {itemKind} start and end time are required.");
        }

        if (endAt <= startAt)
        {
            throw new ArgumentException($"The {itemKind} end time must be after the start time.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeTimeZoneId(string? timeZoneId)
    {
        return string.IsNullOrWhiteSpace(timeZoneId) ? TimeZoneInfo.Local.Id : timeZoneId.Trim();
    }
}
