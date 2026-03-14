using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Infrastructure.Persistence.Options;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Persistence.Services;

namespace Overview.Client.Tests;

public sealed class SqliteRepositoryTests : IDisposable
{
    private readonly string databaseName = $"overview-tests-{Guid.NewGuid():N}.db3";

    [Fact]
    public async Task SqliteRepositories_QueryByGuidWithoutInvokingSqlToStringFunction()
    {
        SQLitePCL.Batteries_V2.Init();

        var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var itemId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var now = new DateTimeOffset(2026, 3, 14, 12, 0, 0, TimeSpan.Zero);
        var factory = CreateFactory();

        var itemRepository = new SqliteItemRepository(factory);
        var settingsRepository = new SqliteUserSettingsRepository(factory);
        var aiRepository = new SqliteAiChatMessageRepository(factory);
        var syncRepository = new SqliteSyncChangeRepository(factory);

        await itemRepository.UpsertAsync(new Item
        {
            Id = itemId,
            UserId = userId,
            Type = ItemType.Task,
            Title = "Offline item",
            CreatedAt = now,
            UpdatedAt = now,
            LastModifiedAt = now,
            SourceDeviceId = "device"
        });

        await settingsRepository.UpsertAsync(new UserSettings
        {
            UserId = userId,
            Language = "en-US",
            TimeZoneId = "UTC",
            CreatedAt = now,
            UpdatedAt = now,
            LastModifiedAt = now,
            SourceDeviceId = "device"
        });

        await aiRepository.UpsertAsync(new AiChatMessage
        {
            UserId = userId,
            OccurredOn = new DateOnly(2026, 3, 14),
            Role = AiChatRole.Assistant,
            Message = "Offline ready",
            CreatedAt = now
        });

        await syncRepository.UpsertAsync(new SyncChange
        {
            UserId = userId,
            DeviceId = "device",
            EntityType = SyncEntityType.Item,
            ChangeType = SyncChangeType.Upsert,
            EntityId = itemId,
            CreatedAt = now,
            LastModifiedAt = now
        });

        var loadedItem = await itemRepository.GetAsync(userId, itemId);
        var loadedItems = await itemRepository.ListByUserAsync(userId);
        var loadedSettings = await settingsRepository.GetAsync(userId);
        var loadedMessages = await aiRepository.ListByDateRangeAsync(userId, new DateOnly(2026, 3, 14), new DateOnly(2026, 3, 14));
        var loadedChanges = await syncRepository.ListPendingAsync(userId);

        Assert.NotNull(loadedItem);
        Assert.Single(loadedItems);
        Assert.NotNull(loadedSettings);
        Assert.Single(loadedMessages);
        Assert.Single(loadedChanges);
    }

    public void Dispose()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), databaseName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private SqliteConnectionFactory CreateFactory()
    {
        return new SqliteConnectionFactory(new ClientSqliteOptions
        {
            DatabaseName = databaseName
        });
    }
}
