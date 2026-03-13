using Overview.Client.Application.Settings;
using Overview.Client.Application.Items;
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
    private readonly IItemService itemService;
    private readonly IUserSettingsService userSettingsService;
    private readonly IAiRemoteClient aiRemoteClient;
    private readonly TimeProvider timeProvider;

    public AiChatService(
        IAiChatMessageRepository aiChatMessageRepository,
        IAiOrchestrationService aiOrchestrationService,
        IItemService itemService,
        IUserSettingsService userSettingsService,
        IAiRemoteClient aiRemoteClient,
        TimeProvider timeProvider)
    {
        this.aiChatMessageRepository = aiChatMessageRepository;
        this.aiOrchestrationService = aiOrchestrationService;
        this.itemService = itemService;
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
        var executionResult = await ExecuteAssistantReplyAsync(
            userId,
            requestPackage,
            assistantReply,
            settings.TimeZoneId,
            cancellationToken).ConfigureAwait(false);

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
            Message = executionResult.Message,
            CreatedAt = timeProvider.GetUtcNow(),
            RequestType = requestPackage.RequestType,
            LinkedItemIds = executionResult.LinkedItemIds
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

    private async Task<AiAssistantExecutionResult> ExecuteAssistantReplyAsync(
        Guid userId,
        AiRequestPackage requestPackage,
        string assistantReply,
        string timeZoneId,
        CancellationToken cancellationToken)
    {
        var normalizedReply = string.IsNullOrWhiteSpace(assistantReply)
            ? "AI returned an empty response."
            : assistantReply.Trim();
        var parseResult = aiOrchestrationService.ParseResponse(normalizedReply);
        if (parseResult.Response is null)
        {
            return AiAssistantExecutionResult.Clarify(BuildValidationMessage(parseResult.ValidationErrors), requestPackage.RelevantItems.Select(item => item.Id));
        }

        var response = parseResult.Response;
        if (parseResult.ValidationErrors.Count > 0)
        {
            return response.Intent is AiRequestType.Clarify or AiRequestType.AnswerQuestion or AiRequestType.QueryItems
                ? new AiAssistantExecutionResult(response.Answer ?? BuildValidationMessage(parseResult.ValidationErrors), ResolveLinkedItemIds(response, requestPackage.RelevantItems))
                : AiAssistantExecutionResult.Clarify(BuildValidationMessage(parseResult.ValidationErrors), ResolveLinkedItemIds(response, requestPackage.RelevantItems));
        }

        return response.Intent switch
        {
            AiRequestType.CreateItem => parseResult.CanApplyWriteOperation
                ? await ExecuteCreateAsync(userId, response, timeZoneId, cancellationToken).ConfigureAwait(false)
                : AiAssistantExecutionResult.Clarify(BuildLowConfidenceMessage("create or update an item"), Array.Empty<Guid>()),
            AiRequestType.DeleteItem => parseResult.CanApplyWriteOperation
                ? await ExecuteDeleteAsync(userId, response, cancellationToken).ConfigureAwait(false)
                : AiAssistantExecutionResult.Clarify(BuildLowConfidenceMessage("delete an item"), ResolveLinkedItemIds(response, requestPackage.RelevantItems)),
            AiRequestType.QueryItems => new AiAssistantExecutionResult(
                response.Answer ?? "I could not summarize the matching items yet.",
                ResolveLinkedItemIds(response, requestPackage.RelevantItems)),
            AiRequestType.Clarify => new AiAssistantExecutionResult(
                response.Answer ?? "I need more detail before I can help with that.",
                ResolveLinkedItemIds(response, requestPackage.RelevantItems)),
            _ => new AiAssistantExecutionResult(
                response.Answer ?? normalizedReply,
                ResolveLinkedItemIds(response, requestPackage.RelevantItems))
        };
    }

    private async Task<AiAssistantExecutionResult> ExecuteCreateAsync(
        Guid userId,
        AiStructuredResponse response,
        string timeZoneId,
        CancellationToken cancellationToken)
    {
        var createdItem = await itemService.CreateAsync(
            userId,
            BuildCreateRequest(response, timeZoneId),
            cancellationToken).ConfigureAwait(false);

        var message = string.IsNullOrWhiteSpace(response.Answer)
            ? $"Created {createdItem.Type.ToString().ToLowerInvariant()} \"{createdItem.Title}\"."
            : response.Answer!;
        return new AiAssistantExecutionResult(message, [createdItem.Id]);
    }

    private async Task<AiAssistantExecutionResult> ExecuteDeleteAsync(
        Guid userId,
        AiStructuredResponse response,
        CancellationToken cancellationToken)
    {
        var deletedItems = new List<Item>();
        foreach (var itemId in response.ItemIds)
        {
            var item = await itemService.GetAsync(userId, itemId, includeDeleted: false, cancellationToken).ConfigureAwait(false);
            if (item is null)
            {
                return AiAssistantExecutionResult.Clarify(
                    "I could not safely match the item to delete. Please restate it with a clearer title or time.",
                    response.ItemIds);
            }

            deletedItems.Add(item);
        }

        foreach (var item in deletedItems)
        {
            await itemService.DeleteAsync(userId, item.Id, cancellationToken).ConfigureAwait(false);
        }

        var message = string.IsNullOrWhiteSpace(response.Answer)
            ? deletedItems.Count == 1
                ? $"Deleted \"{deletedItems[0].Title}\"."
                : $"Deleted {deletedItems.Count} items: {string.Join(", ", deletedItems.Select(item => $"\"{item.Title}\""))}."
            : response.Answer!;
        return new AiAssistantExecutionResult(message, deletedItems.Select(item => item.Id).ToArray());
    }

    private static ItemUpsertRequest BuildCreateRequest(AiStructuredResponse response, string timeZoneId)
    {
        var reminderConfig = response.Reminder is null
            ? new ReminderConfig()
            : new ReminderConfig
            {
                IsEnabled = response.Reminder.IsEnabled,
                Triggers = response.Reminder.MinutesBeforeStart
                    .Distinct()
                    .OrderBy(value => value)
                    .Select(value => new ReminderTrigger
                    {
                        MinutesBeforeStart = value
                    })
                    .ToArray()
            };

        var repeatRule = response.RepeatRule is null
            ? new RepeatRule()
            : new RepeatRule
            {
                Frequency = response.RepeatRule.Frequency,
                Interval = response.RepeatRule.Interval,
                DaysOfWeek = response.RepeatRule.DaysOfWeek,
                DayOfMonth = response.RepeatRule.DayOfMonth,
                MonthOfYear = response.RepeatRule.MonthOfYear,
                UntilAt = response.RepeatRule.UntilAt,
                Count = response.RepeatRule.Count
            };

        return new ItemUpsertRequest
        {
            Type = response.ItemType ?? ItemType.Task,
            Title = response.Title ?? string.Empty,
            Description = response.Description,
            Location = response.Location,
            Color = response.Color,
            IsImportant = response.IsImportant ?? false,
            ReminderConfig = reminderConfig,
            RepeatRule = repeatRule,
            TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId,
            StartAt = response.ItemType == ItemType.Schedule ? response.StartAt : null,
            EndAt = response.ItemType == ItemType.Schedule ? response.EndAt : null,
            PlannedStartAt = response.ItemType == ItemType.Task ? response.StartAt : null,
            PlannedEndAt = response.ItemType == ItemType.Task ? response.EndAt : null,
            DeadlineAt = response.ItemType == ItemType.Task ? response.DeadlineAt : null,
            ExpectedDurationMinutes = response.ItemType == ItemType.Note ? response.ExpectedDurationMinutes : null,
            TargetDate = response.ItemType == ItemType.Note ? response.TargetDate : null
        };
    }

    private static IReadOnlyList<Guid> ResolveLinkedItemIds(
        AiStructuredResponse response,
        IReadOnlyList<AiItemSummary> relevantItems)
    {
        if (response.ItemIds.Count > 0)
        {
            return response.ItemIds;
        }

        return response.Intent == AiRequestType.QueryItems
            ? relevantItems.Select(item => item.Id).ToArray()
            : Array.Empty<Guid>();
    }

    private static string BuildValidationMessage(IReadOnlyList<string> validationErrors)
    {
        return validationErrors.Count == 0
            ? "I need more detail before I can safely change your data."
            : $"I need more detail before I can safely change your data. {validationErrors[0]}";
    }

    private static string BuildLowConfidenceMessage(string action)
    {
        return $"I need more detail before I can safely {action}. Please confirm the exact item or provide the missing time details.";
    }

    private sealed record AiAssistantExecutionResult(string Message, IReadOnlyList<Guid> LinkedItemIds)
    {
        public static AiAssistantExecutionResult Clarify(string message, IEnumerable<Guid> linkedItemIds)
        {
            return new AiAssistantExecutionResult(message, linkedItemIds.Distinct().ToArray());
        }
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
