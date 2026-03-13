using Overview.Client.Domain.Enums;

namespace Overview.Client.Application.Ai;

public sealed record AiStructuredResponse
{
    public AiRequestType Intent { get; init; } = AiRequestType.AnswerQuestion;

    public ItemType? ItemType { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? StartAt { get; init; }

    public DateTimeOffset? EndAt { get; init; }

    public DateTimeOffset? DeadlineAt { get; init; }

    public string? Location { get; init; }

    public bool? IsImportant { get; init; }

    public AiReminderInstruction? Reminder { get; init; }

    public AiRepeatRuleInstruction? RepeatRule { get; init; }

    public double Confidence { get; init; }

    public string? Answer { get; init; }
}
