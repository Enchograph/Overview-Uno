using Overview.Client.Domain.Entities;

namespace Overview.Client.Infrastructure.Persistence.Repositories;

public interface IItemRepository
{
    Task<Item?> GetAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Item>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UpsertAsync(Item item, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
}
