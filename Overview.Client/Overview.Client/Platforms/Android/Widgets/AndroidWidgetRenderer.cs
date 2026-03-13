using Android.Appwidget;
using Android.Content;
using Java.Lang;
using Overview.Client.Infrastructure.Widgets;

namespace Overview.Client.Droid.Widgets;

public sealed class AndroidWidgetRenderer : IWidgetRenderer
{
    public Task RenderAsync(WidgetSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        RequestUpdate(snapshot.Kind);
        return Task.CompletedTask;
    }

    public Task ClearAsync(WidgetKind kind, CancellationToken cancellationToken = default)
    {
        RequestUpdate(kind);
        return Task.CompletedTask;
    }

    private static void RequestUpdate(WidgetKind kind)
    {
        var context = Application.Context;
        var appWidgetManager = AppWidgetManager.GetInstance(context);
        var providerClass = ResolveProviderClass(kind);
        var appWidgetIds = appWidgetManager.GetAppWidgetIds(new ComponentName(context, providerClass));
        if (appWidgetIds.Length == 0)
        {
            return;
        }

        OverviewWidgetProviderBase.UpdateWidgets(context, appWidgetManager, appWidgetIds, kind);
    }

    private static Class ResolveProviderClass(WidgetKind kind)
    {
        return kind switch
        {
            WidgetKind.Home => Class.FromType(typeof(HomeWidgetProvider)),
            WidgetKind.List => Class.FromType(typeof(ListWidgetProvider)),
            WidgetKind.AiShortcut => Class.FromType(typeof(AiShortcutWidgetProvider)),
            WidgetKind.QuickAdd => Class.FromType(typeof(QuickAddWidgetProvider)),
            _ => Class.FromType(typeof(HomeWidgetProvider))
        };
    }
}
