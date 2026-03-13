using Overview.Client.Application.Auth;
using Overview.Client.Application.Items;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Presentation.Pages;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Tests;

public sealed class AddItemPageViewModelTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task InitializeAsync_AppliesTaskTabPresetsToForm()
    {
        var viewModel = CreateViewModel();

        await viewModel.InitializeAsync(new AddItemNavigationRequest
        {
            SourceTabKey = ListPageTab.Tasks.ToString(),
            SuggestedType = ItemType.Task,
            SuggestedStartDate = new DateOnly(2026, 3, 13)
        });

        Assert.Equal(ItemType.Task, viewModel.Form.Type);
        Assert.Equal(new DateOnly(2026, 3, 13), viewModel.Form.StartDate);
        Assert.Equal(new DateOnly(2026, 3, 13), viewModel.Form.DeadlineDate);
    }

    [Fact]
    public async Task InitializeAsync_AppliesImportantAndNotePresetsToForm()
    {
        var viewModel = CreateViewModel();

        await viewModel.InitializeAsync(new AddItemNavigationRequest
        {
            SourceTabKey = ListPageTab.Important.ToString(),
            SuggestedType = ItemType.Note,
            SuggestedIsImportant = true,
            SuggestedStartDate = new DateOnly(2026, 3, 14)
        });

        Assert.Equal(ItemType.Note, viewModel.Form.Type);
        Assert.True(viewModel.Form.IsImportant);
        Assert.Equal(new DateOnly(2026, 3, 14), viewModel.Form.TargetDate);
    }

    private static AddItemPageViewModel CreateViewModel()
    {
        return new AddItemPageViewModel(
            new FakeAuthenticationService(),
            new FakeItemService(),
            new FakeUserSettingsService());
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public AuthSession? CurrentSession { get; } = new()
        {
            UserId = UserId,
            Email = "test@example.com",
            BaseUrl = "https://example.com",
            AccessToken = "token",
            RefreshToken = "refresh",
            AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
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

    private sealed class FakeItemService : IItemService
    {
        public Task<Item?> GetAsync(Guid userId, Guid itemId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Item?>(null);
        }

        public Task<IReadOnlyList<Item>> ListAsync(Guid userId, ItemQueryOptions? options = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Item>>(Array.Empty<Item>());
        }

        public Task<Item> CreateAsync(Guid userId, ItemUpsertRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Item> UpdateAsync(Guid userId, Guid itemId, ItemUpsertRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Item> SetCompletedAsync(Guid userId, Guid itemId, bool isCompleted, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Item> SetImportantAsync(Guid userId, Guid itemId, bool isImportant, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
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
                TimeZoneId = TimeZoneInfo.Utc.Id,
                DayPlanStartTime = new TimeOnly(8, 0)
            });
        }

        public Task<UserSettings> SaveAsync(Guid userId, UserSettingsUpdateRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
