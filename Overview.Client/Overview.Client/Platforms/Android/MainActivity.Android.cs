using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace Overview.Client.Droid;

[Activity(
    MainLauncher = true,
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
}
