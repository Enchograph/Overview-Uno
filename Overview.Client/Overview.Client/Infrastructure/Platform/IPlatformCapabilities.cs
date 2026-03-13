namespace Overview.Client.Infrastructure.Platform;

public interface IPlatformCapabilities
{
    string PlatformName { get; }

    string PlatformFamily { get; }

    bool SupportsPersistentLocalStorage { get; }

    bool SupportsLocalNotifications { get; }

    bool SupportsHomeWidgets { get; }

    string MainFlowStatus { get; }

    string CapabilitySummary { get; }

    string DegradationSummary { get; }
}

public sealed record PlatformCapabilities(
    string PlatformName,
    string PlatformFamily,
    bool SupportsPersistentLocalStorage,
    bool SupportsLocalNotifications,
    bool SupportsHomeWidgets,
    string MainFlowStatus,
    string CapabilitySummary,
    string DegradationSummary) : IPlatformCapabilities
{
    public static IPlatformCapabilities Current { get; } = CreateCurrent();

    private static IPlatformCapabilities CreateCurrent()
    {
#if __ANDROID__
        return new PlatformCapabilities(
            PlatformName: "Android",
            PlatformFamily: "Mobile",
            SupportsPersistentLocalStorage: true,
            SupportsLocalNotifications: true,
            SupportsHomeWidgets: true,
            MainFlowStatus: "Primary mobile target with full local notification and widget integration.",
            CapabilitySummary: "Notifications and all four widgets are active on Android.",
            DegradationSummary: "No platform degradation is active on this target.");
#elif __WASM__
        return new PlatformCapabilities(
            PlatformName: "Web",
            PlatformFamily: "Browser",
            SupportsPersistentLocalStorage: false,
            SupportsLocalNotifications: false,
            SupportsHomeWidgets: false,
            MainFlowStatus: "Main flows run in the browser with session-scoped local state and explicit platform downgrades.",
            CapabilitySummary: "Home, List, AI, Add, Settings, login, and sync flows stay available in the browser.",
            DegradationSummary: "Web uses in-memory local state for this release; data durability relies on sync after sign-in. Notifications and widgets are unavailable and intentionally downgraded.");
#else
        return new PlatformCapabilities(
            PlatformName: "Windows/Desktop",
            PlatformFamily: "Desktop",
            SupportsPersistentLocalStorage: true,
            SupportsLocalNotifications: false,
            SupportsHomeWidgets: false,
            MainFlowStatus: "Desktop main flows run with persistent local storage and explicit notification/widget downgrade.",
            CapabilitySummary: "Home, List, AI, Add, Settings, login, and sync flows are supported on desktop windows.",
            DegradationSummary: "Desktop keeps notifications and widgets disabled in this release; all related refresh paths degrade to no-op services.");
#endif
    }
}
