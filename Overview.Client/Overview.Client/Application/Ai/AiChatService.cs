using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Infrastructure.Api.Ai;
using Overview.Client.Infrastructure.Persistence.Repositories;

namespace Overview.Client.Application.Ai;

public sealed class AiChatService : IAiChatService
{
    private readonly IAiChatMessageRepository aiChatMessageRepository;
    private readonly IAiOrchestrationService aiOrchestrationService;
    private readonly IUserSettingsService userSettingsService;
    private readonly IAiRemoteClient aiRemoteClient;
    private readonly TimeProvider timeProvider;

    public AiChatService(
        IAiChatMessageRepository aiChatMessageRepository,
        IAiOrchestrationService aiOrchestrationService,
        IUserSettingsService userSettingsService,
        IAiRemoteClient aiRemoteClient,
        TimeProvider timeProvider)
    {
        this.aiChatMessageRepository = aiChatMessageRepository;
        this.aiOrchestrationService = aiOrchestrationService;
        this.userSettingsService = userSettingsService;
        this.aiRemoteClient = aiRemoteClient;
        this.timeProvider = timeProvider;
    }

    public async Task<AiChatDaySnapshot> GetTodaySnapshotAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var occurredOn = ResolveOccurredOn(settings.TimeZoneId, timeProvider.GetUtcNow());
        return await GetSnapshotAsync(userId, occurredOn, settings.TimeZoneId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AiChatDaySnapshot> SendMessageAsync(
        Guid userId,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException("AI message is required.", nameof(userMessage));
        }

        var requestPackage = await aiOrchestrationService.BuildRequestAsync(userId, userMessage, cancellationToken)
            .ConfigureAwait(false);
        if (!requestPackage.HasRequiredConfiguration)
        {
            throw new InvalidOperationException(
                "AI settings are incomplete. Configure Base URL, API Key, and Model in Settings before sending messages.");
        }

        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow();
        var occurredOn = ResolveOccurredOn(settings.TimeZoneId, now);
        var linkedItemIds = requestPackage.RelevantItems.Select(item => item.Id).ToArray();

        var assistantReply = await aiRemoteClient.CompleteChatAsync(
            requestPackage.BaseUrl,
            requestPackage.ApiKey,
            requestPackage.RequestBody,
            cancellationToken).ConfigureAwait(false);
        var normalizedReply = string.IsNullOrWhiteSpace(assistantReply)
            ? "AI returned an empty response."
            : assistantReply.Trim();

        await aiChatMessageRepository.UpsertAsync(new AiChatMessage
        {
            UserId = userId,
            OccurredOn = occurredOn,
            Role = Domain.Enums.AiChatRole.User,
            Message = userMessage.Trim(),
            CreatedAt = now,
            RequestType = requestPackage.RequestType,
            LinkedItemIds = linkedItemIds
        }, cancellationToken).ConfigureAwait(false);

        await aiChatMessageRepository.UpsertAsync(new AiChatMessage
        {
            UserId = userId,
            OccurredOn = occurredOn,
            Role = Domain.Enums.AiChatRole.Assistant,
            Message = normalizedReply,
            CreatedAt = timeProvider.GetUtcNow(),
            RequestType = requestPackage.RequestType,
            LinkedItemIds = linkedItemIds
        }, cancellationToken).ConfigureAwait(false);

        return await GetSnapshotAsync(userId, occurredOn, settings.TimeZoneId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AiChatDaySnapshot> GetSnapshotAsync(
        Guid userId,
        DateOnly occurredOn,
        string timeZoneId,
        CancellationToken cancellationToken)
    {
        var messages = await aiChatMessageRepository.ListByDateRangeAsync(
            userId,
            occurredOn,
            occurredOn,
            cancellationToken).ConfigureAwait(false);

        return new AiChatDaySnapshot
        {
            OccurredOn = occurredOn,
            TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId,
            Messages = messages.OrderBy(message => message.CreatedAt).ToArray()
        };
    }

    private static DateOnly ResolveOccurredOn(string? timeZoneId, DateTimeOffset utcNow)
    {
        var timeZone = ResolveTimeZone(timeZoneId);
        var localNow = TimeZoneInfo.ConvertTime(utcNow, timeZone);
        return DateOnly.FromDateTime(localNow.DateTime);
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}
