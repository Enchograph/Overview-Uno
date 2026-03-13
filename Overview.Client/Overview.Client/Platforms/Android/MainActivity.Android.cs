using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Overview.Client.Droid.Widgets;

namespace Overview.Client.Droid;

[Activity(
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden
)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        global::AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);

        base.OnCreate(savedInstanceState);
        EnsureNotificationPermission();
        HandleExternalIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleExternalIntent(intent);
    }

    private void EnsureNotificationPermission()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            return;
        }

        if (ContextCompat.CheckSelfPermission(this, global::Android.Manifest.Permission.PostNotifications) == Permission.Granted)
        {
            return;
        }

        ActivityCompat.RequestPermissions(
            this,
            [global::Android.Manifest.Permission.PostNotifications],
            1001);
    }

    private static void HandleExternalIntent(Intent? intent)
    {
        var deepLink = intent?.GetStringExtra(AndroidWidgetConstants.DeepLinkExtraKey);
        if (string.IsNullOrWhiteSpace(deepLink))
        {
            return;
        }

        App.HandleExternalNavigation(deepLink);
    }
}
