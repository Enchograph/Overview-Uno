using System;
using Overview.Client.Presentation.ViewModels;
using Overview.Client.Domain.Enums;
using Microsoft.UI.Xaml.Navigation;

namespace Overview.Client.Presentation.Pages;

public sealed partial class AddItemPage : Page
{
    private AddItemNavigationRequest? navigationRequest;

    private AddItemPageViewModel ViewModel => (AddItemPageViewModel)DataContext;

    public AddItemPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<AddItemPageViewModel>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync(navigationRequest).ConfigureAwait(true);
        ApplyViewModelState();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        navigationRequest = e.Parameter as AddItemNavigationRequest;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }

    private async void OnSubmitButtonClick(object sender, RoutedEventArgs e)
    {
        SyncViewModelFromInputs();
        ApplyViewModelState();
        await ViewModel.SaveAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnNewItemButtonClick(object sender, RoutedEventArgs e)
    {
        SyncViewModelFromInputs();
        ApplyViewModelState();
        await ViewModel.StartCreateAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnExistingItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is AddItemListEntry entry)
        {
            await ViewModel.LoadDetailAsync(entry.Id).ConfigureAwait(true);
            ApplyViewModelState();
        }
    }

    private async void OnViewItemButtonClick(object sender, RoutedEventArgs e)
    {
        if (TryGetItemIdFromSender(sender, out var itemId))
        {
            await ViewModel.LoadDetailAsync(itemId).ConfigureAwait(true);
            ApplyViewModelState();
        }
    }

    private async void OnEditItemButtonClick(object sender, RoutedEventArgs e)
    {
        if (TryGetItemIdFromSender(sender, out var itemId))
        {
            await ViewModel.LoadForEditAsync(itemId).ConfigureAwait(true);
            ApplyViewModelState();
        }
    }

    private async void OnDetailEditRequested(object sender, Guid itemId)
    {
        await ViewModel.LoadForEditAsync(itemId).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnItemTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        SyncViewModelFromInputs();
        ApplyViewModelState();
    }

    private void OnReminderEnabledChanged(object sender, RoutedEventArgs e)
    {
        SyncViewModelFromInputs();
        ApplyViewModelState();
    }

    private void OnRepeatUntilEnabledChanged(object sender, RoutedEventArgs e)
    {
        SyncViewModelFromInputs();
        ApplyViewModelState();
    }

    private void OnTargetDateEnabledChanged(object sender, RoutedEventArgs e)
    {
        SyncViewModelFromInputs();
        ApplyViewModelState();
    }

    private void SyncViewModelFromInputs()
    {
        var form = ViewModel.Form;
        form.Type = ParseItemType(ItemTypeComboBox.SelectedItem);
        form.Title = TitleTextBox.Text;
        form.Description = DescriptionTextBox.Text;
        form.Location = LocationTextBox.Text;
        form.Color = ColorTextBox.Text;
        form.TimeZoneId = TimeZoneTextBox.Text;
        form.IsImportant = ImportantCheckBox.IsChecked == true;
        form.IsCompleted = CompletedCheckBox.IsChecked == true;
        form.ReminderEnabled = ReminderEnabledCheckBox.IsChecked == true;
        form.ReminderMinutesBeforeStart = (int)Math.Round(ReminderMinutesNumberBox.Value);
        form.RepeatFrequency = ParseRepeatFrequency(RepeatFrequencyComboBox.SelectedItem);
        form.RepeatInterval = Math.Max(1, (int)Math.Round(RepeatIntervalNumberBox.Value));
        form.RepeatUntilEnabled = RepeatUntilEnabledCheckBox.IsChecked == true;
        form.RepeatUntilDate = DateOnly.FromDateTime(RepeatUntilDatePicker.Date.DateTime);
        form.StartDate = DateOnly.FromDateTime(StartDatePicker.Date.DateTime);
        form.StartTime = TimeOnly.FromTimeSpan(StartTimePicker.Time);
        form.EndDate = DateOnly.FromDateTime(EndDatePicker.Date.DateTime);
        form.EndTime = TimeOnly.FromTimeSpan(EndTimePicker.Time);
        form.DeadlineDate = DateOnly.FromDateTime(DeadlineDatePicker.Date.DateTime);
        form.DeadlineTime = TimeOnly.FromTimeSpan(DeadlineTimePicker.Time);
        form.ExpectedDurationMinutes = Math.Max(1, (int)Math.Round(ExpectedDurationNumberBox.Value));
        form.TargetDateEnabled = TargetDateEnabledCheckBox.IsChecked == true;
        form.TargetDate = DateOnly.FromDateTime(TargetDatePicker.Date.DateTime);
    }

    private void ApplyViewModelState()
    {
        var form = ViewModel.Form;

        PageTitleTextBlock.Text = ViewModel.PageTitle;
        SelectComboBoxItem(ItemTypeComboBox, form.Type.ToString());
        TitleTextBox.Text = form.Title;
        DescriptionTextBox.Text = form.Description;
        LocationTextBox.Text = form.Location;
        ColorTextBox.Text = form.Color;
        TimeZoneTextBox.Text = form.TimeZoneId;
        ImportantCheckBox.IsChecked = form.IsImportant;
        CompletedCheckBox.IsChecked = form.IsCompleted;
        ReminderEnabledCheckBox.IsChecked = form.ReminderEnabled;
        ReminderMinutesNumberBox.Value = form.ReminderMinutesBeforeStart;
        SelectComboBoxItem(RepeatFrequencyComboBox, form.RepeatFrequency.ToString());
        RepeatIntervalNumberBox.Value = form.RepeatInterval;
        RepeatUntilEnabledCheckBox.IsChecked = form.RepeatUntilEnabled;
        RepeatUntilDatePicker.Date = new DateTimeOffset(form.RepeatUntilDate.ToDateTime(TimeOnly.MinValue));
        StartDatePicker.Date = new DateTimeOffset(form.StartDate.ToDateTime(TimeOnly.MinValue));
        StartTimePicker.Time = form.StartTime.ToTimeSpan();
        EndDatePicker.Date = new DateTimeOffset(form.EndDate.ToDateTime(TimeOnly.MinValue));
        EndTimePicker.Time = form.EndTime.ToTimeSpan();
        DeadlineDatePicker.Date = new DateTimeOffset(form.DeadlineDate.ToDateTime(TimeOnly.MinValue));
        DeadlineTimePicker.Time = form.DeadlineTime.ToTimeSpan();
        ExpectedDurationNumberBox.Value = form.ExpectedDurationMinutes;
        TargetDateEnabledCheckBox.IsChecked = form.TargetDateEnabled;
        TargetDatePicker.Date = new DateTimeOffset(form.TargetDate.ToDateTime(TimeOnly.MinValue));

        var isTask = form.Type == ItemType.Task;
        var isNote = form.Type == ItemType.Note;
        TimedSectionTitleTextBlock.Text = isTask ? "Planned Time Range" : "Time Range";
        TimedFieldsBorder.Visibility = isNote ? Visibility.Collapsed : Visibility.Visible;
        TaskFieldsBorder.Visibility = isTask ? Visibility.Visible : Visibility.Collapsed;
        NoteFieldsBorder.Visibility = isNote ? Visibility.Visible : Visibility.Collapsed;
        ReminderMinutesNumberBox.IsEnabled = form.ReminderEnabled;
        RepeatUntilDatePicker.IsEnabled = form.RepeatUntilEnabled;
        TargetDatePicker.IsEnabled = form.TargetDateEnabled;

        ExistingItemsListView.ItemsSource = ViewModel.ExistingItems;
        ExistingItemsListView.Visibility = ViewModel.IsAuthenticated ? Visibility.Visible : Visibility.Collapsed;
        ItemDetailCard.DataContext = ViewModel.Detail;
        SubmitButton.Content = ViewModel.SubmitButtonText;
        StatusTextBlock.Text = ViewModel.StatusMessage;
        BusyIndicator.IsActive = ViewModel.IsBusy;
        FormPanel.IsHitTestVisible = ViewModel.IsAuthenticated && !ViewModel.IsBusy;
        FormPanel.Opacity = ViewModel.IsAuthenticated ? (ViewModel.IsBusy ? 0.72 : 1) : 0.6;
    }

    private static ItemType ParseItemType(object? selectedItem)
    {
        var tag = (selectedItem as ComboBoxItem)?.Tag as string;
        return Enum.TryParse<ItemType>(tag, out var itemType) ? itemType : ItemType.Task;
    }

    private static RepeatFrequency ParseRepeatFrequency(object? selectedItem)
    {
        var tag = (selectedItem as ComboBoxItem)?.Tag as string;
        return Enum.TryParse<RepeatFrequency>(tag, out var frequency) ? frequency : RepeatFrequency.None;
    }

    private static void SelectComboBoxItem(ComboBox comboBox, string targetTag)
    {
        foreach (var item in comboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem && string.Equals(comboBoxItem.Tag as string, targetTag, StringComparison.Ordinal))
            {
                comboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        comboBox.SelectedIndex = 0;
    }

    private static bool TryGetItemIdFromSender(object sender, out Guid itemId)
    {
        if (sender is FrameworkElement { Tag: Guid id })
        {
            itemId = id;
            return true;
        }

        itemId = Guid.Empty;
        return false;
    }
}
