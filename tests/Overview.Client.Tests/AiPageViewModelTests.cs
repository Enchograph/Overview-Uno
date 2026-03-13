using Overview.Client.Application.Ai;
using Overview.Client.Application.Auth;
using Overview.Client.Application.Home;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Tests;

public sealed class AiPageViewModelTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task InitializeAsync_LoadsTodayMessagesForAuthenticatedUser()
    {
        var chatService = new FakeAiChatService();
        var viewModel = new AiPageViewModel(
            new FakeAuthenticationService(),
            chatService,
            new FakeTimeSelectionService());

        await viewModel.InitializeAsync();

        Assert.Equal("2026-03-13", viewModel.CurrentPeriodTitle);
        Assert.Single(viewModel.Messages);
        Assert.Equal("AI", viewModel.Messages[0].SpeakerLabel);
        Assert.Contains("Loaded 1 AI messages", viewModel.StatusMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendAsync_ClearsDraftAndRefreshesSelectedPeriod()
    {
        var chatService = new FakeAiChatService();
        var viewModel = new AiPageViewModel(
            new FakeAuthenticationService(),
            chatService,
            new FakeTimeSelectionService());

        await viewModel.InitializeAsync();
        viewModel.UpdateDraft("Add a task for tonight");

        await viewModel.SendAsync();

        Assert.Equal(string.Empty, viewModel.DraftMessage);
        Assert.Equal(2, viewModel.Messages.Count);
        Assert.Equal("Add a task for tonight", chatService.LastSentMessage);
        Assert.Equal("AI response stored and current period refreshed.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task SendAsync_WithoutAuthentication_ShowsSignInStatus()
    {
        var viewModel = new AiPageViewModel(
            new AnonymousAuthenticationService(),
            new FakeAiChatService(),
            new FakeTimeSelectionService());

        await viewModel.SendAsync();

        Assert.Equal("AI chat requires an authenticated account.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task SetSelectionModeAsync_LoadsWeekMessages()
    {
        var chatService = new FakeAiChatService();
        var viewModel = new AiPageViewModel(
            new FakeAuthenticationService(),
            chatService,
            new FakeTimeSelectionService());

        await viewModel.InitializeAsync();
        await viewModel.SetSelectionModeAsync(TimeSelectionMode.Week);

        Assert.Equal(TimeSelectionMode.Week, viewModel.CurrentSelectionMode);
        Assert.Equal("2026-03-09 to 2026-03-15", viewModel.CurrentPeriodTitle);
        Assert.Equal(2, viewModel.Messages.Count);
        Assert.Contains("selected week", viewModel.StatusMessage, StringComparison.Ordinal);
    }

    private sealed class FakeAiChatService : IAiChatService
    {
        public string LastSentMessage { get; private set; } = string.Empty;
        private bool hasSentMessage;

        public Task<AiChatPeriodSnapshot> GetSnapshotAsync(Guid userId, CalendarPeriod period, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AiChatMessage> messages = period.Mode switch
            {
                TimeSelectionMode.Week =>
                [
                    new AiChatMessage
                    {
                        UserId = userId,
                        OccurredOn = new DateOnly(2026, 3, 10),
                        Role = AiChatRole.User,
                        Message = "Week plan",
                        CreatedAt = new DateTimeOffset(2026, 3, 10, 8, 0, 0, TimeSpan.Zero)
                    },
                    new AiChatMessage
                    {
                        UserId = userId,
                        OccurredOn = new DateOnly(2026, 3, 12),
                        Role = AiChatRole.Assistant,
                        Message = "Weekly summary",
                        CreatedAt = new DateTimeOffset(2026, 3, 12, 8, 0, 0, TimeSpan.Zero)
                    }
                ],
                _ =>
                    hasSentMessage
                        ?
                        [
                            new AiChatMessage
                            {
                                UserId = userId,
                                OccurredOn = new DateOnly(2026, 3, 13),
                                Role = AiChatRole.User,
                                Message = LastSentMessage,
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
                        :
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
            };

            return Task.FromResult(new AiChatPeriodSnapshot
            {
                Period = period,
                Messages = messages
            });
        }

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
            hasSentMessage = true;
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

    private sealed class FakeTimeSelectionService : ITimeSelectionService
    {
        public Task<TimeSelectionSnapshot> BuildMonthSnapshotAsync(
            Guid userId,
            DateOnly visibleMonth,
            TimeSelectionMode selectionMode,
            DateOnly? selectedDate = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<CalendarPeriod> ResolveSelectionAsync(
            Guid userId,
            DateOnly selectedDate,
            TimeSelectionMode selectionMode,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(selectionMode switch
            {
                TimeSelectionMode.Week => new CalendarPeriod
                {
                    Mode = TimeSelectionMode.Week,
                    ReferenceDate = new DateOnly(2026, 3, 13),
                    StartDate = new DateOnly(2026, 3, 9),
                    EndDate = new DateOnly(2026, 3, 15)
                },
                TimeSelectionMode.Month => new CalendarPeriod
                {
                    Mode = TimeSelectionMode.Month,
                    ReferenceDate = new DateOnly(2026, 3, 1),
                    StartDate = new DateOnly(2026, 3, 1),
                    EndDate = new DateOnly(2026, 3, 31)
                },
                _ => new CalendarPeriod
                {
                    Mode = TimeSelectionMode.Day,
                    ReferenceDate = new DateOnly(2026, 3, 13),
                    StartDate = new DateOnly(2026, 3, 13),
                    EndDate = new DateOnly(2026, 3, 13)
                }
            });
        }

        public Task<CalendarPeriod> GetPreviousPeriodAsync(Guid userId, CalendarPeriod period, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<CalendarPeriod> GetNextPeriodAsync(Guid userId, CalendarPeriod period, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
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
