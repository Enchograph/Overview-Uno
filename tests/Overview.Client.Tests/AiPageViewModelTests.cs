using Overview.Client.Application.Ai;
using Overview.Client.Application.Auth;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Tests;

public sealed class AiPageViewModelTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task InitializeAsync_LoadsTodayMessagesForAuthenticatedUser()
    {
        var chatService = new FakeAiChatService();
        var viewModel = new AiPageViewModel(new FakeAuthenticationService(), chatService);

        await viewModel.InitializeAsync();

        Assert.Equal("2026-03-13", viewModel.CurrentDayTitle);
        Assert.Single(viewModel.Messages);
        Assert.Equal("AI", viewModel.Messages[0].SpeakerLabel);
        Assert.Contains("Loaded 1 AI messages", viewModel.StatusMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendAsync_ClearsDraftAndAppendsReturnedMessages()
    {
        var chatService = new FakeAiChatService();
        var viewModel = new AiPageViewModel(new FakeAuthenticationService(), chatService);

        await viewModel.InitializeAsync();
        viewModel.UpdateDraft("Add a task for tonight");

        await viewModel.SendAsync();

        Assert.Equal(string.Empty, viewModel.DraftMessage);
        Assert.Equal(2, viewModel.Messages.Count);
        Assert.Equal("Add a task for tonight", chatService.LastSentMessage);
        Assert.Equal("AI response stored for today.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task SendAsync_WithoutAuthentication_ShowsSignInStatus()
    {
        var viewModel = new AiPageViewModel(new AnonymousAuthenticationService(), new FakeAiChatService());

        await viewModel.SendAsync();

        Assert.Equal("AI chat requires an authenticated account.", viewModel.StatusMessage);
    }

    private sealed class FakeAiChatService : IAiChatService
    {
        public string LastSentMessage { get; private set; } = string.Empty;

        public Task<AiChatDaySnapshot> GetTodaySnapshotAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AiChatDaySnapshot
            {
                OccurredOn = new DateOnly(2026, 3, 13),
                Messages =
                [
                    new AiChatMessage
                    {
                        UserId = userId,
                        OccurredOn = new DateOnly(2026, 3, 13),
                        Role = AiChatRole.Assistant,
                        Message = "You have one meeting today.",
                        CreatedAt = new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero)
                    }
                ]
            });
        }

        public Task<AiChatDaySnapshot> SendMessageAsync(Guid userId, string userMessage, CancellationToken cancellationToken = default)
        {
            LastSentMessage = userMessage;
            return Task.FromResult(new AiChatDaySnapshot
            {
                OccurredOn = new DateOnly(2026, 3, 13),
                Messages =
                [
                    new AiChatMessage
                    {
                        UserId = userId,
                        OccurredOn = new DateOnly(2026, 3, 13),
                        Role = AiChatRole.User,
                        Message = userMessage,
                        CreatedAt = new DateTimeOffset(2026, 3, 13, 8, 1, 0, TimeSpan.Zero)
                    },
                    new AiChatMessage
                    {
                        UserId = userId,
                        OccurredOn = new DateOnly(2026, 3, 13),
                        Role = AiChatRole.Assistant,
                        Message = "Noted.",
                        CreatedAt = new DateTimeOffset(2026, 3, 13, 8, 1, 5, TimeSpan.Zero)
                    }
                ]
            });
        }
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public AuthSession? CurrentSession { get; } = new()
        {
            UserId = UserId,
            Email = "ai@example.com",
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

    private sealed class AnonymousAuthenticationService : IAuthenticationService
    {
        public AuthSession? CurrentSession => null;

        public bool IsAuthenticated => false;

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
}
