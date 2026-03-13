using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.Rules;
using Overview.Client.Infrastructure.Notifications;
using Overview.Client.Infrastructure.Persistence.Repositories;

namespace Overview.Client.Application.Notifications;

public sealed class NotificationRefreshService : INotificationRefreshService
{
    private static readonly TimeSpan SchedulingWindow = TimeSpan.FromDays(30);

    private readonly IItemRepository itemRepository;
    private readonly IUserSettingsRepository userSettingsRepository;
    private readonly IReminderRuleService reminderRuleService;
    private readonly INotificationScheduler notificationScheduler;
    private readonly INotificationStateStore notificationStateStore;
    private readonly TimeProvider timeProvider;

    public NotificationRefreshService(
        IItemRepository itemRepository,
        IUserSettingsRepository userSettingsRepository,
        IReminderRuleService reminderRuleService,
        INotificationScheduler notificationScheduler,
        INotificationStateStore notificationStateStore,
        TimeProvider timeProvider)
    {
        this.itemRepository = itemRepository;
        this.userSettingsRepository = userSettingsRepository;
        this.reminderRuleService = reminderRuleService;
        this.notificationScheduler = notificationScheduler;
        this.notificationStateStore = notificationStateStore;
        this.timeProvider = timeProvider;
    }

    public async Task RefreshAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var trackedIds = await notificationStateStore.LoadAsync(userId, cancellationToken).ConfigureAwait(false);
        var settings = await userSettingsRepository.GetAsync(userId, cancellationToken).ConfigureAwait(false);

        if (settings is null || !settings.NotificationEnabled)
        {
            await CancelTrackedAsync(userId, trackedIds, cancellationToken).ConfigureAwait(false);
            return;
        }

        var now = timeProvider.GetUtcNow();
        var rangeEnd = now.Add(SchedulingWindow);
        var items = await itemRepository.ListByUserAsync(userId, cancellationToken).ConfigureAwait(false);

        var desiredRequests = items
            .Where(IsNotificationCandidate)
            .SelectMany(item => BuildRequests(item, now, rangeEnd))
            .GroupBy(request => request.NotificationId, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();

        var desiredIds = desiredRequests
            .Select(request => request.NotificationId)
            .ToHashSet(StringComparer.Ordinal);

        var staleIds = trackedIds
            .Where(notificationId => !desiredIds.Contains(notificationId))
            .ToArray();

        if (staleIds.Length > 0)
        {
            await notificationScheduler.CancelAsync(staleIds, cancellationToken).ConfigureAwait(false);
        }

        if (desiredRequests.Length > 0)
        {
            await notificationScheduler.ScheduleAsync(desiredRequests, cancellationToken).ConfigureAwait(false);
        }

        if (desiredIds.Count == 0)
        {
            await notificationStateStore.ClearAsync(userId, cancellationToken).ConfigureAwait(false);
            return;
        }

        await notificationStateStore.SaveAsync(userId, desiredIds, cancellationToken).ConfigureAwait(false);
    }

    private async Task CancelTrackedAsync(
        Guid userId,
        IReadOnlyCollection<string> trackedIds,
        CancellationToken cancellationToken)
    {
        if (trackedIds.Count > 0)
        {
            await notificationScheduler.CancelAsync(trackedIds, cancellationToken).ConfigureAwait(false);
        }

        await notificationStateStore.ClearAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private IEnumerable<NotificationScheduleRequest> BuildRequests(
        Item item,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd)
    {
        foreach (var reminder in reminderRuleService.BuildReminderSchedule(item, rangeStart, rangeEnd))
        {
            if (reminder.Channel != ReminderChannel.Notification)
            {
                continue;
            }

            yield return new NotificationScheduleRequest
            {
                NotificationId = BuildNotificationId(item.UserId, item.Id, reminder),
                ItemId = item.Id,
                Title = item.Title,
                Body = BuildBody(item, reminder),
                Reminder = reminder
            };
        }
    }

    private static bool IsNotificationCandidate(Item item)
    {
        return item.DeletedAt is null &&
            !item.IsCompleted &&
            item.ReminderConfig.IsEnabled;
    }

    private static string BuildNotificationId(Guid userId, Guid itemId, Domain.ValueObjects.ScheduledReminder reminder)
    {
        return $"{userId:N}:{itemId:N}:{reminder.TriggerAt.UtcTicks}:{reminder.MinutesBeforeStart}";
    }

    private static string? BuildBody(Item item, Domain.ValueObjects.ScheduledReminder reminder)
    {
        var localStart = ConvertToLocal(reminder.OccurrenceStartAt, item.TimeZoneId);
        var parts = new List<string>
        {
            localStart.ToString("yyyy-MM-dd HH:mm")
        };

        if (!string.IsNullOrWhiteSpace(item.Location))
        {
            parts.Add(item.Location!);
        }

        return string.Join(" · ", parts);
    }

    private static DateTimeOffset ConvertToLocal(DateTimeOffset value, string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.ConvertTime(value, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }
        catch (TimeZoneNotFoundException)
        {
            return value;
        }
        catch (InvalidTimeZoneException)
        {
            return value;
        }
    }
}
