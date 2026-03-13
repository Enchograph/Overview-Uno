using Android.App;
using Android.Content;

namespace Overview.Client.Infrastructure.Notifications;

public sealed class AndroidNotificationScheduler : INotificationScheduler
{
    internal const string ChannelId = "overview.reminders";
    internal const string ReminderAction = "com.companyname.overview-client.SHOW_REMINDER";
    internal const string ExtraNotificationId = "notification_id";
    internal const string ExtraTitle = "title";
    internal const string ExtraBody = "body";
    internal const string ExtraItemId = "item_id";

    public Task ScheduleAsync(
        IReadOnlyCollection<NotificationScheduleRequest> requests,
        CancellationToken cancellationToken)
    {
        var context = global::Android.App.Application.Context;
        EnsureChannel(context);
        var alarmManager = GetAlarmManager(context);

        foreach (var request in requests)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pendingIntent = ReminderBroadcastReceiver.CreatePendingIntent(
                context,
                request.NotificationId,
                request.Title,
                request.Body,
                request.ItemId);

            alarmManager.Cancel(pendingIntent);
            alarmManager.SetAndAllowWhileIdle(
                AlarmType.RtcWakeup,
                request.Reminder.TriggerAt.ToUnixTimeMilliseconds(),
                pendingIntent);
        }

        return Task.CompletedTask;
    }

    public Task CancelAsync(
        IReadOnlyCollection<string> notificationIds,
        CancellationToken cancellationToken)
    {
        var context = global::Android.App.Application.Context;
        var alarmManager = GetAlarmManager(context);
        var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

        foreach (var notificationId in notificationIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pendingIntent = ReminderBroadcastReceiver.CreatePendingIntent(context, notificationId, null, null, null);
            alarmManager.Cancel(pendingIntent);
            pendingIntent.Cancel();
            notificationManager?.Cancel(GetRequestCode(notificationId));
        }

        return Task.CompletedTask;
    }

    public Task CancelAllAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    internal static int GetRequestCode(string notificationId)
    {
        unchecked
        {
            var hash = 17;
            foreach (var character in notificationId)
            {
                hash = (hash * 31) + character;
            }

            return hash == int.MinValue ? 0 : Math.Abs(hash);
        }
    }

    internal static void EnsureChannel(Context context)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
            if (notificationManager?.GetNotificationChannel(ChannelId) is not null)
            {
                return;
            }

            var channel = new NotificationChannel(
                ChannelId,
                "Overview reminders",
                NotificationImportance.Default)
            {
                Description = "Local reminders for scheduled items."
            };

            notificationManager?.CreateNotificationChannel(channel);
        }
    }

    private static AlarmManager GetAlarmManager(Context context)
    {
        return (context.GetSystemService(Context.AlarmService) as AlarmManager)
            ?? throw new InvalidOperationException("Alarm manager is unavailable.");
    }
}
