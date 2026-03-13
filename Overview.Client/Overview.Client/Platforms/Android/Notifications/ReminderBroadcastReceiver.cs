using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Overview.Client.Droid;

namespace Overview.Client.Infrastructure.Notifications;

[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter([AndroidNotificationScheduler.ReminderAction])]
public sealed class ReminderBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent?.Action != AndroidNotificationScheduler.ReminderAction)
        {
            return;
        }

        AndroidNotificationScheduler.EnsureChannel(context);

        var notificationId = intent.GetStringExtra(AndroidNotificationScheduler.ExtraNotificationId);
        if (string.IsNullOrWhiteSpace(notificationId))
        {
            return;
        }

        var title = intent.GetStringExtra(AndroidNotificationScheduler.ExtraTitle) ?? "Overview";
        var body = intent.GetStringExtra(AndroidNotificationScheduler.ExtraBody) ?? string.Empty;
        var launchIntent = new Intent(context, typeof(MainActivity));
        launchIntent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);

        var contentIntent = PendingIntent.GetActivity(
            context,
            AndroidNotificationScheduler.GetRequestCode($"{notificationId}:open"),
            launchIntent,
            BuildPendingIntentFlags());

        var notification = new NotificationCompat.Builder(context, AndroidNotificationScheduler.ChannelId)
            .SetSmallIcon(Resource.Mipmap.icon)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
            .SetAutoCancel(true)
            .SetPriority((int)NotificationPriority.Default)
            .SetContentIntent(contentIntent)
            .Build();

        NotificationManagerCompat.From(context).Notify(
            AndroidNotificationScheduler.GetRequestCode(notificationId),
            notification);
    }

    internal static PendingIntent CreatePendingIntent(
        Context context,
        string notificationId,
        string? title,
        string? body,
        Guid? itemId)
    {
        var intent = new Intent(context, typeof(ReminderBroadcastReceiver));
        intent.SetAction(AndroidNotificationScheduler.ReminderAction);
        intent.PutExtra(AndroidNotificationScheduler.ExtraNotificationId, notificationId);

        if (!string.IsNullOrWhiteSpace(title))
        {
            intent.PutExtra(AndroidNotificationScheduler.ExtraTitle, title);
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            intent.PutExtra(AndroidNotificationScheduler.ExtraBody, body);
        }

        if (itemId is Guid value)
        {
            intent.PutExtra(AndroidNotificationScheduler.ExtraItemId, value.ToString("N"));
        }

        return PendingIntent.GetBroadcast(
            context,
            AndroidNotificationScheduler.GetRequestCode(notificationId),
            intent,
            BuildPendingIntentFlags())!;
    }

    private static PendingIntentFlags BuildPendingIntentFlags()
    {
        return OperatingSystem.IsAndroidVersionAtLeast(23)
            ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
            : PendingIntentFlags.UpdateCurrent;
    }
}
