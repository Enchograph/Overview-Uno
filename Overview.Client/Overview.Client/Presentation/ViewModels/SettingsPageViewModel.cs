using Overview.Client.Application.Auth;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;

namespace Overview.Client.Presentation.ViewModels;

public sealed class SettingsPageViewModel
{
    public const string AiSectionKey = "ai";
    public const string ListSectionKey = "list";

    private readonly IAuthenticationService authenticationService;
    private readonly IUserSettingsService userSettingsService;
    private UserSettings? currentSettings;

    public SettingsPageViewModel(
        IAuthenticationService authenticationService,
        IUserSettingsService userSettingsService)
    {
        this.authenticationService = authenticationService;
        this.userSettingsService = userSettingsService;
        Sections = Array.Empty<SettingsSectionEntry>();
        ActiveFields = Array.Empty<SettingsSectionField>();
        PageTitle = "Settings";
        PageSubtitle = "Choose a section to manage Overview preferences and integrations.";
        RootIntro = "Sign in to configure sync, AI, and planning preferences.";
        SessionSummary = "Account status unavailable.";
        AiSettingsForm = new AiSettingsFormModel();
    }

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

    public bool CanSaveAiSettings => IsAuthenticated && !IsBusy;

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
        ActiveFields = BuildActiveFields(section.Key, currentSettings, authenticationService.CurrentSession);
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
                ActiveFields = BuildActiveFields(AiSectionKey, currentSettings, session);
                ResetSectionDraft(AiSectionKey);
                DetailFootnote = BuildDetailFootnote(AiSectionKey);
                SessionSummary =
                    $"Signed in as {session.Email}. Sync endpoint: {DisplayOrFallback(currentSettings.SyncServerBaseUrl, "not configured")}.";
                StatusMessage = "AI settings saved.";
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
            : "Select a settings section. Secondary pages already follow the final back-navigation structure.";
        SessionSummary = session is null
            ? "Not signed in. Account and sync settings will unlock after login."
            : $"Signed in as {session.Email}. Sync endpoint: {DisplayOrFallback(currentSettings?.SyncServerBaseUrl, "not configured")}.";
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
                Summary = session is null ? "Not signed in" : $"{session.Email} · Session active",
                Description = "Review the current account identity before later account-management tasks land."
            },
            new SettingsSectionEntry
            {
                Key = "sync",
                Title = "Sync",
                Subtitle = "Server endpoint, sync model, and background status entry.",
                Summary =
                    $"Server {DisplayOrFallback(settings?.SyncServerBaseUrl, "not configured")} · Conflict policy last-write-wins",
                Description = "Expose sync configuration and reserve the secondary page for status and manual controls."
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
        AuthSession? session)
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
                CreateField("Authentication", session is null ? "Not signed in" : "Signed in"),
                CreateField("Email", session?.Email ?? "Unavailable"),
                CreateField("User ID", session?.UserId.ToString() ?? "Unavailable"),
                CreateField("Token Expires At", session?.AccessTokenExpiresAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz") ?? "Unavailable")
            ],
            "sync" =>
            [
                CreateField("Sync Server", DisplayOrFallback(settings?.SyncServerBaseUrl, "not configured")),
                CreateField("Conflict Strategy", "LastModifiedAt last-write-wins"),
                CreateField("Notification Toggle", settings?.NotificationEnabled == false ? "Disabled" : "Enabled"),
                CreateField("Background Sync UI", "Reserved for later task")
            ],
            "about" =>
            [
                CreateField("Product", "Overview / 一览"),
                CreateField("Client Stack", "Uno Platform + MVVM"),
                CreateField("Server Stack", "ASP.NET Core + PostgreSQL"),
                CreateField("Current Milestone", "Milestone A - MVP")
            ],
            _ => Array.Empty<SettingsSectionField>()
        };
    }

    private static string BuildDetailFootnote(string sectionKey)
    {
        return sectionKey switch
        {
            "ai" => "AI settings now persist to synchronized user settings. Chat delivery will land in the next AI tasks.",
            "sync" => "Manual sync actions and status widgets will be added in the real-time sync presentation phase.",
            "about" => "This page is a stable shell for later version, license, and support details.",
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
}
