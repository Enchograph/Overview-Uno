using Overview.Client.Application.Ai;
using Overview.Client.Application.Auth;
using Overview.Client.Application.Home;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;

namespace Overview.Client.Presentation.ViewModels;

public sealed class AiPageViewModel
{
    private readonly IAuthenticationService authenticationService;
    private readonly IAiChatService aiChatService;
    private readonly ITimeSelectionService timeSelectionService;

    public AiPageViewModel(
        IAuthenticationService authenticationService,
        IAiChatService aiChatService,
        ITimeSelectionService timeSelectionService)
    {
        this.authenticationService = authenticationService;
        this.aiChatService = aiChatService;
        this.timeSelectionService = timeSelectionService;
    }

    public string Title => "AI";

    public string Description =>
        "Direct AI chat for Overview. Messages are stored by day and can be reviewed by day, week, or month.";

    public bool IsBusy { get; private set; }

    public bool IsAuthenticated => authenticationService.CurrentSession is not null;

    public string CurrentPeriodTitle { get; private set; } = "Today";

    public string VisibleRangeSummary { get; private set; } = string.Empty;

    public string StatusMessage { get; private set; } = "Loading AI chat.";

    public string DraftMessage { get; private set; } = string.Empty;

    public IReadOnlyList<AiChatMessageEntryViewModel> Messages { get; private set; } =
        Array.Empty<AiChatMessageEntryViewModel>();

    public bool CanSend => IsAuthenticated && !IsBusy && !string.IsNullOrWhiteSpace(DraftMessage);

    public bool IsPickerOpen { get; private set; }

    public TimeSelectionMode CurrentSelectionMode { get; private set; } = TimeSelectionMode.Day;

    public DateOnly CurrentReferenceDate { get; private set; } = DateOnly.FromDateTime(DateTime.Today);

    public CalendarPeriod? CurrentPeriod { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            Messages = Array.Empty<AiChatMessageEntryViewModel>();
            CurrentPeriodTitle = "Today";
            VisibleRangeSummary = string.Empty;
            StatusMessage = "AI chat requires an authenticated account.";
            return;
        }

        await LoadSelectedPeriodCoreAsync(userId, CurrentReferenceDate, CurrentSelectionMode, cancellationToken)
            .ConfigureAwait(false);
    }

    public void UpdateDraft(string? draftMessage)
    {
        DraftMessage = draftMessage ?? string.Empty;
    }

    public void TogglePicker()
    {
        IsPickerOpen = !IsPickerOpen;
    }

    public async Task SetSelectionModeAsync(
        TimeSelectionMode selectionMode,
        CancellationToken cancellationToken = default)
    {
        CurrentSelectionMode = selectionMode;

        if (!TryGetUserId(out var userId))
        {
            Messages = Array.Empty<AiChatMessageEntryViewModel>();
            CurrentPeriodTitle = "Today";
            VisibleRangeSummary = string.Empty;
            StatusMessage = "AI chat requires an authenticated account.";
            return;
        }

        await LoadSelectedPeriodCoreAsync(userId, CurrentReferenceDate, CurrentSelectionMode, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task ApplyConfirmedPeriodAsync(
        CalendarPeriod period,
        CancellationToken cancellationToken = default)
    {
        CurrentSelectionMode = period.Mode;
        CurrentReferenceDate = period.ReferenceDate;
        IsPickerOpen = false;

        if (!TryGetUserId(out var userId))
        {
            Messages = Array.Empty<AiChatMessageEntryViewModel>();
            CurrentPeriodTitle = "Today";
            VisibleRangeSummary = string.Empty;
            StatusMessage = "AI chat requires an authenticated account.";
            return;
        }

        await LoadPeriodAsync(userId, period, cancellationToken).ConfigureAwait(false);
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
            await aiChatService.SendMessageAsync(userId, DraftMessage, cancellationToken).ConfigureAwait(false);
            DraftMessage = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsBusy = false;
            return;
        }

        IsBusy = false;
        await LoadSelectedPeriodCoreAsync(userId, CurrentReferenceDate, CurrentSelectionMode, cancellationToken)
            .ConfigureAwait(false);
        StatusMessage = "AI response stored and current period refreshed.";
    }

    private async Task LoadSelectedPeriodCoreAsync(
        Guid userId,
        DateOnly referenceDate,
        TimeSelectionMode selectionMode,
        CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var period = await timeSelectionService.ResolveSelectionAsync(
                userId,
                referenceDate,
                selectionMode,
                cancellationToken).ConfigureAwait(false);
            await LoadPeriodInternalAsync(userId, period, cancellationToken).ConfigureAwait(false);
            StatusMessage = Messages.Count == 0
                ? $"No AI messages stored for the selected {period.Mode.ToString().ToLowerInvariant()} yet."
                : $"Loaded {Messages.Count} AI messages for the selected {period.Mode.ToString().ToLowerInvariant()}.";
        }
        catch (Exception ex)
        {
            Messages = Array.Empty<AiChatMessageEntryViewModel>();
            CurrentPeriodTitle = "Today";
            VisibleRangeSummary = string.Empty;
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadPeriodAsync(
        Guid userId,
        CalendarPeriod period,
        CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await LoadPeriodInternalAsync(userId, period, cancellationToken).ConfigureAwait(false);
            StatusMessage = Messages.Count == 0
                ? $"No AI messages stored for the selected {period.Mode.ToString().ToLowerInvariant()} yet."
                : $"Loaded {Messages.Count} AI messages for the selected {period.Mode.ToString().ToLowerInvariant()}.";
        }
        catch (Exception ex)
        {
            Messages = Array.Empty<AiChatMessageEntryViewModel>();
            CurrentPeriodTitle = "Today";
            VisibleRangeSummary = string.Empty;
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadPeriodInternalAsync(
        Guid userId,
        CalendarPeriod period,
        CancellationToken cancellationToken)
    {
        var snapshot = await aiChatService.GetSnapshotAsync(userId, period, cancellationToken).ConfigureAwait(false);
        ApplySnapshot(snapshot);
    }

    private void ApplySnapshot(AiChatPeriodSnapshot snapshot)
    {
        CurrentPeriod = snapshot.Period;
        CurrentReferenceDate = snapshot.Period.ReferenceDate;
        CurrentSelectionMode = snapshot.Period.Mode;
        CurrentPeriodTitle = FormatPeriodTitle(snapshot.Period);
        VisibleRangeSummary = $"{snapshot.Period.StartDate:yyyy-MM-dd} to {snapshot.Period.EndDate:yyyy-MM-dd}";
        Messages = snapshot.Messages
            .Select(AiChatMessageEntryViewModel.FromMessage)
            .ToArray();
    }

    private static string FormatPeriodTitle(CalendarPeriod period)
    {
        return period.Mode switch
        {
            TimeSelectionMode.Day => period.ReferenceDate.ToString("yyyy-MM-dd"),
            TimeSelectionMode.Week => $"{period.StartDate:yyyy-MM-dd} to {period.EndDate:yyyy-MM-dd}",
            TimeSelectionMode.Month => period.ReferenceDate.ToString("yyyy-MM"),
            _ => period.ReferenceDate.ToString("yyyy-MM-dd")
        };
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
