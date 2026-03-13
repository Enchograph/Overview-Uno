using Overview.Client.Application.Auth;
using Overview.Client.Application.Settings;
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

    private static SettingsPageViewModel CreateViewModel()
    {
        return new SettingsPageViewModel(
            new FakeAuthenticationService(),
            new FakeUserSettingsService());
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
        public Task<UserSettings> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new UserSettings
            {
                UserId = userId,
                ListPageDefaultTab = ListPageTab.Important,
                ListPageSortBy = ListSortBy.CreatedAt,
                ListPageTheme = "forest",
                SourceDeviceId = "test-device"
            });
        }

        public Task<UserSettings> SaveAsync(Guid userId, UserSettingsUpdateRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
