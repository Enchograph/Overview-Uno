using System.Text;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Presentation.ViewModels;

public sealed class ItemDetailViewModel
{
    public static ItemDetailViewModel Empty { get; } = new()
    {
        Title = "No item selected",
        TypeText = "Select an item to inspect its details.",
        TimeText = "Time information will appear here.",
        ReminderText = "Reminder settings will appear here.",
        RepeatText = "Repeat settings will appear here.",
        StatusText = "No status yet.",
        MetadataText = "Use View on an existing item or save a new item first.",
        IsEmpty = true
    };

    public Guid? ItemId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string TypeText { get; init; } = string.Empty;

    public string TimeText { get; init; } = string.Empty;

    public string LocationText { get; init; } = string.Empty;

    public string DescriptionText { get; init; } = string.Empty;

    public string ReminderText { get; init; } = string.Empty;

    public string RepeatText { get; init; } = string.Empty;

    public string StatusText { get; init; } = string.Empty;

    public string MetadataText { get; init; } = string.Empty;

    public string AccentColor { get; init; } = "#4F46E5";

    public bool HasLocation => !string.IsNullOrWhiteSpace(LocationText);

    public bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionText);

    public bool CanEdit => ItemId is not null;

    public bool IsEmpty { get; init; }

    public static ItemDetailViewModel FromItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemDetailViewModel
        {
            ItemId = item.Id,
            Title = item.Title,
            TypeText = GetTypeText(item.Type),
            TimeText = GetTimeText(item),
            LocationText = item.Location ?? string.Empty,
            DescriptionText = item.Description ?? string.Empty,
            ReminderText = GetReminderText(item.ReminderConfig),
            RepeatText = GetRepeatText(item.RepeatRule),
            StatusText = GetStatusText(item),
            MetadataText = GetMetadataText(item),
            AccentColor = string.IsNullOrWhiteSpace(item.Color) ? "#4F46E5" : item.Color!,
            IsEmpty = false
        };
    }

    private static string GetTypeText(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Schedule => "Schedule",
            ItemType.Task => "Task",
            ItemType.Note => "Note",
            _ => itemType.ToString()
        };
    }

    private static string GetTimeText(Item item)
    {
        return item.Type switch
        {
            ItemType.Schedule => BuildRangeText("Schedule", item.StartAt, item.EndAt, item.TimeZoneId),
            ItemType.Task => BuildTaskText(item),
            ItemType.Note => BuildNoteText(item),
            _ => "No time information."
        };
    }

    private static string BuildTaskText(Item item)
    {
        var builder = new StringBuilder();
        builder.Append(BuildRangeText("Planned", item.PlannedStartAt, item.PlannedEndAt, item.TimeZoneId));

        if (item.DeadlineAt is DateTimeOffset deadlineAt)
        {
            builder.AppendLine();
            builder.Append("Deadline: ");
            builder.Append(FormatDateTime(deadlineAt, item.TimeZoneId));
        }

        return builder.ToString();
    }

    private static string BuildNoteText(Item item)
    {
        var builder = new StringBuilder();
        builder.Append("Expected duration: ");
        builder.Append(item.ExpectedDurationMinutes is int minutes ? $"{minutes} min" : "Not set");

        if (item.TargetDate is DateOnly targetDate)
        {
            builder.AppendLine();
            builder.Append("Target date: ");
            builder.Append(targetDate.ToString("yyyy-MM-dd"));
        }

        return builder.ToString();
    }

    private static string BuildRangeText(string label, DateTimeOffset? startAt, DateTimeOffset? endAt, string timeZoneId)
    {
        if (startAt is null || endAt is null)
        {
            return $"{label}: Not set";
        }

        return $"{label}: {FormatDateTime(startAt.Value, timeZoneId)} - {FormatDateTime(endAt.Value, timeZoneId)}";
    }

    private static string GetReminderText(ReminderConfig reminderConfig)
    {
        if (!reminderConfig.IsEnabled || reminderConfig.Triggers.Count == 0)
        {
            return "Reminder: Off";
        }

        var triggerTexts = reminderConfig.Triggers
            .Select(trigger => $"{trigger.Channel} {trigger.MinutesBeforeStart} min before");

        return $"Reminder: {string.Join(", ", triggerTexts)}";
    }

    private static string GetRepeatText(RepeatRule repeatRule)
    {
        if (repeatRule.Frequency == RepeatFrequency.None)
        {
            return "Repeat: None";
        }

        var builder = new StringBuilder();
        builder.Append($"Repeat: {repeatRule.Frequency} every {Math.Max(1, repeatRule.Interval)}");

        if (repeatRule.UntilAt is DateTimeOffset untilAt)
        {
            builder.Append(" until ");
            builder.Append(untilAt.LocalDateTime.ToString("yyyy-MM-dd"));
        }

        return builder.ToString();
    }

    private static string GetStatusText(Item item)
    {
        var flags = new List<string>();

        flags.Add(item.IsCompleted ? "Completed" : "Active");

        if (item.IsImportant)
        {
            flags.Add("Important");
        }

        if (item.DeletedAt is not null)
        {
            flags.Add("Deleted");
        }

        return string.Join(" · ", flags);
    }

    private static string GetMetadataText(Item item)
    {
        return $"Updated {item.LastModifiedAt.LocalDateTime:yyyy-MM-dd HH:mm} · Time zone {item.TimeZoneId}";
    }

    private static string FormatDateTime(DateTimeOffset value, string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var localValue = TimeZoneInfo.ConvertTime(value, timeZone);
            return localValue.ToString("yyyy-MM-dd HH:mm");
        }
        catch (TimeZoneNotFoundException)
        {
            return value.LocalDateTime.ToString("yyyy-MM-dd HH:mm");
        }
        catch (InvalidTimeZoneException)
        {
            return value.LocalDateTime.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
