namespace Overview.Client.Presentation.Layout;

internal static class AdaptiveLayout
{
    public const double TabletMinWidth = 768d;
    public const double DualPaneMinWidth = 1100d;
    public const double NavigationRailMinWidth = 900d;

    public static bool IsTablet(double width) => width >= TabletMinWidth;

    public static bool UseDualPane(double width) => width >= DualPaneMinWidth;

    public static bool UseNavigationRail(double width) => width >= NavigationRailMinWidth;
}
