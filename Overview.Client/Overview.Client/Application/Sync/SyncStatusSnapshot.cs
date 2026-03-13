namespace Overview.Client.Application.Sync;

public sealed class SyncStatusSnapshot
{
    public SyncLifecycleState State { get; init; } = SyncLifecycleState.Idle;

    public SyncExecutionTrigger? LastTrigger { get; init; }

    public DateTimeOffset? LastAttemptedAt { get; init; }

    public DateTimeOffset? LastSuccessfulAt { get; init; }

    public DateTimeOffset? LastKnownServerTime { get; init; }

    public int PendingChangeCount { get; init; }

    public int AppliedChangeCount { get; init; }

    public int PulledItemCount { get; init; }

    public bool SettingsApplied { get; init; }

    public int ConflictCount { get; init; }

    public int ConsecutiveFailureCount { get; init; }

    public bool IsAutoSyncEnabled { get; init; }

    public bool IsAuthenticated { get; init; }

    public string? LastError { get; init; }
}
