using Overview.Client.Application.Ai;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;
using Overview.Client.Infrastructure.Api.Ai;
using Overview.Client.Infrastructure.Persistence.Repositories;

namespace Overview.Client.Tests;

public sealed class AiChatServiceTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task GetTodaySnapshotAsync_ReturnsOnlyMessagesForResolvedLocalDate()
    {
        var repository = new FakeAiChatMessageRepository();
        repository.Messages.Add(new AiChatMessage
        {
            UserId = UserId,
            OccurredOn = new DateOnly(2026, 3, 14),
            Role = AiChatRole.User,
            Message = "Today",
            CreatedAt = new DateTimeOffset(2026, 3, 13, 16, 30, 0, TimeSpan.Zero)
        });
        repository.Messages.Add(new AiChatMessage
        {
            UserId = UserId,
            OccurredOn = new DateOnly(2026, 3, 13),
            Role = AiChatRole.Assistant,
            Message = "Yesterday",
            CreatedAt = new DateTimeOffset(2026, 3, 12, 16, 30, 0, TimeSpan.Zero)
        });

        var service = CreateService(
            repository,
            timeProvider: new FixedTimeProvider(new DateTimeOffset(2026, 3, 13, 16, 30, 0, TimeSpan.Zero)));

        var snapshot = await service.GetTodaySnapshotAsync(UserId);

        Assert.Equal(new DateOnly(2026, 3, 14), snapshot.OccurredOn);
        Assert.Single(snapshot.Messages);
        Assert.Equal("Today", snapshot.Messages[0].Message);
    }

    [Fact]
    public async Task SendMessageAsync_PersistsUserAndAssistantMessagesOnCurrentDay()
    {
        var repository = new FakeAiChatMessageRepository();
        var remoteClient = new FakeAiRemoteClient
        {
            Response = "{\"intent\":\"answer_question\",\"answer\":\"Meeting is at 10.\"}"
        };
        var service = CreateService(
            repository,
            remoteClient,
            new FixedTimeProvider(new DateTimeOffset(2026, 3, 13, 16, 30, 0, TimeSpan.Zero)));

        var snapshot = await service.SendMessageAsync(UserId, "What's next?");

        Assert.Equal(2, snapshot.Messages.Count);
        Assert.Collection(
            snapshot.Messages,
            userMessage =>
            {
                Assert.Equal(AiChatRole.User, userMessage.Role);
                Assert.Equal("What's next?", userMessage.Message);
                Assert.Equal(new DateOnly(2026, 3, 14), userMessage.OccurredOn);
            },
            assistantMessage =>
            {
                Assert.Equal(AiChatRole.Assistant, assistantMessage.Role);
                Assert.Equal("{\"intent\":\"answer_question\",\"answer\":\"Meeting is at 10.\"}", assistantMessage.Message);
                Assert.Equal(new DateOnly(2026, 3, 14), assistantMessage.OccurredOn);
            });
        Assert.Equal("https://ai.example.com/v1", remoteClient.LastBaseUrl);
        Assert.Equal("secret-key", remoteClient.LastApiKey);
    }

    [Fact]
    public async Task SendMessageAsync_WithoutAiConfiguration_ThrowsAndDoesNotPersist()
    {
        var repository = new FakeAiChatMessageRepository();
        var service = CreateService(
            repository,
            settingsService: new FakeUserSettingsService
            {
                Settings = new UserSettings
                {
                    UserId = UserId,
                    TimeZoneId = "Asia/Shanghai",
                    AiBaseUrl = string.Empty,
                    AiApiKey = string.Empty,
                    AiModel = string.Empty
                }
            },
            orchestrationService: new FakeAiOrchestrationService
            {
                RequestPackage = new AiRequestPackage
                {
                    BaseUrl = string.Empty,
                    ApiKey = string.Empty,
                    Model = string.Empty,
                    HasRequiredConfiguration = false
                }
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendMessageAsync(UserId, "Hello"));

        Assert.Contains("AI settings are incomplete", exception.Message, StringComparison.Ordinal);
        Assert.Empty(repository.Messages);
    }

    [Fact]
    public async Task GetSnapshotAsync_ReturnsMessagesAcrossRequestedWeekRange()
    {
        var repository = new FakeAiChatMessageRepository();
        repository.Messages.Add(new AiChatMessage
        {
            UserId = UserId,
            OccurredOn = new DateOnly(2026, 3, 9),
            Role = AiChatRole.User,
            Message = "Monday",
            CreatedAt = new DateTimeOffset(2026, 3, 9, 1, 0, 0, TimeSpan.Zero)
        });
        repository.Messages.Add(new AiChatMessage
        {
            UserId = UserId,
            OccurredOn = new DateOnly(2026, 3, 12),
            Role = AiChatRole.Assistant,
            Message = "Thursday",
            CreatedAt = new DateTimeOffset(2026, 3, 12, 2, 0, 0, TimeSpan.Zero)
        });
        repository.Messages.Add(new AiChatMessage
        {
            UserId = UserId,
            OccurredOn = new DateOnly(2026, 3, 18),
            Role = AiChatRole.Assistant,
            Message = "Outside week",
            CreatedAt = new DateTimeOffset(2026, 3, 18, 2, 0, 0, TimeSpan.Zero)
        });

        var service = CreateService(repository);

        var snapshot = await service.GetSnapshotAsync(UserId, new CalendarPeriod
        {
            Mode = TimeSelectionMode.Week,
            ReferenceDate = new DateOnly(2026, 3, 10),
            StartDate = new DateOnly(2026, 3, 9),
            EndDate = new DateOnly(2026, 3, 15)
        });

        Assert.Equal(TimeSelectionMode.Week, snapshot.Period.Mode);
        Assert.Equal(2, snapshot.Messages.Count);
        Assert.Equal(["Monday", "Thursday"], snapshot.Messages.Select(message => message.Message).ToArray());
    }

    private static AiChatService CreateService(
        FakeAiChatMessageRepository repository,
        FakeAiRemoteClient? remoteClient = null,
        TimeProvider? timeProvider = null,
        FakeUserSettingsService? settingsService = null,
        FakeAiOrchestrationService? orchestrationService = null)
    {
        return new AiChatService(
            repository,
            orchestrationService ?? new FakeAiOrchestrationService(),
            settingsService ?? new FakeUserSettingsService(),
            remoteClient ?? new FakeAiRemoteClient(),
            timeProvider ?? new FixedTimeProvider(new DateTimeOffset(2026, 3, 13, 0, 0, 0, TimeSpan.Zero)));
    }

    private sealed class FakeAiChatMessageRepository : IAiChatMessageRepository
    {
        public List<AiChatMessage> Messages { get; } = [];

        public Task<IReadOnlyList<AiChatMessage>> ListByDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
        {
            var filtered = Messages
                .Where(message => message.UserId == userId && message.OccurredOn >= startDate && message.OccurredOn <= endDate)
                .OrderBy(message => message.CreatedAt)
                .ToArray();
            return Task.FromResult<IReadOnlyList<AiChatMessage>>(filtered);
        }

        public Task UpsertAsync(AiChatMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAiRemoteClient : IAiRemoteClient
    {
        public string Response { get; set; } = "AI response";

        public string LastBaseUrl { get; private set; } = string.Empty;

        public string LastApiKey { get; private set; } = string.Empty;

        public Task<string> CompleteChatAsync(string baseUrl, string apiKey, AiChatCompletionRequest request, CancellationToken cancellationToken = default)
        {
            LastBaseUrl = baseUrl;
            LastApiKey = apiKey;
            return Task.FromResult(Response);
        }
    }

    private sealed class FakeAiOrchestrationService : IAiOrchestrationService
    {
        public AiRequestPackage RequestPackage { get; set; } = new()
        {
            BaseUrl = "https://ai.example.com/v1",
            ApiKey = "secret-key",
            Model = "gpt-4.1-mini",
            HasRequiredConfiguration = true,
            RequestType = AiRequestType.AnswerQuestion,
            RequestBody = new AiChatCompletionRequest
            {
                Model = "gpt-4.1-mini",
                Messages =
                [
                    new AiChatCompletionMessage("system", "system"),
                    new AiChatCompletionMessage("user", "user")
                ]
            }
        };

        public Task<AiRequestPackage> BuildRequestAsync(Guid userId, string userMessage, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RequestPackage with { UserMessage = userMessage });
        }

        public Task<IReadOnlyList<AiItemSummary>> SearchRelevantItemsAsync(Guid userId, string userMessage, int maxCount = 8, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public AiParseResult ParseResponse(string responseContent)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeUserSettingsService : IUserSettingsService
    {
        public UserSettings Settings { get; set; } = new()
        {
            UserId = UserId,
            TimeZoneId = "Asia/Shanghai",
            AiBaseUrl = "https://ai.example.com/v1",
            AiApiKey = "secret-key",
            AiModel = "gpt-4.1-mini",
            SourceDeviceId = "device"
        };

        public Task<UserSettings> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Settings);
        }

        public Task<UserSettings> SaveAsync(Guid userId, UserSettingsUpdateRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            this.utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
