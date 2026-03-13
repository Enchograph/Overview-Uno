using Microsoft.UI.Xaml.Input;
using Overview.Client.Presentation.ViewModels;
using Overview.Client.Domain.Enums;

namespace Overview.Client.Presentation.Pages;

public sealed partial class ListPage : Page
{
    private bool isApplyingViewModel;

    private ListPageViewModel ViewModel => (ListPageViewModel)DataContext;

    public ListPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<ListPageViewModel>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync().ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }

    private async void OnTabButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string tabKey } ||
            !Enum.TryParse<ListPageTab>(tabKey, out var tab))
        {
            return;
        }

        await ViewModel.SelectTabAsync(tab).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnSortBySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (isApplyingViewModel ||
            sender is not ComboBox { SelectedItem: ListPageSortOptionViewModel option })
        {
            return;
        }

        await ViewModel.SelectSortAsync(option.SortBy).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnCompletionButtonClick(object sender, RoutedEventArgs e)
    {
        if (!TryGetItemId(sender, out var itemId))
        {
            return;
        }

        await ViewModel.ToggleCompletionAsync(itemId).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnImportanceButtonClick(object sender, RoutedEventArgs e)
    {
        if (!TryGetItemId(sender, out var itemId))
        {
            return;
        }

        await ViewModel.ToggleImportanceAsync(itemId).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnItemContainerTapped(object sender, TappedRoutedEventArgs e)
    {
        if (!TryGetItemId(sender, out var itemId))
        {
            return;
        }

        await ViewModel.ToggleCompletionAsync(itemId).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void ApplyViewModelState()
    {
        isApplyingViewModel = true;

        PageTitleTextBlock.Text = ViewModel.PageTitle;
        PageSubtitleTextBlock.Text = ViewModel.PageSubtitle;
        ActiveSummaryTextBlock.Text = ViewModel.ActiveSummary;
        CompletedSummaryTextBlock.Text = ViewModel.CompletedSummary;
        StatusTextBlock.Text = ViewModel.StatusMessage;
        BusyIndicator.IsActive = ViewModel.IsBusy;

        TabItemsControl.ItemsSource = ViewModel.Tabs;
        TabItemsControl.UpdateLayout();
        SortByComboBox.ItemsSource = ViewModel.SortOptions;
        SortByComboBox.SelectedItem = ViewModel.SortOptions.FirstOrDefault(option => option.IsSelected);
        ActiveItemsListView.ItemsSource = ViewModel.ActiveItems;
        CompletedItemsListView.ItemsSource = ViewModel.CompletedItems;

        EmptyStateTitleTextBlock.Text = ViewModel.EmptyStateTitle;
        EmptyStateDescriptionTextBlock.Text = ViewModel.EmptyStateDescription;

        var hasItems = ViewModel.ActiveItems.Count > 0 || ViewModel.CompletedItems.Count > 0;
        ContentPanel.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
        EmptyStateBorder.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;

        isApplyingViewModel = false;
    }

    private static bool TryGetItemId(object sender, out Guid itemId)
    {
        itemId = default;

        if (sender is FrameworkElement { Tag: Guid typedId })
        {
            itemId = typedId;
            return true;
        }

        if (sender is FrameworkElement { Tag: string itemIdText } &&
            Guid.TryParse(itemIdText, out itemId))
        {
            return true;
        }

        return false;
    }
}
