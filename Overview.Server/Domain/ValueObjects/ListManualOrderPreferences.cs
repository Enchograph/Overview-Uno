namespace Overview.Server.Domain.ValueObjects;

public sealed record ListManualOrderPreferences
{
    public IReadOnlyList<Guid> MyDay { get; init; } = Array.Empty<Guid>();

    public IReadOnlyList<Guid> AllItems { get; init; } = Array.Empty<Guid>();

    public IReadOnlyList<Guid> Tasks { get; init; } = Array.Empty<Guid>();

    public IReadOnlyList<Guid> Schedules { get; init; } = Array.Empty<Guid>();

    public IReadOnlyList<Guid> Notes { get; init; } = Array.Empty<Guid>();

    public IReadOnlyList<Guid> Important { get; init; } = Array.Empty<Guid>();
}
