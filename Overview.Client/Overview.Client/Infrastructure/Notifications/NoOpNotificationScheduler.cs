namespace Overview.Client.Infrastructure.Notifications;

public sealed class NoOpNotificationScheduler : INotificationScheduler
{
    public Task ScheduleAsync(
        IReadOnlyCollection<NotificationScheduleRequest> requests,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task CancelAsync(
        IReadOnlyCollection<string> notificationIds,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task CancelAllAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
