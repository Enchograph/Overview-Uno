using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Overview.Client.Presentation.ViewModels;
using Overview.Client.Domain.Enums;
using Windows.UI;

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

    private async void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (isApplyingViewModel ||
            sender is not ComboBox { SelectedItem: ListPageThemeOptionViewModel option })
        {
            return;
        }

        await ViewModel.SelectThemeAsync(option.ThemeKey).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnReorderModeButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleReorderMode();
        ApplyViewModelState();
    }

    private void OnMoreSettingsButtonClick(object sender, RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(SettingsPage), SettingsPageViewModel.ListSectionKey);
    }

    private void OnAddItemButtonClick(object sender, RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(AddItemPage), ViewModel.CreateAddNavigationRequest());
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
        if (ViewModel.IsReorderMode)
        {
            return;
        }

        if (!TryGetItemId(sender, out var itemId))
        {
            return;
        }

        await ViewModel.ToggleCompletionAsync(itemId).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnMoveUpButtonClick(object sender, RoutedEventArgs e)
    {
        if (!TryGetItemId(sender, out var itemId))
        {
            return;
        }

        await ViewModel.MoveItemUpAsync(itemId).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private async void OnMoveDownButtonClick(object sender, RoutedEventArgs e)
    {
        if (!TryGetItemId(sender, out var itemId))
        {
            return;
        }

        await ViewModel.MoveItemDownAsync(itemId).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnEditSwipeItemInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if (!TryGetSwipeItemId(sender, out var itemId))
        {
            return;
        }

        Frame?.Navigate(typeof(AddItemPage), new AddItemNavigationRequest
        {
            EditItemId = itemId,
            SourceTabKey = ViewModel.CurrentTab.ToString()
        });
    }

    private async void OnDeleteSwipeItemInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if (!TryGetSwipeItemId(sender, out var itemId))
        {
            return;
        }

        await ViewModel.DeleteItemAsync(itemId).ConfigureAwait(true);
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
        ReorderModeButton.Content = ViewModel.ReorderButtonLabel;
        BusyIndicator.IsActive = ViewModel.IsBusy;

        TabItemsControl.ItemsSource = ViewModel.Tabs;
        TabItemsControl.UpdateLayout();
        SortByComboBox.ItemsSource = ViewModel.SortOptions;
        SortByComboBox.SelectedItem = ViewModel.SortOptions.FirstOrDefault(option => option.IsSelected);
        ThemeComboBox.ItemsSource = ViewModel.ThemeOptions;
        ThemeComboBox.SelectedItem = ViewModel.ThemeOptions.FirstOrDefault(option => option.IsSelected);
        ActiveItemsListView.ItemsSource = ViewModel.ActiveItems;
        CompletedItemsListView.ItemsSource = ViewModel.CompletedItems;

        EmptyStateTitleTextBlock.Text = ViewModel.EmptyStateTitle;
        EmptyStateDescriptionTextBlock.Text = ViewModel.EmptyStateDescription;

        var hasItems = ViewModel.ActiveItems.Count > 0 || ViewModel.CompletedItems.Count > 0;
        ContentPanel.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
        EmptyStateBorder.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;
        AddItemButton.IsEnabled = !ViewModel.IsBusy;
        AddItemButton.Opacity = ViewModel.IsBusy ? 0.72 : 1;
        ApplyThemeResources(ViewModel.CurrentTheme);

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

    private static bool TryGetSwipeItemId(SwipeItem sender, out Guid itemId)
    {
        itemId = default;

        if (sender.CommandParameter is Guid typedId)
        {
            itemId = typedId;
            return true;
        }

        if (sender.CommandParameter is string itemIdText &&
            Guid.TryParse(itemIdText, out itemId))
        {
            return true;
        }

        return false;
    }

    private void ApplyThemeResources(string themeKey)
    {
        var palette = ResolveThemePalette(themeKey);
        SetBrushColor("ListPagePageBackgroundBrush", palette.PageBackground);
        SetBrushColor("ListPageHeaderBackgroundBrush", palette.ToolbarBackground);
        SetBrushColor("ListPageHeaderBorderBrush", palette.ToolbarBorder);
        SetBrushColor("ListPageSectionBackgroundBrush", palette.SectionBackground);
        SetBrushColor("ListPageSectionBorderBrush", palette.SectionBorder);
        SetBrushColor("ListPageItemBackgroundBrush", palette.ItemBackground);
        SetBrushColor("ListPageActiveBadgeBrush", palette.ActiveBadge);
        SetBrushColor("ListPageCompletedBadgeBrush", palette.CompletedBadge);
        SetBrushColor("ListPageStatusBrush", palette.StatusForeground);
    }

    private void SetBrushColor(string resourceKey, Color color)
    {
        if (Resources[resourceKey] is SolidColorBrush brush)
        {
            brush.Color = color;
        }
    }

    private static ListPageThemePalette ResolveThemePalette(string themeKey)
    {
        return themeKey switch
        {
            "sunrise" => new ListPageThemePalette(
                PageBackground: ColorHelper.FromArgb(0xFF, 0xFF, 0xF1, 0xE3),
                ToolbarBackground: ColorHelper.FromArgb(0xFF, 0xFF, 0xFA, 0xF2),
                ToolbarBorder: ColorHelper.FromArgb(0xFF, 0xE5, 0xB6, 0x7D),
                SectionBackground: ColorHelper.FromArgb(0xFF, 0xFF, 0xF7, 0xED),
                SectionBorder: ColorHelper.FromArgb(0xFF, 0xE8, 0xC2, 0x99),
                ItemBackground: ColorHelper.FromArgb(0xFF, 0xFF, 0xE8, 0xD2),
                ActiveBadge: ColorHelper.FromArgb(0xFF, 0xE6, 0x8A, 0x4A),
                CompletedBadge: ColorHelper.FromArgb(0xFF, 0xF2, 0xBD, 0x8B),
                StatusForeground: ColorHelper.FromArgb(0xFF, 0xA8, 0x4C, 0x1D)),
            "forest" => new ListPageThemePalette(
                PageBackground: ColorHelper.FromArgb(0xFF, 0xEC, 0xF4, 0xEE),
                ToolbarBackground: ColorHelper.FromArgb(0xFF, 0xF6, 0xFB, 0xF6),
                ToolbarBorder: ColorHelper.FromArgb(0xFF, 0x84, 0xA9, 0x88),
                SectionBackground: ColorHelper.FromArgb(0xFF, 0xF2, 0xF8, 0xF1),
                SectionBorder: ColorHelper.FromArgb(0xFF, 0x97, 0xB7, 0x98),
                ItemBackground: ColorHelper.FromArgb(0xFF, 0xDE, 0xEC, 0xDE),
                ActiveBadge: ColorHelper.FromArgb(0xFF, 0x4A, 0x8A, 0x67),
                CompletedBadge: ColorHelper.FromArgb(0xFF, 0x93, 0xBF, 0x9B),
                StatusForeground: ColorHelper.FromArgb(0xFF, 0x2F, 0x63, 0x46)),
            "slate" => new ListPageThemePalette(
                PageBackground: ColorHelper.FromArgb(0xFF, 0xEC, 0xEF, 0xF4),
                ToolbarBackground: ColorHelper.FromArgb(0xFF, 0xF6, 0xF8, 0xFC),
                ToolbarBorder: ColorHelper.FromArgb(0xFF, 0x8F, 0x9D, 0xB3),
                SectionBackground: ColorHelper.FromArgb(0xFF, 0xF3, 0xF5, 0xF9),
                SectionBorder: ColorHelper.FromArgb(0xFF, 0xA0, 0xAD, 0xC0),
                ItemBackground: ColorHelper.FromArgb(0xFF, 0xDF, 0xE6, 0xF0),
                ActiveBadge: ColorHelper.FromArgb(0xFF, 0x5E, 0x79, 0xA6),
                CompletedBadge: ColorHelper.FromArgb(0xFF, 0xA4, 0xB6, 0xD2),
                StatusForeground: ColorHelper.FromArgb(0xFF, 0x3C, 0x4D, 0x6B)),
            _ => new ListPageThemePalette(
                PageBackground: ColorHelper.FromArgb(0xFF, 0xF6, 0xF4, 0xEF),
                ToolbarBackground: ColorHelper.FromArgb(0xFF, 0xFD, 0xFB, 0xF4),
                ToolbarBorder: ColorHelper.FromArgb(0xFF, 0xD9, 0xD2, 0xC3),
                SectionBackground: ColorHelper.FromArgb(0xFF, 0xFF, 0xFC, 0xF5),
                SectionBorder: ColorHelper.FromArgb(0xFF, 0xD9, 0xD2, 0xC3),
                ItemBackground: ColorHelper.FromArgb(0xFF, 0xF6, 0xF0, 0xE6),
                ActiveBadge: ColorHelper.FromArgb(0xFF, 0xD3, 0xA1, 0x5D),
                CompletedBadge: ColorHelper.FromArgb(0xFF, 0xE2, 0xCF, 0xAE),
                StatusForeground: ColorHelper.FromArgb(0xFF, 0x8A, 0x5A, 0x2B))
        };
    }

    private readonly record struct ListPageThemePalette(
        Color PageBackground,
        Color ToolbarBackground,
        Color ToolbarBorder,
        Color SectionBackground,
        Color SectionBorder,
        Color ItemBackground,
        Color ActiveBadge,
        Color CompletedBadge,
        Color StatusForeground);
}
