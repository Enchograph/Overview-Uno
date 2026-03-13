using Overview.Client.Application.Navigation;
using Overview.Client.Infrastructure.Widgets;

namespace Overview.Client.Droid.Widgets;

[global::Android.Content.BroadcastReceiver(Enabled = true, Exported = false, Label = "Overview AI Widget")]
[global::Android.App.IntentFilter([global::Android.Appwidget.AppWidgetManager.ActionAppwidgetUpdate])]
[global::Android.App.MetaData("android.appwidget.provider", Resource = "@xml/overview_ai_widget_info")]
public sealed class AiShortcutWidgetProvider : OverviewWidgetProviderBase
{
    protected override WidgetKind Kind => WidgetKind.AiShortcut;

    protected override string DefaultTitle => "Overview AI";

    protected override string DefaultSubtitle => "Tap to open AI chat.";

    protected override string DefaultDeepLink => AppNavigationRequest.CreateDeepLink(AppNavigationTarget.Ai);
}
