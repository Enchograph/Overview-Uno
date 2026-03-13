using Overview.Client.Domain.Entities;

namespace Overview.Client.Application.Items;

public interface IItemService
{
    Task<Item?> GetAsync(
        Guid userId,
        Guid itemId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Item>> ListAsync(
        Guid userId,
        ItemQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<Item> CreateAsync(
        Guid userId,
        ItemUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<Item> UpdateAsync(
        Guid userId,
        Guid itemId,
        ItemUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<Item> SetCompletedAsync(
        Guid userId,
        Guid itemId,
        bool isCompleted,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid userId,
        Guid itemId,
        CancellationToken cancellationToken = default);
}
