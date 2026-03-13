namespace Overview.Client.Application.Sync;

public sealed class SyncCheckpoint
{
    public Guid UserId { get; init; }

    public DateTimeOffset? LastKnownServerTime { get; init; }

    public DateTimeOffset? LastAttemptedAt { get; init; }

    public DateTimeOffset? LastSuccessfulAt { get; init; }

    public SyncExecutionTrigger? LastTrigger { get; init; }

    public string? LastError { get; init; }

    public int ConsecutiveFailureCount { get; init; }
}
