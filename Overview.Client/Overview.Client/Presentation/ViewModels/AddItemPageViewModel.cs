using Overview.Client.Application.Auth;
using Overview.Client.Application.Items;
using Overview.Client.Application.Settings;
using Overview.Client.Domain.Entities;
using Overview.Client.Domain.Enums;
using Overview.Client.Domain.ValueObjects;
using Overview.Client.Presentation.Pages;

namespace Overview.Client.Presentation.ViewModels;

public sealed class AddItemPageViewModel
{
    private readonly IAuthenticationService authenticationService;
    private readonly IItemService itemService;
    private readonly IUserSettingsService userSettingsService;

    public AddItemPageViewModel(
        IAuthenticationService authenticationService,
        IItemService itemService,
        IUserSettingsService userSettingsService)
    {
        this.authenticationService = authenticationService;
        this.itemService = itemService;
        this.userSettingsService = userSettingsService;
    }

    public AddItemFormModel Form { get; private set; } = AddItemFormModel.CreateDefault();

    public IReadOnlyList<AddItemListEntry> ExistingItems { get; private set; } = Array.Empty<AddItemListEntry>();

    public ItemDetailViewModel Detail { get; private set; } = ItemDetailViewModel.Empty;

    public bool IsBusy { get; private set; }

    public string StatusMessage { get; private set; } = string.Empty;

    public string PageTitle => IsEditMode ? "Edit Item" : "Add Item";

    public string SubmitButtonText => IsEditMode ? "Save Changes" : "Create Item";

    public bool IsAuthenticated => authenticationService.CurrentSession is not null;

    public bool IsEditMode => EditingItemId is not null;

    public Guid? EditingItemId { get; private set; }

    public async Task InitializeAsync(
        AddItemNavigationRequest? navigationRequest = null,
        CancellationToken cancellationToken = default)
    {
        await ExecuteBusyActionAsync(
            async () =>
            {
                if (!TryGetUserId(out var userId))
                {
                    ResetForLoggedOutState();
                    return false;
                }

                var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
                Form = AddItemFormModel.CreateDefault(settings.TimeZoneId, settings.DayPlanStartTime);
                EditingItemId = null;
                Detail = ItemDetailViewModel.Empty;
                await LoadExistingItemsAsync(userId, cancellationToken).ConfigureAwait(false);
                ApplyNavigationPreset(navigationRequest);
                StatusMessage = ExistingItems.Count == 0
                    ? "No items yet. Fill the form to create your first item."
                    : "Select an existing item to edit it, or create a new one.";

                if (navigationRequest?.EditItemId is Guid editItemId)
                {
                    var item = await itemService.GetAsync(userId, editItemId, cancellationToken: cancellationToken).ConfigureAwait(false);
                    if (item is null)
                    {
                        StatusMessage = "The selected item was not found.";
                        return false;
                    }

                    Form = AddItemFormModel.FromItem(item);
                    EditingItemId = item.Id;
                    Detail = ItemDetailViewModel.FromItem(item);
                    StatusMessage = "Edit mode ready.";
                }
                else if (navigationRequest?.SuggestedStartDate is not null || navigationRequest?.SuggestedStartTime is not null)
                {
                    StatusMessage = "Create mode ready with the selected start time.";
                }

                return true;
            }).ConfigureAwait(false);
    }

    public async Task StartCreateAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteBusyActionAsync(
            async () =>
            {
                if (!TryGetUserId(out var userId))
                {
                    ResetForLoggedOutState();
                    return false;
                }

                var settings = await userSettingsService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
                Form = AddItemFormModel.CreateDefault(settings.TimeZoneId, settings.DayPlanStartTime);
                EditingItemId = null;
                Detail = ItemDetailViewModel.Empty;
                StatusMessage = "Create mode ready.";
                return true;
            }).ConfigureAwait(false);
    }

    public async Task LoadForEditAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        await ExecuteBusyActionAsync(
            async () =>
            {
                if (!TryGetUserId(out var userId))
                {
                    ResetForLoggedOutState();
                    return false;
                }

                var item = await itemService.GetAsync(userId, itemId, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (item is null)
                {
                    StatusMessage = "The selected item was not found.";
                    return false;
                }

                Form = AddItemFormModel.FromItem(item);
                EditingItemId = item.Id;
                Detail = ItemDetailViewModel.FromItem(item);
                StatusMessage = "Edit mode ready.";
                return true;
            }).ConfigureAwait(false);
    }

    public async Task LoadDetailAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        await ExecuteBusyActionAsync(
            async () =>
            {
                if (!TryGetUserId(out var userId))
                {
                    ResetForLoggedOutState();
                    return false;
                }

                var item = await itemService.GetAsync(userId, itemId, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (item is null)
                {
                    Detail = ItemDetailViewModel.Empty;
                    StatusMessage = "The selected item was not found.";
                    return false;
                }

                Detail = ItemDetailViewModel.FromItem(item);
                StatusMessage = "Detail view updated.";
                return true;
            }).ConfigureAwait(false);
    }

    public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteBusyActionAsync(
            async () =>
            {
                if (!TryGetUserId(out var userId))
                {
                    ResetForLoggedOutState();
                    return false;
                }

                var request = Form.ToRequest();
                if (EditingItemId is Guid itemId)
                {
                    await itemService.UpdateAsync(userId, itemId, request, cancellationToken).ConfigureAwait(false);
                    StatusMessage = "Item updated.";
                }
                else
                {
                    var createdItem = await itemService.CreateAsync(userId, request, cancellationToken).ConfigureAwait(false);
                    EditingItemId = createdItem.Id;
                    StatusMessage = "Item created.";
                }

                await LoadExistingItemsAsync(userId, cancellationToken).ConfigureAwait(false);

                if (EditingItemId is Guid refreshedItemId)
                {
                    var refreshedItem = await itemService.GetAsync(userId, refreshedItemId, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    if (refreshedItem is not null)
                    {
                        Form = AddItemFormModel.FromItem(refreshedItem);
                        Detail = ItemDetailViewModel.FromItem(refreshedItem);
                    }
                }

                return true;
            }).ConfigureAwait(false);
    }

    private async Task LoadExistingItemsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await itemService.ListAsync(userId, cancellationToken: cancellationToken).ConfigureAwait(false);
        ExistingItems = items
            .Where(item => item.DeletedAt is null)
            .OrderByDescending(item => item.LastModifiedAt)
            .Select(AddItemListEntry.FromItem)
            .ToArray();
    }

    private bool TryGetUserId(out Guid userId)
    {
        var session = authenticationService.CurrentSession;
        if (session is null)
        {
            userId = Guid.Empty;
            return false;
        }

        userId = session.UserId;
        return true;
    }

    private void ResetForLoggedOutState()
    {
        Form = AddItemFormModel.CreateDefault();
        EditingItemId = null;
        ExistingItems = Array.Empty<AddItemListEntry>();
        Detail = ItemDetailViewModel.Empty;
        StatusMessage = "Sign in first to create or edit items.";
    }

    private void ApplyNavigationPreset(AddItemNavigationRequest? navigationRequest)
    {
        if (navigationRequest?.EditItemId is not null)
        {
            return;
        }

        if (navigationRequest?.SuggestedStartDate is DateOnly startDate)
        {
            Form.StartDate = startDate;
            Form.EndDate = startDate;
        }

        if (navigationRequest?.SuggestedStartTime is TimeOnly startTime)
        {
            Form.StartTime = startTime;
            var adjustedEndTime = startTime.AddHours(1);
            if (adjustedEndTime < startTime)
            {
                Form.EndDate = Form.StartDate.AddDays(1);
            }

            Form.EndTime = adjustedEndTime;
        }
    }

    private async Task<bool> ExecuteBusyActionAsync(Func<Task<bool>> action)
    {
        if (IsBusy)
        {
            return false;
        }

        try
        {
            IsBusy = true;
            return await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
