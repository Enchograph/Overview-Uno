using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Views;
using Android.Widget;
using Overview.Client.Application.Navigation;
using Overview.Client.Infrastructure.Widgets;

namespace Overview.Client.Droid.Widgets;

public abstract class OverviewWidgetProviderBase : AppWidgetProvider
{
    protected abstract WidgetKind Kind { get; }

    protected abstract string DefaultTitle { get; }

    protected abstract string DefaultSubtitle { get; }

    protected abstract string DefaultDeepLink { get; }

    public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
    {
        if (context is null || appWidgetManager is null || appWidgetIds is null)
        {
            return;
        }

        UpdateWidgets(context, appWidgetManager, appWidgetIds, Kind);
    }

    internal static void UpdateWidgets(
        Context context,
        AppWidgetManager appWidgetManager,
        int[] appWidgetIds,
        WidgetKind kind)
    {
        if (appWidgetIds.Length == 0)
        {
            return;
        }

        var provider = ResolveProvider(kind);
        var store = new FileWidgetSnapshotStore();
        var snapshot = store.GetAsync(kind, CancellationToken.None).GetAwaiter().GetResult();
        var remoteViews = BuildRemoteViews(context, provider, snapshot);
        appWidgetManager.UpdateAppWidget(appWidgetIds, remoteViews);
    }

    private static RemoteViews BuildRemoteViews(
        Context context,
        OverviewWidgetProviderBase provider,
        WidgetSnapshot? snapshot)
    {
        var views = new RemoteViews(context.PackageName, Resource.Layout.overview_widget);
        var effectiveSnapshot = snapshot ?? new WidgetSnapshot
        {
            Kind = provider.Kind,
            Title = provider.DefaultTitle,
            Subtitle = provider.DefaultSubtitle,
            DeepLink = provider.DefaultDeepLink,
            GeneratedAt = DateTimeOffset.UtcNow,
            Entries = []
        };

        views.SetTextViewText(Resource.Id.widget_title, effectiveSnapshot.Title);
        views.SetTextViewText(Resource.Id.widget_subtitle, effectiveSnapshot.Subtitle ?? string.Empty);
        views.SetViewVisibility(
            Resource.Id.widget_subtitle,
            string.IsNullOrWhiteSpace(effectiveSnapshot.Subtitle) ? ViewStates.Gone : ViewStates.Visible);

        BindEntry(views, Resource.Id.widget_row_1, effectiveSnapshot.Entries.ElementAtOrDefault(0));
        BindEntry(views, Resource.Id.widget_row_2, effectiveSnapshot.Entries.ElementAtOrDefault(1));
        BindEntry(views, Resource.Id.widget_row_3, effectiveSnapshot.Entries.ElementAtOrDefault(2));
        BindEntry(views, Resource.Id.widget_row_4, effectiveSnapshot.Entries.ElementAtOrDefault(3));

        var launchIntent = new Intent(context, typeof(MainActivity));
        launchIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        launchIntent.PutExtra(AndroidWidgetConstants.DeepLinkExtraKey, effectiveSnapshot.DeepLink ?? provider.DefaultDeepLink);
        var pendingIntent = PendingIntent.GetActivity(
            context,
            (int)provider.Kind + 3000,
            launchIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        views.SetOnClickPendingIntent(Resource.Id.widget_root, pendingIntent);

        return views;
    }

    private static void BindEntry(RemoteViews views, int viewId, WidgetSnapshotEntry? entry)
    {
        if (entry is null)
        {
            views.SetViewVisibility(viewId, ViewStates.Gone);
            return;
        }

        var line = entry.Title;
        if (!string.IsNullOrWhiteSpace(entry.BadgeText))
        {
            line = $"{line}  [{entry.BadgeText}]";
        }

        if (!string.IsNullOrWhiteSpace(entry.Subtitle))
        {
            line = $"{line}\n{entry.Subtitle}";
        }

        views.SetTextViewText(viewId, line);
        views.SetViewVisibility(viewId, ViewStates.Visible);
    }

    private static OverviewWidgetProviderBase ResolveProvider(WidgetKind kind)
    {
        return kind switch
        {
            WidgetKind.Home => new HomeWidgetProvider(),
            WidgetKind.List => new ListWidgetProvider(),
            WidgetKind.AiShortcut => new AiShortcutWidgetProvider(),
            WidgetKind.QuickAdd => new QuickAddWidgetProvider(),
            _ => new HomeWidgetProvider()
        };
    }
}
