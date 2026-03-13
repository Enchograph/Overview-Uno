using Overview.Client.Application.Ai;
using Overview.Client.Application.Auth;

namespace Overview.Client.Presentation.ViewModels;

public sealed class AiPageViewModel
{
    private readonly IAuthenticationService authenticationService;
    private readonly IAiChatService aiChatService;

    public AiPageViewModel(
        IAuthenticationService authenticationService,
        IAiChatService aiChatService)
    {
        this.authenticationService = authenticationService;
        this.aiChatService = aiChatService;
    }

    public string Title => "AI";

    public string Description =>
        "Direct AI chat for Overview. Each request is sent independently and stored locally by day.";

    public bool IsBusy { get; private set; }

    public bool IsAuthenticated => authenticationService.CurrentSession is not null;

    public string CurrentDayTitle { get; private set; } = "Today";

    public string StatusMessage { get; private set; } = "Loading AI chat.";

    public string DraftMessage { get; private set; } = string.Empty;

    public IReadOnlyList<AiChatMessageEntryViewModel> Messages { get; private set; } =
        Array.Empty<AiChatMessageEntryViewModel>();

    public bool CanSend => IsAuthenticated && !IsBusy && !string.IsNullOrWhiteSpace(DraftMessage);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            Messages = Array.Empty<AiChatMessageEntryViewModel>();
            CurrentDayTitle = "Today";
            StatusMessage = "AI chat requires an authenticated account.";
            return;
        }

        await LoadTodayAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public void UpdateDraft(string? draftMessage)
    {
        DraftMessage = draftMessage ?? string.Empty;
    }

    public async Task SendAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || !TryGetUserId(out var userId))
        {
            StatusMessage = "AI chat requires an authenticated account.";
            return;
        }

        if (string.IsNullOrWhiteSpace(DraftMessage))
        {
            StatusMessage = "Enter a message before sending.";
            return;
        }

        try
        {
            IsBusy = true;
            var snapshot = await aiChatService.SendMessageAsync(userId, DraftMessage, cancellationToken)
                .ConfigureAwait(false);
            DraftMessage = string.Empty;
            ApplySnapshot(snapshot);
            StatusMessage = "AI response stored for today.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadTodayAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var snapshot = await aiChatService.GetTodaySnapshotAsync(userId, cancellationToken).ConfigureAwait(false);
            ApplySnapshot(snapshot);
            StatusMessage = snapshot.Messages.Count == 0
                ? "No AI messages stored for today yet."
                : $"Loaded {snapshot.Messages.Count} AI messages for today.";
        }
        catch (Exception ex)
        {
            Messages = Array.Empty<AiChatMessageEntryViewModel>();
            CurrentDayTitle = "Today";
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplySnapshot(AiChatDaySnapshot snapshot)
    {
        CurrentDayTitle = snapshot.OccurredOn.ToString("yyyy-MM-dd");
        Messages = snapshot.Messages
            .Select(AiChatMessageEntryViewModel.FromMessage)
            .ToArray();
    }

    private bool TryGetUserId(out Guid userId)
    {
        var session = authenticationService.CurrentSession;
        if (session is null)
        {
            userId = default;
            return false;
        }

        userId = session.UserId;
        return true;
    }
}
