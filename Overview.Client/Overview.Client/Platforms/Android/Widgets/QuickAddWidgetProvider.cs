using Overview.Client.Application.Navigation;
using Overview.Client.Domain.Enums;
using Overview.Client.Infrastructure.Widgets;

namespace Overview.Client.Droid.Widgets;

[global::Android.Content.BroadcastReceiver(Enabled = true, Exported = false, Label = "Overview Quick Add Widget")]
[global::Android.App.IntentFilter([global::Android.Appwidget.AppWidgetManager.ActionAppwidgetUpdate])]
[global::Android.App.MetaData("android.appwidget.provider", Resource = "@xml/overview_quick_add_widget_info")]
public sealed class QuickAddWidgetProvider : OverviewWidgetProviderBase
{
    protected override WidgetKind Kind => WidgetKind.QuickAdd;

    protected override string DefaultTitle => "Quick Add";

    protected override string DefaultSubtitle => "Tap to create a task.";

    protected override string DefaultDeepLink => AppNavigationRequest.CreateDeepLink(AppNavigationTarget.Add, ItemType.Task);
}
