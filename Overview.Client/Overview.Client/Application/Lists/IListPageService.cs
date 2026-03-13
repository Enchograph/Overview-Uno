using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Lists;

public interface IListPageService
{
    Task<ListPageSnapshot> BuildSnapshotAsync(
        Guid userId,
        ListPageQuery? query = null,
        CancellationToken cancellationToken = default);

    Task<UserSettings> SetSortByAsync(
        Guid userId,
        ListSortBy sortBy,
        CancellationToken cancellationToken = default);

    Task<UserSettings> ReorderAsync(
        Guid userId,
        ListPageTab tab,
        IReadOnlyList<Guid> orderedItemIds,
        CancellationToken cancellationToken = default);
}
