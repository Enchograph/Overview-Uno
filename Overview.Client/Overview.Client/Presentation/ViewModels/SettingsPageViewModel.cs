using Overview.Client.Application.Auth;
using Overview.Client.Application.Settings;
using Overview.Client.Application.Sync;
using Overview.Client.Domain.Entities;
using Overview.Client.Infrastructure.Platform;

namespace Overview.Client.Presentation.ViewModels;

public sealed class SettingsPageViewModel
{
    public const string AiSectionKey = "ai";
    public const string ListSectionKey = "list";
    public const string SyncSectionKey = "sync";

    private readonly IAuthenticationService authenticationService;
    private readonly IUserSettingsService userSettingsService;
    private readonly ISyncOrchestrationService syncOrchestrationService;
    private readonly IPlatformCapabilities platformCapabilities;
    private UserSettings? currentSettings;
    private SyncStatusSnapshot currentSyncStatus;

    public SettingsPageViewModel(
        IAuthenticationService authenticationService,
        IUserSettingsService userSettingsService,
        ISyncOrchestrationService syncOrchestrationService,
        IPlatformCapabilities platformCapabilities)
    {
        this.authenticationService = authenticationService;
        this.userSettingsService = userSettingsService;
        this.syncOrchestrationService = syncOrchestrationService;
        this.platformCapabilities = platformCapabilities;
        Sections = Array.Empty<SettingsSectionEntry>();
        ActiveFields = Array.Empty<SettingsSectionField>();
        PageTitle = "Settings";
        PageSubtitle = "Choose a section to manage Overview preferences and integrations.";
        RootIntro = "Sign in to configure sync, AI, and planning preferences.";
        SessionSummary = "Account status unavailable.";
        AiSettingsForm = new AiSettingsFormModel();
        currentSyncStatus = syncOrchestrationService.CurrentStatus;
        syncOrchestrationService.StatusChanged += OnSyncStatusChanged;
    }

    public event EventHandler? ViewStateChanged;

    public IReadOnlyList<SettingsSectionEntry> Sections { get; private set; }

    public IReadOnlyList<SettingsSectionField> ActiveFields { get; private set; }

    public SettingsSectionEntry? ActiveSection { get; private set; }

    public bool IsBusy { get; private set; }

    public string StatusMessage { get; private set; } = string.Empty;

    public string PageTitle { get; private set; }

    public string PageSubtitle { get; private set; }

    public string RootIntro { get; private set; }

    public string SessionSummary { get; private set; }

    public string DetailLead { get; private set; } = string.Empty;

    public string DetailFootnote { get; private set; } =
        "This section currently provides the navigation shell and summary. Editable controls will be added in later tasks.";

    public AiSettingsFormModel AiSettingsForm { get; }

    public bool IsAuthenticated => authenticationService.CurrentSession is not null;

    public bool IsRootView => ActiveSection is null;

    public bool IsAiEditorVisible => string.Equals(ActiveSection?.Key, AiSectionKey, StringComparison.Ordinal);

    public bool IsSyncSectionVisible => string.Equals(ActiveSection?.Key, SyncSectionKey, StringComparison.Ordinal);

    public bool CanSaveAiSettings => IsAuthenticated && !IsBusy;

    public bool CanRunManualSync => IsAuthenticated && authenticationService.CurrentSession?.SupportsRemoteSync == true && !IsBusy;

    public async Task InitializeAsync(
        string? initialSectionKey = null,
        CancellationToken cancellationToken = default)
    {
        await ExecuteBusyActionAsync(
            async () =>
            {
                await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
                ApplySectionState(initialSectionKey);
                return true;
            }).ConfigureAwait(false);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteBusyActionAsync(
            async () =>
            {
                await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
                ApplySectionState(ActiveSection?.Key);
                return true;
            }).ConfigureAwait(false);
    }

    public void OpenSection(string sectionKey)
    {
        var section = Sections.FirstOrDefault(candidate =>
            string.Equals(candidate.Key, sectionKey, StringComparison.Ordinal));
        if (section is null)
        {
            return;
        }

        ActiveSection = section;
        PageTitle = section.Title;
        PageSubtitle = section.Description;
        DetailLead = section.Subtitle;
        ActiveFields = BuildActiveFields(
            section.Key,
            currentSettings,
            authenticationService.CurrentSession,
            currentSyncStatus,
            platformCapabilities);
        DetailFootnote = BuildDetailFootnote(section.Key);
        ResetSectionDraft(section.Key);
        StatusMessage = $"{section.Title} section ready.";
    }

    public void NavigateBack()
    {
        ActiveSection = null;
        ActiveFields = Array.Empty<SettingsSectionField>();
        PageTitle = "Settings";
        PageSubtitle = "Choose a section to manage Overview preferences and integrations.";
        DetailLead = string.Empty;
        DetailFootnote =
            "This section currently provides the navigation shell and summary. Editable controls will be added in later tasks.";
    }

    public void UpdateAiDraft(string? baseUrl, string? apiKey, string? model)
    {
        AiSettingsForm.BaseUrl = baseUrl ?? string.Empty;
        AiSettingsForm.ApiKey = apiKey ?? string.Empty;
        AiSettingsForm.Model = model ?? string.Empty;
    }

    public async Task SaveAiSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (authenticationService.CurrentSession is not { } session)
        {
            StatusMessage = "Sign in before saving AI settings.";
            return;
        }

        await ExecuteBusyActionAsync(
            async () =>
            {
                var baseline = currentSettings
                    ?? await userSettingsService.GetAsync(session.UserId, cancellationToken).ConfigureAwait(false);
                currentSettings = await userSettingsService.SaveAsync(
                    session.UserId,
                    ToUpdateRequest(
                        baseline,
                        AiSettingsForm.BaseUrl,
                        AiSettingsForm.ApiKey,
                        AiSettingsForm.Model),
                    cancellationToken).ConfigureAwait(false);

                Sections = BuildSections(currentSettings, session);
                ActiveFields = BuildActiveFields(
                    AiSectionKey,
                    currentSettings,
                    session,
                    currentSyncStatus,
                    platformCapabilities);
                ResetSectionDraft(AiSectionKey);
                DetailFootnote = BuildDetailFootnote(AiSectionKey);
                SessionSummary = BuildSessionSummary(session, currentSettings);
                StatusMessage = "AI settings saved.";
                return true;
            }).ConfigureAwait(false);
    }

    public async Task RunManualSyncAsync(CancellationToken cancellationToken = default)
    {
        if (authenticationService.CurrentSession is null)
        {
            StatusMessage = "Sign in before running sync.";
            NotifyStateChanged();
            return;
        }

        await ExecuteBusyActionAsync(
            async () =>
            {
                var snapshot = await syncOrchestrationService.SynchronizeNowAsync(cancellationToken).ConfigureAwait(false);
                ApplySyncStatus(snapshot);
                StatusMessage = snapshot.State switch
                {
                    SyncLifecycleState.Succeeded => "Manual sync completed.",
                    SyncLifecycleState.Failed => $"Manual sync failed: {snapshot.LastError ?? "Unknown error."}",
                    SyncLifecycleState.Offline => "Offline mode is active. Sync is unavailable.",
                    SyncLifecycleState.RequiresAuthentication => "Sign in before running sync.",
                    _ => "Manual sync requested."
                };
                return true;
            }).ConfigureAwait(false);
    }

    private void ApplySectionState(string? sectionKey)
    {
        var normalizedSectionKey = NormalizeSectionKey(sectionKey);
        if (normalizedSectionKey is not null)
        {
            OpenSection(normalizedSectionKey);
            return;
        }

        NavigateBack();
        StatusMessage = IsAuthenticated
            ? "Settings summary loaded."
            : "Sign in to load synchronized settings.";
    }

    private async Task LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        var session = authenticationService.CurrentSession;
        currentSettings = session is null
            ? null
            : await userSettingsService.GetAsync(session.UserId, cancellationToken).ConfigureAwait(false);

        Sections = BuildSections(currentSettings, session);
        RootIntro = session is null
            ? "Sign in to configure sync, AI, and planning preferences."
            : session.SupportsRemoteSync
                ? "Select a settings section. Secondary pages already follow the final back-navigation structure."
                : "Offline mode is active. Your data, reminders, and settings stay on this device.";
        SessionSummary = BuildSessionSummary(session, currentSettings);
    }

    private static IReadOnlyList<SettingsSectionEntry> BuildSections(
        UserSettings? settings,
        AuthSession? session)
    {
        return
        [
            new SettingsSectionEntry
            {
                Key = "general",
                Title = "General",
                Subtitle = "Language, theme, timezone, and calendar defaults.",
                Summary =
                    $"Language {DisplayOrFallback(settings?.Language, "zh-CN")} · Theme {settings?.ThemeMode.ToString() ?? "System"} · Week starts {DisplayOrFallback(settings?.WeekStartDay.ToString(), "Monday")}",
                Description = "Manage shared app preferences used across the rest of the client."
            },
            new SettingsSectionEntry
            {
                Key = "home",
                Title = "Home",
                Subtitle = "Homepage view mode and time-block planning defaults.",
                Summary =
                    $"{settings?.HomeViewMode.ToString() ?? "Week"} view · Starts {settings?.DayPlanStartTime.ToString("HH:mm") ?? "08:00"} · {settings?.TimeBlockCount.ToString() ?? "12"} blocks",
                Description = "Prepare the homepage week or month layout and planning block defaults."
            },
            new SettingsSectionEntry
            {
                Key = "list",
                Title = "List",
                Subtitle = "Default tab, sort order, manual ordering, and theme shell.",
                Summary =
                    $"Default {settings?.ListPageDefaultTab.ToString() ?? "MyDay"} · Sort {settings?.ListPageSortBy.ToString() ?? "Importance"} · Theme {DisplayOrFallback(settings?.ListPageTheme, "default")}",
                Description = "Control the list page entry point and later list-specific appearance options."
            },
            new SettingsSectionEntry
            {
                Key = "ai",
                Title = "AI",
                Subtitle = "OpenAI-compatible endpoint, model, and API credential placeholders.",
                Summary =
                    $"Base URL {DisplayOrFallback(settings?.AiBaseUrl, "not configured")} · Model {DisplayOrFallback(settings?.AiModel, "not configured")}",
                Description = "Configure the direct client-to-AI connection required by the product scope."
            },
            new SettingsSectionEntry
            {
                Key = "account",
                Title = "Account",
                Subtitle = "Current sign-in identity and future account actions.",
                Summary = session is null ? "Not signed in" : session.SupportsRemoteSync ? $"{session.Email} · Cloud sync active" : $"{session.Email} · Offline local mode",
                Description = "Review the current account identity before later account-management tasks land."
            },
            new SettingsSectionEntry
            {
                Key = "sync",
                Title = "Sync",
                Subtitle = "Server endpoint, sync model, and background status entry.",
                Summary = session?.SupportsRemoteSync == true
                    ? $"Server {DisplayOrFallback(settings?.SyncServerBaseUrl, session.BaseUrl)} · Manual sync and live status"
                    : "Offline mode · Local-only data and sync controls disabled",
                Description = session?.SupportsRemoteSync == true
                    ? "Expose sync configuration and reserve the secondary page for status and manual controls."
                    : "Show that this session stores data locally and does not contact the sync server."
            },
            new SettingsSectionEntry
            {
                Key = "about",
                Title = "About",
                Subtitle = "Project scope, platform goals, and delivery status summary.",
                Summary = "Overview MVP shell on Uno Platform with ASP.NET Core sync backend",
                Description = "Surface product identity and keep a stable target for future about content."
            }
        ];
    }

    private static IReadOnlyList<SettingsSectionField> BuildActiveFields(
        string sectionKey,
        UserSettings? settings,
        AuthSession? session,
        SyncStatusSnapshot? syncStatus,
        IPlatformCapabilities platformCapabilities)
    {
        return sectionKey switch
        {
            "general" =>
            [
                CreateField("Language", DisplayOrFallback(settings?.Language, "zh-CN")),
                CreateField("Theme Mode", settings?.ThemeMode.ToString() ?? "System"),
                CreateField("Week Starts On", DisplayOrFallback(settings?.WeekStartDay.ToString(), "Monday")),
                CreateField("Timezone", DisplayOrFallback(settings?.TimeZoneId, TimeZoneInfo.Local.Id))
            ],
            "home" =>
            [
                CreateField("View Mode", settings?.HomeViewMode.ToString() ?? "Week"),
                CreateField("Day Starts At", settings?.DayPlanStartTime.ToString("HH:mm") ?? "08:00"),
                CreateField("Block Duration", $"{settings?.TimeBlockDurationMinutes ?? 60} min"),
                CreateField("Block Gap", $"{settings?.TimeBlockGapMinutes ?? 0} min"),
                CreateField("Block Count", (settings?.TimeBlockCount ?? 12).ToString())
            ],
            "list" =>
            [
                CreateField("Default Tab", settings?.ListPageDefaultTab.ToString() ?? "MyDay"),
                CreateField("Sort By", settings?.ListPageSortBy.ToString() ?? "Importance"),
                CreateField("Theme Preset", DisplayOrFallback(settings?.ListPageTheme, "default")),
                CreateField(
                    "Manual Order Slots",
                    (
                        (settings?.ListManualOrder.MyDay.Count ?? 0) +
                        (settings?.ListManualOrder.AllItems.Count ?? 0) +
                        (settings?.ListManualOrder.Tasks.Count ?? 0) +
                        (settings?.ListManualOrder.Schedules.Count ?? 0) +
                        (settings?.ListManualOrder.Notes.Count ?? 0) +
                        (settings?.ListManualOrder.Important.Count ?? 0)
                    ).ToString())
            ],
            "ai" =>
            [
                CreateField("Base URL", DisplayOrFallback(settings?.AiBaseUrl, "not configured")),
                CreateField("Model", DisplayOrFallback(settings?.AiModel, "not configured")),
                CreateField("API Key", string.IsNullOrWhiteSpace(settings?.AiApiKey) ? "not configured" : "configured"),
                CreateField("Conversation Scope", "Current user message only")
            ],
            "account" =>
            [
                CreateField("Authentication", session is null ? "Not signed in" : session.SupportsRemoteSync ? "Signed in to sync server" : "Offline local session"),
                CreateField("Email", session?.Email ?? "Unavailable"),
                CreateField("User ID", session?.UserId.ToString() ?? "Unavailable"),
                CreateField("Token Expires At", session?.SupportsRemoteSync == true ? session.AccessTokenExpiresAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz") : "Unavailable in offline mode")
            ],
            "sync" =>
            [
                CreateField("Sync Server", session?.SupportsRemoteSync == true ? DisplayOrFallback(settings?.SyncServerBaseUrl, session.BaseUrl) : "Offline mode"),
                CreateField("Conflict Strategy", session?.SupportsRemoteSync == true ? "LastModifiedAt last-write-wins" : "Not applicable while offline"),
                CreateField("Auto Sync", session?.SupportsRemoteSync == true ? (syncStatus?.IsAutoSyncEnabled == true ? "Running" : "Stopped") : "Disabled in offline mode"),
                CreateField("Current State", FormatSyncState(syncStatus)),
                CreateField("Last Trigger", FormatTrigger(syncStatus?.LastTrigger)),
                CreateField("Pending Local Changes", (syncStatus?.PendingChangeCount ?? 0).ToString()),
                CreateField("Applied Changes", (syncStatus?.AppliedChangeCount ?? 0).ToString()),
                CreateField("Pulled Items", (syncStatus?.PulledItemCount ?? 0).ToString()),
                CreateField("Settings Applied", syncStatus?.SettingsApplied == true ? "Yes" : "No"),
                CreateField("Conflicts", (syncStatus?.ConflictCount ?? 0).ToString()),
                CreateField("Last Attempt", FormatTimestamp(syncStatus?.LastAttemptedAt)),
                CreateField("Last Success", FormatTimestamp(syncStatus?.LastSuccessfulAt)),
                CreateField("Last Error", DisplayOrFallback(syncStatus?.LastError, "None"))
            ],
            "about" =>
            [
                CreateField("Product", "Overview / 一览"),
                CreateField("Client Stack", "Uno Platform + MVVM"),
                CreateField("Server Stack", "ASP.NET Core + PostgreSQL"),
                CreateField("Current Milestone", "Milestone C - Platform Closure"),
                CreateField("Current Platform", platformCapabilities.PlatformName),
                CreateField("Platform Family", platformCapabilities.PlatformFamily),
                CreateField("Main Flow Status", platformCapabilities.MainFlowStatus),
                CreateField("Local Data", platformCapabilities.SupportsPersistentLocalStorage ? "Persistent local storage" : "Session-only local state"),
                CreateField("Notifications", platformCapabilities.SupportsLocalNotifications ? "Supported" : "Downgraded / unavailable"),
                CreateField("Widgets", platformCapabilities.SupportsHomeWidgets ? "Supported" : "Downgraded / unavailable"),
                CreateField("Capability Summary", platformCapabilities.CapabilitySummary),
                CreateField("Degradation Policy", platformCapabilities.DegradationSummary)
            ],
            _ => Array.Empty<SettingsSectionField>()
        };
    }

    private static string BuildDetailFootnote(string sectionKey)
    {
        return sectionKey switch
        {
            "ai" => "AI settings now persist to synchronized user settings. Chat delivery will land in the next AI tasks.",
            "sync" => "Manual sync is available only for remote sessions. Offline mode keeps all data on this device.",
            "about" => "This page now records the active platform profile, supported capabilities, and explicit downgrade policy.",
            _ => "This secondary page skeleton is complete. Editable controls will be layered in later focused tasks."
        };
    }

    private async Task<bool> ExecuteBusyActionAsync(Func<Task<bool>> action)
    {
        if (IsBusy)
        {
            return false;
        }

        try
        {
            IsBusy = true;
            return await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static SettingsSectionField CreateField(string label, string value)
    {
        return new SettingsSectionField
        {
            Label = label,
            Value = value
        };
    }

    private static string DisplayOrFallback(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private void ResetSectionDraft(string sectionKey)
    {
        if (!string.Equals(sectionKey, AiSectionKey, StringComparison.Ordinal))
        {
            return;
        }

        UpdateAiDraft(
            currentSettings?.AiBaseUrl,
            currentSettings?.AiApiKey,
            currentSettings?.AiModel);
    }

    private static UserSettingsUpdateRequest ToUpdateRequest(
        UserSettings settings,
        string? aiBaseUrl,
        string? aiApiKey,
        string? aiModel)
    {
        return new UserSettingsUpdateRequest
        {
            Language = settings.Language,
            ThemeMode = settings.ThemeMode,
            ThemePreset = settings.ThemePreset,
            WeekStartDay = settings.WeekStartDay,
            HomeViewMode = settings.HomeViewMode,
            DayPlanStartTime = settings.DayPlanStartTime,
            TimeBlockDurationMinutes = settings.TimeBlockDurationMinutes,
            TimeBlockGapMinutes = settings.TimeBlockGapMinutes,
            TimeBlockCount = settings.TimeBlockCount,
            ListPageDefaultTab = settings.ListPageDefaultTab,
            ListPageSortBy = settings.ListPageSortBy,
            ListPageTheme = settings.ListPageTheme,
            ListManualOrder = settings.ListManualOrder,
            AiBaseUrl = aiBaseUrl ?? string.Empty,
            AiApiKey = aiApiKey ?? string.Empty,
            AiModel = aiModel ?? string.Empty,
            SyncServerBaseUrl = settings.SyncServerBaseUrl,
            NotificationEnabled = settings.NotificationEnabled,
            WidgetPreferences = settings.WidgetPreferences,
            TimeZoneId = settings.TimeZoneId
        };
    }

    private static string? NormalizeSectionKey(string? sectionKey)
    {
        return string.IsNullOrWhiteSpace(sectionKey)
            ? null
            : sectionKey.Trim().ToLowerInvariant();
    }

    private void OnSyncStatusChanged(object? sender, SyncStatusSnapshot snapshot)
    {
        ApplySyncStatus(snapshot);
        NotifyStateChanged();
    }

    private void ApplySyncStatus(SyncStatusSnapshot snapshot)
    {
        currentSyncStatus = snapshot;
        if (ActiveSection is not null)
        {
            ActiveFields = BuildActiveFields(
                ActiveSection.Key,
                currentSettings,
                authenticationService.CurrentSession,
                currentSyncStatus,
                platformCapabilities);
        }
    }

    private void NotifyStateChanged()
    {
        ViewStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private static string FormatSyncState(SyncStatusSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return "Unknown";
        }

        return snapshot.State switch
        {
            SyncLifecycleState.Idle => "Idle",
            SyncLifecycleState.Running => "Running",
            SyncLifecycleState.Succeeded => "Succeeded",
            SyncLifecycleState.Failed => $"Failed ({snapshot.ConsecutiveFailureCount} consecutive)",
            SyncLifecycleState.RequiresAuthentication => "Sign-in required",
            SyncLifecycleState.Offline => "Offline mode",
            _ => snapshot.State.ToString()
        };
    }

    private static string BuildSessionSummary(AuthSession? session, UserSettings? settings)
    {
        if (session is null)
        {
            return "Not signed in. Account and sync settings will unlock after login.";
        }

        return session.SupportsRemoteSync
            ? $"Signed in as {session.Email}. Sync endpoint: {DisplayOrFallback(settings?.SyncServerBaseUrl, session.BaseUrl)}."
            : $"Offline mode active as {session.Email}. Data is stored on this device only.";
    }

    private static string FormatTrigger(SyncExecutionTrigger? trigger)
    {
        return trigger switch
        {
            SyncExecutionTrigger.Automatic => "Automatic",
            SyncExecutionTrigger.Manual => "Manual",
            _ => "Not yet triggered"
        };
    }

    private static string FormatTimestamp(DateTimeOffset? timestamp)
    {
        return timestamp?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz") ?? "Never";
    }
}
