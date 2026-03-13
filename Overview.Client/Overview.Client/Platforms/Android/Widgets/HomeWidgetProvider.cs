using Overview.Client.Application.Navigation;
using Overview.Client.Infrastructure.Widgets;

namespace Overview.Client.Droid.Widgets;

[global::Android.Content.BroadcastReceiver(Enabled = true, Exported = false, Label = "Overview Home Widget")]
[global::Android.App.IntentFilter([global::Android.Appwidget.AppWidgetManager.ActionAppwidgetUpdate])]
[global::Android.App.MetaData("android.appwidget.provider", Resource = "@xml/overview_home_widget_info")]
public sealed class HomeWidgetProvider : OverviewWidgetProviderBase
{
    protected override WidgetKind Kind => WidgetKind.Home;

    protected override string DefaultTitle => "Overview Home";

    protected override string DefaultSubtitle => "Tap to open the timeline.";

    protected override string DefaultDeepLink => AppNavigationRequest.CreateDeepLink(AppNavigationTarget.Home);
}
