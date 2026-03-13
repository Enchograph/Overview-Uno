namespace Overview.Client.Application.Ai;

public sealed record AiReminderInstruction
{
    public bool IsEnabled { get; init; }

    public IReadOnlyList<int> MinutesBeforeStart { get; init; } = Array.Empty<int>();
}
