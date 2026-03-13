using Overview.Client.Application.Navigation;
using Overview.Client.Infrastructure.Widgets;

namespace Overview.Client.Droid.Widgets;

[global::Android.Content.BroadcastReceiver(Enabled = true, Exported = false, Label = "Overview List Widget")]
[global::Android.App.IntentFilter([global::Android.Appwidget.AppWidgetManager.ActionAppwidgetUpdate])]
[global::Android.App.MetaData("android.appwidget.provider", Resource = "@xml/overview_list_widget_info")]
public sealed class ListWidgetProvider : OverviewWidgetProviderBase
{
    protected override WidgetKind Kind => WidgetKind.List;

    protected override string DefaultTitle => "Overview List";

    protected override string DefaultSubtitle => "Tap to open your list.";

    protected override string DefaultDeepLink => AppNavigationRequest.CreateDeepLink(AppNavigationTarget.List);
}
