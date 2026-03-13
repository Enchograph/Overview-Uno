using Overview.Client.Application.Items;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Presentation.ViewModels;

public sealed class AddItemFormModel
{
    public ItemType Type { get; set; } = ItemType.Task;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Color { get; set; } = "#4F46E5";

    public bool IsImportant { get; set; }

    public bool IsCompleted { get; set; }

    public bool ReminderEnabled { get; set; }

    public int ReminderMinutesBeforeStart { get; set; } = 15;

    public RepeatFrequency RepeatFrequency { get; set; } = RepeatFrequency.None;

    public int RepeatInterval { get; set; } = 1;

    public bool RepeatUntilEnabled { get; set; }

    public DateOnly RepeatUntilDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;

    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public TimeOnly StartTime { get; set; } = new(8, 0);

    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public TimeOnly EndTime { get; set; } = new(9, 0);

    public DateOnly DeadlineDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public TimeOnly DeadlineTime { get; set; } = new(18, 0);

    public int ExpectedDurationMinutes { get; set; } = 30;

    public bool TargetDateEnabled { get; set; }

    public DateOnly TargetDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public static AddItemFormModel CreateDefault(string? timeZoneId = null, TimeOnly? planStartTime = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var startTime = planStartTime ?? new TimeOnly(8, 0);
        var endTime = startTime.AddHours(1);

        return new AddItemFormModel
        {
            Type = ItemType.Task,
            Color = "#4F46E5",
            ReminderMinutesBeforeStart = 15,
            RepeatFrequency = RepeatFrequency.None,
            RepeatInterval = 1,
            TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? TimeZoneInfo.Local.Id : timeZoneId.Trim(),
            StartDate = today,
            StartTime = startTime,
            EndDate = today,
            EndTime = endTime,
            DeadlineDate = today,
            DeadlineTime = new TimeOnly(18, 0),
            ExpectedDurationMinutes = 30,
            TargetDate = today
        };
    }

    public static AddItemFormModel FromItem(Item item)
    {
        var model = CreateDefault(item.TimeZoneId);
        model.Type = item.Type;
        model.Title = item.Title;
        model.Description = item.Description ?? string.Empty;
        model.Location = item.Location ?? string.Empty;
        model.Color = string.IsNullOrWhiteSpace(item.Color) ? "#4F46E5" : item.Color!;
        model.IsImportant = item.IsImportant;
        model.IsCompleted = item.IsCompleted;
        model.ReminderEnabled = item.ReminderConfig.IsEnabled;
        model.ReminderMinutesBeforeStart = item.ReminderConfig.Triggers.FirstOrDefault()?.MinutesBeforeStart ?? 15;
        model.RepeatFrequency = item.RepeatRule.Frequency;
        model.RepeatInterval = Math.Max(1, item.RepeatRule.Interval);

        if (item.RepeatRule.UntilAt is DateTimeOffset repeatUntilAt)
        {
            model.RepeatUntilEnabled = true;
            model.RepeatUntilDate = DateOnly.FromDateTime(repeatUntilAt.LocalDateTime.Date);
        }

        if (item.Type == ItemType.Schedule)
        {
            ApplyTimeRange(model, item.StartAt, item.EndAt);
        }
        else if (item.Type == ItemType.Task)
        {
            ApplyTimeRange(model, item.PlannedStartAt, item.PlannedEndAt);

            if (item.DeadlineAt is DateTimeOffset deadlineAt)
            {
                model.DeadlineDate = DateOnly.FromDateTime(deadlineAt.LocalDateTime.Date);
                model.DeadlineTime = TimeOnly.FromDateTime(deadlineAt.LocalDateTime);
            }
        }
        else if (item.Type == ItemType.Note)
        {
            model.ExpectedDurationMinutes = item.ExpectedDurationMinutes ?? 30;
            if (item.TargetDate is DateOnly targetDate)
            {
                model.TargetDateEnabled = true;
                model.TargetDate = targetDate;
            }
        }

        return model;
    }

    public ItemUpsertRequest ToRequest()
    {
        var repeatRule = new RepeatRule
        {
            Frequency = RepeatFrequency,
            Interval = Math.Max(1, RepeatInterval),
            UntilAt = RepeatUntilEnabled
                ? new DateTimeOffset(RepeatUntilDate.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero)
                : null
        };

        var reminderConfig = ReminderEnabled
            ? new ReminderConfig
            {
                IsEnabled = true,
                Triggers = new[]
                {
                    new ReminderTrigger
                    {
                        Channel = ReminderChannel.Notification,
                        MinutesBeforeStart = Math.Max(0, ReminderMinutesBeforeStart)
                    }
                }
            }
            : new ReminderConfig();

        var request = new ItemUpsertRequest
        {
            Type = Type,
            Title = Title,
            Description = Description,
            Location = Location,
            Color = Color,
            IsImportant = IsImportant,
            IsCompleted = IsCompleted,
            ReminderConfig = reminderConfig,
            RepeatRule = repeatRule,
            TimeZoneId = TimeZoneId
        };

        return Type switch
        {
            ItemType.Schedule => request with
            {
                StartAt = CombineLocal(StartDate, StartTime),
                EndAt = CombineLocal(EndDate, EndTime)
            },
            ItemType.Task => request with
            {
                PlannedStartAt = CombineLocal(StartDate, StartTime),
                PlannedEndAt = CombineLocal(EndDate, EndTime),
                DeadlineAt = CombineLocal(DeadlineDate, DeadlineTime)
            },
            ItemType.Note => request with
            {
                ExpectedDurationMinutes = ExpectedDurationMinutes,
                TargetDate = TargetDateEnabled ? TargetDate : null
            },
            _ => request
        };
    }

    private static void ApplyTimeRange(AddItemFormModel model, DateTimeOffset? startAt, DateTimeOffset? endAt)
    {
        if (startAt is DateTimeOffset start)
        {
            model.StartDate = DateOnly.FromDateTime(start.LocalDateTime.Date);
            model.StartTime = TimeOnly.FromDateTime(start.LocalDateTime);
        }

        if (endAt is DateTimeOffset end)
        {
            model.EndDate = DateOnly.FromDateTime(end.LocalDateTime.Date);
            model.EndTime = TimeOnly.FromDateTime(end.LocalDateTime);
        }
    }

    private static DateTimeOffset CombineLocal(DateOnly date, TimeOnly time)
    {
        var localDateTime = date.ToDateTime(time);
        return new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime));
    }
}
