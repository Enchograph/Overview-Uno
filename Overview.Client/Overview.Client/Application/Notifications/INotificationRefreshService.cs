namespace Overview.Client.Application.Notifications;

public interface INotificationRefreshService
{
    Task RefreshAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed class NoOpNotificationRefreshService : INotificationRefreshService
{
    public static NoOpNotificationRefreshService Instance { get; } = new();

    private NoOpNotificationRefreshService()
    {
    }

    public Task RefreshAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
