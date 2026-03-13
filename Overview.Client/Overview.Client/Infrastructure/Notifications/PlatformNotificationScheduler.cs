namespace Overview.Client.Infrastructure.Notifications;

public sealed class PlatformNotificationScheduler : INotificationScheduler
{
#if __ANDROID__
    private readonly INotificationScheduler inner = new AndroidNotificationScheduler();
#else
    private readonly INotificationScheduler inner = new NoOpNotificationScheduler();
#endif

    public Task ScheduleAsync(
        IReadOnlyCollection<NotificationScheduleRequest> requests,
        CancellationToken cancellationToken)
    {
        return inner.ScheduleAsync(requests, cancellationToken);
    }

    public Task CancelAsync(
        IReadOnlyCollection<string> notificationIds,
        CancellationToken cancellationToken)
    {
        return inner.CancelAsync(notificationIds, cancellationToken);
    }

    public Task CancelAllAsync(CancellationToken cancellationToken)
    {
        return inner.CancelAllAsync(cancellationToken);
    }
}
