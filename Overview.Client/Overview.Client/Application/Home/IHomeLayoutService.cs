using System.Globalization;

namespace Overview.Client.Application.Home;

public interface IHomeLayoutService
{
    Task<HomeLayoutSnapshot> BuildSnapshotAsync(
        Guid userId,
        DateOnly referenceDate,
        Domain.Enums.HomeViewMode? viewMode = null,
        CultureInfo? culture = null,
        CancellationToken cancellationToken = default);
}
