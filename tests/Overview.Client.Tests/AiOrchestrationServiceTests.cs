using Overview.Client.Application.Ai;
using Overview.Client.Application.Items;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Tests;

public sealed class AiOrchestrationServiceTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void ParseResponse_CreateNote_ParsesExtendedFields()
    {
        var service = CreateService();

        var result = service.ParseResponse(
            """
            {
              "intent": "create_item",
              "itemType": "note",
              "title": "Pack samples",
              "expectedDurationMinutes": 45,
              "targetDate": "2026-03-14",
              "color": "#336699",
              "confidence": 0.92,
              "answer": "Created note."
            }
            """);

        Assert.Empty(result.ValidationErrors);
        Assert.NotNull(result.Response);
        Assert.Equal(AiRequestType.CreateItem, result.Response!.Intent);
        Assert.Equal(ItemType.Note, result.Response.ItemType);
        Assert.Equal(45, result.Response.ExpectedDurationMinutes);
        Assert.Equal(new DateOnly(2026, 3, 14), result.Response.TargetDate);
        Assert.Equal("#336699", result.Response.Color);
        Assert.True(result.CanApplyWriteOperation);
    }

    [Fact]
    public void ParseResponse_DeleteWithoutItemIds_ReturnsValidationError()
    {
        var service = CreateService();

        var result = service.ParseResponse(
            """
            {
              "intent": "delete_item",
              "confidence": 0.91,
              "answer": "Deleted it."
            }
            """);

        Assert.Contains(result.ValidationErrors, error => error.Contains("itemIds", StringComparison.Ordinal));
        Assert.False(result.CanApplyWriteOperation);
    }

    [Fact]
    public void ParseResponse_QueryItems_ParsesItemIds()
    {
        var service = CreateService();
        var firstItemId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var secondItemId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var result = service.ParseResponse(
            $$"""
            {
              "intent": "query_items",
              "itemIds": ["{{firstItemId}}", "{{secondItemId}}"],
              "confidence": 0.87,
              "answer": "You have two matching items."
            }
            """);

        Assert.Empty(result.ValidationErrors);
        Assert.Equal([firstItemId, secondItemId], result.Response!.ItemIds);
    }

    private static AiOrchestrationService CreateService()
    {
        return new AiOrchestrationService(new NoOpItemService(), new FakeUserSettingsService());
    }

    private sealed class NoOpItemService : IItemService
    {
        public Task<Item?> GetAsync(Guid userId, Guid itemId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Item>> ListAsync(Guid userId, ItemQueryOptions? options = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Item>>(Array.Empty<Item>());
        }

        public Task<Item> CreateAsync(Guid userId, ItemUpsertRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Item> UpdateAsync(Guid userId, Guid itemId, ItemUpsertRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Item> SetCompletedAsync(Guid userId, Guid itemId, bool isCompleted, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Item> SetImportantAsync(Guid userId, Guid itemId, bool isImportant, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeUserSettingsService : IUserSettingsService
    {
        public Task<UserSettings> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new UserSettings
            {
                UserId = UserId,
                TimeZoneId = "Asia/Shanghai",
                SourceDeviceId = "device"
            });
        }

        public Task<UserSettings> SaveAsync(Guid userId, UserSettingsUpdateRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
