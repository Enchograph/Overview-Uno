using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Infrastructure.Notifications;

public interface INotificationScheduler
{
    Task ScheduleAsync(
        IReadOnlyCollection<NotificationScheduleRequest> requests,
        CancellationToken cancellationToken);

    Task CancelAsync(
        IReadOnlyCollection<string> notificationIds,
        CancellationToken cancellationToken);

    Task CancelAllAsync(CancellationToken cancellationToken);
}

public sealed record NotificationScheduleRequest
{
    public string NotificationId { get; init; } = string.Empty;

    public Guid ItemId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Body { get; init; }

    public ScheduledReminder Reminder { get; init; } = new();
}
