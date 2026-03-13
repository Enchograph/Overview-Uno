namespace Overview.Client.Application.Widgets;

public interface IWidgetRefreshService
{
    Task RefreshAsync(Guid userId, CancellationToken cancellationToken = default);

    Task ClearAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed class NoOpWidgetRefreshService : IWidgetRefreshService
{
    public static NoOpWidgetRefreshService Instance { get; } = new();

    public Task RefreshAsync(Guid userId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ClearAsync(Guid userId, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
