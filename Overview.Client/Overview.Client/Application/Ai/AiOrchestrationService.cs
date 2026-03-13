using System.Text;
using System.Text.Json;
using Overview.Client.Application.Items;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Ai;

public sealed class AiOrchestrationService : IAiOrchestrationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IItemService itemService;
    private readonly IUserSettingsService userSettingsService;

    public AiOrchestrationService(
        IItemService itemService,
        IUserSettingsService userSettingsService)
    {
        this.itemService = itemService;
        this.userSettingsService = userSettingsService;
    }

    public async Task<AiRequestPackage> BuildRequestAsync(
        Guid userId,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException("AI user message is required.", nameof(userMessage));
        }

        var normalizedMessage = userMessage.Trim();
        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var requestType = DetectRequestType(normalizedMessage);
        var relevantItems = await SearchRelevantItemsAsync(userId, normalizedMessage, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var systemPrompt = BuildSystemPrompt(settings.TimeZoneId, requestType);
        var summariesJson = JsonSerializer.Serialize(relevantItems, JsonOptions);

        return new AiRequestPackage
        {
            BaseUrl = settings.AiBaseUrl,
            ApiKey = settings.AiApiKey,
            Model = settings.AiModel,
            HasRequiredConfiguration = !string.IsNullOrWhiteSpace(settings.AiBaseUrl)
                && !string.IsNullOrWhiteSpace(settings.AiApiKey)
                && !string.IsNullOrWhiteSpace(settings.AiModel),
            RequestType = requestType,
            SystemPrompt = systemPrompt,
            UserMessage = normalizedMessage,
            RelevantItems = relevantItems,
            RequestBody = new AiChatCompletionRequest
            {
                Model = settings.AiModel,
                Messages = new[]
                {
                    new AiChatCompletionMessage("system", systemPrompt),
                    new AiChatCompletionMessage("user", normalizedMessage),
                    new AiChatCompletionMessage("user", $"Relevant item summaries (JSON array):\n{summariesJson}")
                }
            }
        };
    }

    public async Task<IReadOnlyList<AiItemSummary>> SearchRelevantItemsAsync(
        Guid userId,
        string userMessage,
        int maxCount = 8,
        CancellationToken cancellationToken = default)
    {
        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount, "Max count must be greater than zero.");
        }

        var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var timeZone = ResolveTimeZone(settings.TimeZoneId);
        var requestType = DetectRequestType(userMessage);
        if (requestType == AiRequestType.CreateItem)
        {
            return Array.Empty<AiItemSummary>();
        }

        var items = await itemService.ListAsync(userId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (items.Count == 0)
        {
            return Array.Empty<AiItemSummary>();
        }

        var normalizedMessage = userMessage.Trim();
        var tokens = Tokenize(normalizedMessage);
        var scoredItems = items
            .Select(item => new
            {
                Item = item,
                Score = ScoreItem(item, normalizedMessage, tokens, timeZone)
            })
            .OrderByDescending(entry => entry.Score)
            .ThenByDescending(entry => entry.Item.LastModifiedAt)
            .ThenBy(entry => entry.Item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        var selectedItems = scoredItems.Any(entry => entry.Score > 0)
            ? scoredItems.Where(entry => entry.Score > 0).Take(maxCount).Select(entry => entry.Item)
            : scoredItems.Take(Math.Min(maxCount, 5)).Select(entry => entry.Item);

        return selectedItems.Select(item => ToSummary(item, timeZone)).ToArray();
    }

    public AiParseResult ParseResponse(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return new AiParseResult
            {
                ValidationErrors = new[] { "AI response content is empty." }
            };
        }

        try
        {
            var normalizedJson = ExtractJson(responseContent);
            using var document = JsonDocument.Parse(normalizedJson);
            var root = document.RootElement;
            var errors = new List<string>();

            if (!TryParseIntent(root, out var intent))
            {
                errors.Add("Field 'intent' is missing or unsupported.");
            }

            var itemType = TryParseItemType(root, out var parsedItemType) ? parsedItemType : null;
            if (root.TryGetProperty("itemType", out _) && itemType is null)
            {
                errors.Add("Field 'itemType' is unsupported.");
            }

            var response = new AiStructuredResponse
            {
                Intent = intent,
                ItemType = itemType,
                Title = ReadOptionalString(root, "title"),
                Description = ReadOptionalString(root, "description"),
                StartAt = ReadOptionalDateTimeOffset(root, "startAt"),
                EndAt = ReadOptionalDateTimeOffset(root, "endAt"),
                DeadlineAt = ReadOptionalDateTimeOffset(root, "deadlineAt"),
                Location = ReadOptionalString(root, "location"),
                IsImportant = ReadOptionalBoolean(root, "importance"),
                Reminder = ReadReminder(root),
                RepeatRule = ReadRepeatRule(root),
                Confidence = ReadConfidence(root, errors),
                Answer = ReadOptionalString(root, "answer")
            };

            ValidateResponse(response, errors);
            return new AiParseResult
            {
                Response = response,
                ValidationErrors = errors
            };
        }
        catch (JsonException exception)
        {
            return new AiParseResult
            {
                ValidationErrors = new[] { $"AI response is not valid JSON: {exception.Message}" }
            };
        }
    }

    private static string BuildSystemPrompt(string timeZoneId, AiRequestType requestType)
    {
        var builder = new StringBuilder();
        builder.AppendLine("You are the AI assistant for the Overview app.");
        builder.AppendLine("Return a single JSON object only. Do not wrap it in markdown.");
        builder.AppendLine("Never invent items or fields that are not supported by the provided summaries.");
        builder.AppendLine("If information is missing, return intent 'clarify' and explain what is missing in 'answer'.");
        builder.AppendLine("Supported intents: create_item, delete_item, query_items, answer_question, clarify.");
        builder.AppendLine("Supported item types: schedule, task, note.");
        builder.AppendLine("When the user asks about existing items, rely only on the provided relevant item summaries.");
        builder.AppendLine("Do not rely on prior chat history. Treat this request independently.");
        builder.AppendLine($"Current request type hint: {ToIntentString(requestType)}.");
        builder.AppendLine($"Use timezone '{timeZoneId}'.");
        builder.AppendLine("Expected JSON fields: intent, itemType, title, description, startAt, endAt, deadlineAt, location, importance, reminder, repeatRule, confidence, answer.");
        return builder.ToString().Trim();
    }

    private static AiRequestType DetectRequestType(string userMessage)
    {
        var normalized = userMessage.Trim().ToLowerInvariant();
        if (ContainsAny(normalized, "delete", "remove", "cancel", "删", "删除", "取消"))
        {
            return AiRequestType.DeleteItem;
        }

        if (ContainsAny(normalized, "create", "add", "schedule", "plan", "new ", "创建", "新增", "添加", "安排"))
        {
            return AiRequestType.CreateItem;
        }

        if (ContainsAny(normalized, "which", "what", "show", "list", "find", "query", "查", "查询", "列出", "看看"))
        {
            return AiRequestType.QueryItems;
        }

        return AiRequestType.AnswerQuestion;
    }

    private static int ScoreItem(
        Item item,
        string normalizedMessage,
        IReadOnlyList<string> tokens,
        TimeZoneInfo timeZone)
    {
        var score = 0;
        if (string.IsNullOrWhiteSpace(normalizedMessage))
        {
            return score;
        }

        score += ScoreText(item.Title, normalizedMessage, tokens, 40, 10);
        score += ScoreText(item.Description, normalizedMessage, tokens, 18, 5);
        score += ScoreText(item.Location, normalizedMessage, tokens, 18, 5);

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone).DateTime);
        if (ContainsAny(normalizedMessage, "today", "今天") && IsRelevantOnDate(item, today, timeZone))
        {
            score += 15;
        }

        if (ContainsAny(normalizedMessage, "tomorrow", "明天") && IsRelevantOnDate(item, today.AddDays(1), timeZone))
        {
            score += 12;
        }

        if (ContainsAny(normalizedMessage, "this week", "本周", "这周") && IsWithinDateWindow(item, today, today.AddDays(6), timeZone))
        {
            score += 10;
        }

        if (ContainsAny(normalizedMessage, "important", "重要") && item.IsImportant)
        {
            score += 8;
        }

        if (ContainsAny(normalizedMessage, "completed", "done", "完成", "已完成") && item.IsCompleted)
        {
            score += 8;
        }

        return score;
    }

    private static int ScoreText(
        string? value,
        string normalizedMessage,
        IReadOnlyList<string> tokens,
        int fullMatchScore,
        int tokenMatchScore)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        var normalizedValue = value.Trim().ToLowerInvariant();
        var score = normalizedValue.Contains(normalizedMessage, StringComparison.Ordinal) ? fullMatchScore : 0;
        foreach (var token in tokens)
        {
            if (token.Length >= 2 && normalizedValue.Contains(token, StringComparison.Ordinal))
            {
                score += tokenMatchScore;
            }
        }

        return score;
    }

    private static IReadOnlyList<string> Tokenize(string value)
    {
        var tokens = new List<string>();
        var builder = new StringBuilder();
        foreach (var character in value.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                continue;
            }

            FlushToken(builder, tokens);
        }

        FlushToken(builder, tokens);
        return tokens;
    }

    private static void FlushToken(StringBuilder builder, List<string> tokens)
    {
        if (builder.Length == 0)
        {
            return;
        }

        tokens.Add(builder.ToString());
        builder.Clear();
    }

    private static AiItemSummary ToSummary(Item item, TimeZoneInfo timeZone)
    {
        return new AiItemSummary
        {
            Id = item.Id,
            ItemType = ToItemTypeString(item.Type),
            Title = item.Title,
            Description = item.Description,
            Location = item.Location,
            IsImportant = item.IsImportant,
            IsCompleted = item.IsCompleted,
            TimeSummary = BuildTimeSummary(item, timeZone),
            StartAt = item.Type == ItemType.Schedule ? item.StartAt : item.PlannedStartAt,
            EndAt = item.Type == ItemType.Schedule ? item.EndAt : item.PlannedEndAt,
            DeadlineAt = item.DeadlineAt,
            ExpectedDurationMinutes = item.ExpectedDurationMinutes,
            TargetDate = item.TargetDate
        };
    }

    private static string BuildTimeSummary(Item item, TimeZoneInfo timeZone)
    {
        return item.Type switch
        {
            ItemType.Schedule when item.StartAt is not null && item.EndAt is not null
                => $"schedule {FormatLocal(item.StartAt.Value, timeZone)} -> {FormatLocal(item.EndAt.Value, timeZone)}",
            ItemType.Task when item.PlannedStartAt is not null && item.PlannedEndAt is not null
                => $"task {FormatLocal(item.PlannedStartAt.Value, timeZone)} -> {FormatLocal(item.PlannedEndAt.Value, timeZone)}, deadline {FormatLocal(item.DeadlineAt ?? item.PlannedEndAt.Value, timeZone)}",
            ItemType.Note when item.TargetDate is not null
                => $"note target date {item.TargetDate:O}, expected duration {item.ExpectedDurationMinutes ?? 0} minutes",
            ItemType.Note
                => $"note expected duration {item.ExpectedDurationMinutes ?? 0} minutes",
            _ => "no time information"
        };
    }

    private static string FormatLocal(DateTimeOffset value, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTime(value, timeZone).ToString("yyyy-MM-dd HH:mm zzz");
    }

    private static bool TryParseIntent(JsonElement root, out AiRequestType intent)
    {
        if (root.TryGetProperty("intent", out var property))
        {
            var raw = property.GetString();
            if (!string.IsNullOrWhiteSpace(raw) && TryMapIntent(raw, out intent))
            {
                return true;
            }
        }

        intent = AiRequestType.AnswerQuestion;
        return false;
    }

    private static bool TryMapIntent(string value, out AiRequestType intent)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "create_item":
                intent = AiRequestType.CreateItem;
                return true;
            case "delete_item":
                intent = AiRequestType.DeleteItem;
                return true;
            case "query_items":
                intent = AiRequestType.QueryItems;
                return true;
            case "answer_question":
                intent = AiRequestType.AnswerQuestion;
                return true;
            case "clarify":
                intent = AiRequestType.Clarify;
                return true;
            default:
                intent = default;
                return false;
        }
    }

    private static bool TryParseItemType(JsonElement root, out ItemType? itemType)
    {
        if (!root.TryGetProperty("itemType", out var property))
        {
            itemType = null;
            return false;
        }

        var raw = property.GetString();
        itemType = raw?.Trim().ToLowerInvariant() switch
        {
            "schedule" or "日程" => Domain.Enums.ItemType.Schedule,
            "task" or "任务" => Domain.Enums.ItemType.Task,
            "note" or "memo" or "备忘" => Domain.Enums.ItemType.Note,
            _ => null
        };

        return true;
    }

    private static string? ReadOptionalString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var value = property.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool? ReadOptionalBoolean(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.True
            ? true
            : property.ValueKind == JsonValueKind.False
                ? false
                : null;
    }

    private static DateTimeOffset? ReadOptionalDateTimeOffset(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return DateTimeOffset.TryParse(property.GetString(), out var value) ? value : null;
    }

    private static double ReadConfidence(JsonElement root, List<string> errors)
    {
        if (!root.TryGetProperty("confidence", out var property))
        {
            errors.Add("Field 'confidence' is required.");
            return 0;
        }

        var confidence = property.ValueKind switch
        {
            JsonValueKind.Number => property.GetDouble(),
            JsonValueKind.String when double.TryParse(property.GetString(), out var value) => value,
            _ => double.NaN
        };

        if (double.IsNaN(confidence) || confidence < 0 || confidence > 1)
        {
            errors.Add("Field 'confidence' must be a number between 0 and 1.");
            return 0;
        }

        return confidence;
    }

    private static AiReminderInstruction? ReadReminder(JsonElement root)
    {
        if (!root.TryGetProperty("reminder", out var property) || property.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var minutes = new List<int>();
        if (property.TryGetProperty("minutesBefore", out var minutesBefore) && minutesBefore.ValueKind == JsonValueKind.Number)
        {
            minutes.Add(minutesBefore.GetInt32());
        }

        if (property.TryGetProperty("minutesBeforeStart", out var minutesBeforeStart) && minutesBeforeStart.ValueKind == JsonValueKind.Number)
        {
            minutes.Add(minutesBeforeStart.GetInt32());
        }

        if (property.TryGetProperty("triggers", out var triggers) && triggers.ValueKind == JsonValueKind.Array)
        {
            foreach (var trigger in triggers.EnumerateArray())
            {
                if (trigger.ValueKind == JsonValueKind.Number)
                {
                    minutes.Add(trigger.GetInt32());
                    continue;
                }

                if (trigger.ValueKind == JsonValueKind.Object &&
                    trigger.TryGetProperty("minutesBeforeStart", out var triggerMinutes) &&
                    triggerMinutes.ValueKind == JsonValueKind.Number)
                {
                    minutes.Add(triggerMinutes.GetInt32());
                }
            }
        }

        return new AiReminderInstruction
        {
            IsEnabled = property.TryGetProperty("enabled", out var enabled) && enabled.ValueKind == JsonValueKind.True,
            MinutesBeforeStart = minutes.Distinct().OrderBy(value => value).ToArray()
        };
    }

    private static AiRepeatRuleInstruction? ReadRepeatRule(JsonElement root)
    {
        if (!root.TryGetProperty("repeatRule", out var property) || property.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var frequency = RepeatFrequency.None;
        if (property.TryGetProperty("frequency", out var frequencyProperty))
        {
            frequency = frequencyProperty.GetString()?.Trim().ToLowerInvariant() switch
            {
                "daily" or "day" => RepeatFrequency.Daily,
                "weekly" or "week" => RepeatFrequency.Weekly,
                "monthly" or "month" => RepeatFrequency.Monthly,
                "yearly" or "year" => RepeatFrequency.Yearly,
                _ => RepeatFrequency.None
            };
        }

        var daysOfWeek = new List<DayOfWeek>();
        if (property.TryGetProperty("daysOfWeek", out var daysProperty) && daysProperty.ValueKind == JsonValueKind.Array)
        {
            foreach (var day in daysProperty.EnumerateArray())
            {
                if (Enum.TryParse<DayOfWeek>(day.GetString(), true, out var parsedDay))
                {
                    daysOfWeek.Add(parsedDay);
                }
            }
        }

        return new AiRepeatRuleInstruction
        {
            Frequency = frequency,
            Interval = property.TryGetProperty("interval", out var interval) && interval.ValueKind == JsonValueKind.Number
                ? Math.Max(1, interval.GetInt32())
                : 1,
            DaysOfWeek = daysOfWeek,
            DayOfMonth = property.TryGetProperty("dayOfMonth", out var dayOfMonth) && dayOfMonth.ValueKind == JsonValueKind.Number
                ? dayOfMonth.GetInt32()
                : null,
            MonthOfYear = property.TryGetProperty("monthOfYear", out var monthOfYear) && monthOfYear.ValueKind == JsonValueKind.Number
                ? monthOfYear.GetInt32()
                : null,
            UntilAt = property.TryGetProperty("untilAt", out var untilAt) && untilAt.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(untilAt.GetString(), out var parsedUntilAt)
                ? parsedUntilAt
                : null,
            Count = property.TryGetProperty("count", out var count) && count.ValueKind == JsonValueKind.Number
                ? count.GetInt32()
                : null
        };
    }

    private static void ValidateResponse(AiStructuredResponse response, List<string> errors)
    {
        switch (response.Intent)
        {
            case AiRequestType.CreateItem:
                if (response.ItemType is null)
                {
                    errors.Add("Field 'itemType' is required for create_item.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(response.Title))
                {
                    errors.Add("Field 'title' is required for create_item.");
                }

                switch (response.ItemType)
                {
                    case Domain.Enums.ItemType.Schedule:
                        ValidateTimeRange(response.StartAt, response.EndAt, "schedule", errors);
                        break;
                    case Domain.Enums.ItemType.Task:
                        ValidateTimeRange(response.StartAt, response.EndAt, "task", errors);
                        if (response.DeadlineAt is null)
                        {
                            errors.Add("Field 'deadlineAt' is required for task creation.");
                        }

                        break;
                    case Domain.Enums.ItemType.Note:
                        break;
                }

                break;
            case AiRequestType.AnswerQuestion:
            case AiRequestType.Clarify:
                if (string.IsNullOrWhiteSpace(response.Answer))
                {
                    errors.Add("Field 'answer' is required for answer_question and clarify.");
                }

                break;
        }
    }

    private static void ValidateTimeRange(
        DateTimeOffset? startAt,
        DateTimeOffset? endAt,
        string itemType,
        List<string> errors)
    {
        if (startAt is null || endAt is null)
        {
            errors.Add($"Fields 'startAt' and 'endAt' are required for {itemType} creation.");
            return;
        }

        if (endAt <= startAt)
        {
            errors.Add($"Field 'endAt' must be after 'startAt' for {itemType} creation.");
        }
    }

    private static string ExtractJson(string responseContent)
    {
        var trimmed = responseContent.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstLineBreak = trimmed.IndexOf('\n');
        if (firstLineBreak < 0)
        {
            return trimmed;
        }

        var withoutFenceHeader = trimmed[(firstLineBreak + 1)..];
        var closingFence = withoutFenceHeader.LastIndexOf("```", StringComparison.Ordinal);
        return closingFence >= 0
            ? withoutFenceHeader[..closingFence].Trim()
            : withoutFenceHeader.Trim();
    }

    private static bool ContainsAny(string value, params string[] candidates)
    {
        return candidates.Any(candidate => value.Contains(candidate, StringComparison.Ordinal));
    }

    private static bool IsRelevantOnDate(Item item, DateOnly date, TimeZoneInfo timeZone)
    {
        return TryGetRelevantRange(item, timeZone, out var startDate, out var endDate)
            && startDate <= date
            && endDate >= date;
    }

    private static bool IsWithinDateWindow(
        Item item,
        DateOnly startDate,
        DateOnly endDate,
        TimeZoneInfo timeZone)
    {
        return TryGetRelevantRange(item, timeZone, out var itemStartDate, out var itemEndDate)
            && itemStartDate <= endDate
            && itemEndDate >= startDate;
    }

    private static bool TryGetRelevantRange(
        Item item,
        TimeZoneInfo timeZone,
        out DateOnly startDate,
        out DateOnly endDate)
    {
        if (item.Type == ItemType.Schedule && item.StartAt is not null && item.EndAt is not null)
        {
            startDate = ToDateOnly(item.StartAt.Value, timeZone);
            endDate = ToDateOnly(item.EndAt.Value.AddTicks(-1), timeZone);
            return true;
        }

        if (item.Type == ItemType.Task)
        {
            if (item.PlannedStartAt is not null && item.PlannedEndAt is not null)
            {
                startDate = ToDateOnly(item.PlannedStartAt.Value, timeZone);
                endDate = ToDateOnly(item.PlannedEndAt.Value.AddTicks(-1), timeZone);
                return true;
            }

            if (item.DeadlineAt is not null)
            {
                startDate = ToDateOnly(item.DeadlineAt.Value, timeZone);
                endDate = startDate;
                return true;
            }
        }

        if (item.Type == ItemType.Note && item.TargetDate is not null)
        {
            startDate = item.TargetDate.Value;
            endDate = startDate;
            return true;
        }

        startDate = default;
        endDate = default;
        return false;
    }

    private static DateOnly ToDateOnly(DateTimeOffset value, TimeZoneInfo timeZone)
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(value, timeZone).DateTime);
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Local;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Local;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Local;
        }
    }

    private static string ToIntentString(AiRequestType requestType)
    {
        return requestType switch
        {
            AiRequestType.CreateItem => "create_item",
            AiRequestType.DeleteItem => "delete_item",
            AiRequestType.QueryItems => "query_items",
            AiRequestType.AnswerQuestion => "answer_question",
            AiRequestType.Clarify => "clarify",
            _ => "answer_question"
        };
    }

    private static string ToItemTypeString(ItemType itemType)
    {
        return itemType switch
        {
            Domain.Enums.ItemType.Schedule => "schedule",
            Domain.Enums.ItemType.Task => "task",
            Domain.Enums.ItemType.Note => "note",
            _ => "task"
        };
    }
}
