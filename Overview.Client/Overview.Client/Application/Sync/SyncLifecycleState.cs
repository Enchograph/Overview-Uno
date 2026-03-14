namespace Overview.Client.Application.Sync;

public enum SyncLifecycleState
{
    Idle = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3,
    RequiresAuthentication = 4,
    Offline = 5
}
