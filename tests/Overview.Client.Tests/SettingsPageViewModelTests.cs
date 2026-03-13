using Overview.Client.Application.Auth;
using Overview.Client.Application.Settings;
using Overview.Client.Application.Sync;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Tests;

public sealed class SettingsPageViewModelTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task InitializeAsync_WithListSectionKey_OpensListSectionImmediately()
    {
        var viewModel = CreateViewModel();

        await viewModel.InitializeAsync(SettingsPageViewModel.ListSectionKey);

        Assert.False(viewModel.IsRootView);
        Assert.Equal(SettingsPageViewModel.ListSectionKey, viewModel.ActiveSection?.Key);
        Assert.Equal("List", viewModel.PageTitle);
        Assert.Contains(viewModel.ActiveFields, field => field.Label == "Default Tab" && field.Value == "Important");
        Assert.Contains(viewModel.ActiveFields, field => field.Label == "Theme Preset" && field.Value == "forest");
    }

    [Fact]
    public async Task RefreshAsync_PreservesOpenedListSection()
    {
        var viewModel = CreateViewModel();

        await viewModel.InitializeAsync(SettingsPageViewModel.ListSectionKey);
        await viewModel.RefreshAsync();

        Assert.False(viewModel.IsRootView);
        Assert.Equal(SettingsPageViewModel.ListSectionKey, viewModel.ActiveSection?.Key);
        Assert.Equal("List section ready.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task OpenSection_WithAiSection_SeedsEditableDraftFromStoredSettings()
    {
        var viewModel = CreateViewModel();

        await viewModel.InitializeAsync(SettingsPageViewModel.AiSectionKey);

        Assert.False(viewModel.IsRootView);
        Assert.True(viewModel.IsAiEditorVisible);
        Assert.Equal("https://ai.example.com/v1", viewModel.AiSettingsForm.BaseUrl);
        Assert.Equal("secret-key", viewModel.AiSettingsForm.ApiKey);
        Assert.Equal("gpt-4.1-mini", viewModel.AiSettingsForm.Model);
    }

    [Fact]
    public async Task SaveAiSettingsAsync_PersistsSettingsAndRefreshesSummary()
    {
        var settingsService = new FakeUserSettingsService();
        var viewModel = new SettingsPageViewModel(
            new FakeAuthenticationService(),
            settingsService,
            new FakeSyncOrchestrationService());

        await viewModel.InitializeAsync(SettingsPageViewModel.AiSectionKey);

        viewModel.UpdateAiDraft(" https://proxy.example.com/v1 ", " next-secret ", " gpt-4.1 ");
        await viewModel.SaveAiSettingsAsync();

        Assert.NotNull(settingsService.LastSavedRequest);
        Assert.Equal("https://proxy.example.com/v1", settingsService.LastSavedRequest!.AiBaseUrl);
        Assert.Equal("next-secret", settingsService.LastSavedRequest.AiApiKey);
        Assert.Equal("gpt-4.1", settingsService.LastSavedRequest.AiModel);
        Assert.Equal("AI settings saved.", viewModel.StatusMessage);
        Assert.Contains(viewModel.ActiveFields, field => field.Label == "Base URL" && field.Value == "https://proxy.example.com/v1");
        Assert.Contains(viewModel.Sections, section => section.Key == SettingsPageViewModel.AiSectionKey && section.Summary.Contains("https://proxy.example.com/v1", StringComparison.Ordinal));
    }

    [Fact]
    public async Task InitializeAsync_WithSyncSectionKey_ShowsManualSyncStatusFields()
    {
        var syncService = new FakeSyncOrchestrationService
        {
            CurrentStatusSnapshot = new SyncStatusSnapshot
            {
                State = SyncLifecycleState.Succeeded,
                LastTrigger = SyncExecutionTrigger.Automatic,
                PendingChangeCount = 2,
                AppliedChangeCount = 3,
                PulledItemCount = 4,
                SettingsApplied = true,
                ConflictCount = 1,
                IsAutoSyncEnabled = true,
                LastSuccessfulAt = new DateTimeOffset(2026, 3, 13, 8, 30, 0, TimeSpan.Zero)
            }
        };
        var viewModel = new SettingsPageViewModel(
            new FakeAuthenticationService(),
            new FakeUserSettingsService(),
            syncService);

        await viewModel.InitializeAsync(SettingsPageViewModel.SyncSectionKey);

        Assert.True(viewModel.IsSyncSectionVisible);
        Assert.True(viewModel.CanRunManualSync);
        Assert.Contains(viewModel.ActiveFields, field => field.Label == "Auto Sync" && field.Value == "Running");
        Assert.Contains(viewModel.ActiveFields, field => field.Label == "Current State" && field.Value == "Succeeded");
        Assert.Contains(viewModel.ActiveFields, field => field.Label == "Pending Local Changes" && field.Value == "2");
    }

    [Fact]
    public async Task RunManualSyncAsync_UsesSyncServiceAndRefreshesFields()
    {
        var syncService = new FakeSyncOrchestrationService
        {
            SynchronizeNowResult = new SyncStatusSnapshot
            {
                State = SyncLifecycleState.Succeeded,
                LastTrigger = SyncExecutionTrigger.Manual,
                PendingChangeCount = 0,
                AppliedChangeCount = 5,
                PulledItemCount = 2,
                SettingsApplied = true,
                IsAutoSyncEnabled = true
            }
        };
        var viewModel = new SettingsPageViewModel(
            new FakeAuthenticationService(),
            new FakeUserSettingsService(),
            syncService);

        await viewModel.InitializeAsync(SettingsPageViewModel.SyncSectionKey);
        await viewModel.RunManualSyncAsync();

        Assert.Equal(1, syncService.SynchronizeNowCalls);
        Assert.Equal("Manual sync completed.", viewModel.StatusMessage);
        Assert.Contains(viewModel.ActiveFields, field => field.Label == "Last Trigger" && field.Value == "Manual");
        Assert.Contains(viewModel.ActiveFields, field => field.Label == "Applied Changes" && field.Value == "5");
    }

    private static SettingsPageViewModel CreateViewModel()
    {
        return new SettingsPageViewModel(
            new FakeAuthenticationService(),
            new FakeUserSettingsService(),
            new FakeSyncOrchestrationService());
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public AuthSession? CurrentSession { get; } = new()
        {
            UserId = UserId,
            Email = "list@example.com",
            BaseUrl = "https://sync.example.com",
            AccessToken = "token",
            RefreshToken = "refresh",
            AccessTokenExpiresAt = new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero)
        };

        public bool IsAuthenticated => true;

        public Task<VerificationCodeDispatchResult> SendVerificationCodeAsync(string baseUrl, string email, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthSession> RegisterAsync(string baseUrl, string email, string password, string verificationCode, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthSession> LoginAsync(string baseUrl, string email, string password, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthSession?> RestoreSessionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AuthSession> RefreshSessionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeUserSettingsService : IUserSettingsService
    {
        private UserSettings settings = new()
        {
            UserId = UserId,
            ListPageDefaultTab = ListPageTab.Important,
            ListPageSortBy = ListSortBy.CreatedAt,
            ListPageTheme = "forest",
            AiBaseUrl = "https://ai.example.com/v1",
            AiApiKey = "secret-key",
            AiModel = "gpt-4.1-mini",
            SourceDeviceId = "test-device"
        };

        public UserSettingsUpdateRequest? LastSavedRequest { get; private set; }

        public Task<UserSettings> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(settings);
        }

        public Task<UserSettings> SaveAsync(Guid userId, UserSettingsUpdateRequest request, CancellationToken cancellationToken = default)
        {
            LastSavedRequest = request with
            {
                AiBaseUrl = request.AiBaseUrl.Trim(),
                AiApiKey = request.AiApiKey.Trim(),
                AiModel = request.AiModel.Trim()
            };
            settings = new UserSettings
            {
                Id = settings.Id,
                UserId = userId,
                Language = LastSavedRequest.Language,
                ThemeMode = LastSavedRequest.ThemeMode,
                ThemePreset = LastSavedRequest.ThemePreset,
                WeekStartDay = LastSavedRequest.WeekStartDay,
                HomeViewMode = LastSavedRequest.HomeViewMode,
                DayPlanStartTime = LastSavedRequest.DayPlanStartTime,
                TimeBlockDurationMinutes = LastSavedRequest.TimeBlockDurationMinutes,
                TimeBlockGapMinutes = LastSavedRequest.TimeBlockGapMinutes,
                TimeBlockCount = LastSavedRequest.TimeBlockCount,
                ListPageDefaultTab = LastSavedRequest.ListPageDefaultTab,
                ListPageSortBy = LastSavedRequest.ListPageSortBy,
                ListPageTheme = LastSavedRequest.ListPageTheme,
                ListManualOrder = LastSavedRequest.ListManualOrder,
                AiBaseUrl = LastSavedRequest.AiBaseUrl,
                AiApiKey = LastSavedRequest.AiApiKey,
                AiModel = LastSavedRequest.AiModel,
                SyncServerBaseUrl = LastSavedRequest.SyncServerBaseUrl,
                NotificationEnabled = LastSavedRequest.NotificationEnabled,
                WidgetPreferences = LastSavedRequest.WidgetPreferences,
                TimeZoneId = LastSavedRequest.TimeZoneId ?? "UTC",
                SourceDeviceId = settings.SourceDeviceId
            };

            return Task.FromResult(settings);
        }
    }

    private sealed class FakeSyncOrchestrationService : ISyncOrchestrationService
    {
        public SyncStatusSnapshot CurrentStatus => CurrentStatusSnapshot;

        public event EventHandler<SyncStatusSnapshot>? StatusChanged;

        public SyncStatusSnapshot CurrentStatusSnapshot { get; set; } = new();

        public SyncStatusSnapshot SynchronizeNowResult { get; set; } = new()
        {
            State = SyncLifecycleState.Succeeded,
            LastTrigger = SyncExecutionTrigger.Manual,
            IsAutoSyncEnabled = true
        };

        public int SynchronizeNowCalls { get; private set; }

        public Task<SyncStatusSnapshot> InitializeAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CurrentStatusSnapshot);
        }

        public Task<SyncStatusSnapshot> SynchronizeNowAsync(CancellationToken cancellationToken = default)
        {
            SynchronizeNowCalls++;
            CurrentStatusSnapshot = SynchronizeNowResult;
            StatusChanged?.Invoke(this, CurrentStatusSnapshot);
            return Task.FromResult(CurrentStatusSnapshot);
        }

        public Task<SyncStatusSnapshot> StartAutoSyncAsync(TimeSpan? interval = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task StopAutoSyncAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
