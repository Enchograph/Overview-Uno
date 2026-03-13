using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;
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
        var snapshot = await BuildSnapshotAsync(
            userId,
            CreateDayPeriod(occurredOn),
            settings.TimeZoneId,
            cancellationToken).ConfigureAwait(false);
        return new AiChatDaySnapshot
        {
            OccurredOn = occurredOn,
            TimeZoneId = snapshot.TimeZoneId,
            Messages = snapshot.Messages
        };
    }

    public async Task<AiChatPeriodSnapshot> GetSnapshotAsync(
        Guid userId,
        CalendarPeriod period,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(period);

        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        return await BuildSnapshotAsync(userId, period, settings.TimeZoneId, cancellationToken).ConfigureAwait(false);
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

        var snapshot = await BuildSnapshotAsync(
            userId,
            CreateDayPeriod(occurredOn),
            settings.TimeZoneId,
            cancellationToken).ConfigureAwait(false);
        return new AiChatDaySnapshot
        {
            OccurredOn = occurredOn,
            TimeZoneId = snapshot.TimeZoneId,
            Messages = snapshot.Messages
        };
    }

    private async Task<AiChatPeriodSnapshot> BuildSnapshotAsync(
        Guid userId,
        CalendarPeriod period,
        string timeZoneId,
        CancellationToken cancellationToken)
    {
        var messages = await aiChatMessageRepository.ListByDateRangeAsync(
            userId,
            period.StartDate,
            period.EndDate,
            cancellationToken).ConfigureAwait(false);

        return new AiChatPeriodSnapshot
        {
            Period = period,
            TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId,
            Messages = messages.OrderBy(message => message.CreatedAt).ToArray()
        };
    }

    private static CalendarPeriod CreateDayPeriod(DateOnly occurredOn)
    {
        return new CalendarPeriod
        {
            Mode = TimeSelectionMode.Day,
            ReferenceDate = occurredOn,
            StartDate = occurredOn,
            EndDate = occurredOn
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
