using Overview.Client.Presentation.ViewModels;
using Overview.Client.Domain.Enums;
using Overview.Client.Presentation.Components;

namespace Overview.Client.Presentation.Pages;

public sealed partial class HomePage : Page
{
    private HomePageViewModel ViewModel => (HomePageViewModel)DataContext;

    public HomePage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<HomePageViewModel>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        SetSelectionModeComboBox(ViewModel.CurrentSelectionMode);
        await TimeSelectionPicker.InitializeAsync(ViewModel.CurrentSelectionMode).ConfigureAwait(true);
        ApplyViewModelState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }

    private async void OnSelectionModeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectionModeComboBox.SelectedItem is not ComboBoxItem { Tag: string tag })
        {
            return;
        }

        var mode = tag switch
        {
            "Day" => TimeSelectionMode.Day,
            "Week" => TimeSelectionMode.Week,
            "Month" => TimeSelectionMode.Month,
            _ => ViewModel.CurrentSelectionMode
        };

        ViewModel.SetSelectionMode(mode);
        ApplyViewModelState();
        await TimeSelectionPicker.ChangeSelectionModeAsync(mode).ConfigureAwait(true);
    }

    private void OnTimeSelectionConfirmed(object sender, TimeSelectionConfirmedEventArgs e)
    {
        ViewModel.ApplyConfirmedPeriod(e.SelectedPeriod);
        ApplyViewModelState();
    }

    private void ApplyViewModelState()
    {
        PageTitleTextBlock.Text = ViewModel.Title;
        PageDescriptionTextBlock.Text = ViewModel.Description;
        StatusTextBlock.Text = ViewModel.StatusMessage;
        ConfirmedSelectionTextBlock.Text = ViewModel.ConfirmedSelectionText;
    }

    private void SetSelectionModeComboBox(TimeSelectionMode selectionMode)
    {
        foreach (var item in SelectionModeComboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem &&
                comboBoxItem.Tag is string tag &&
                string.Equals(tag, selectionMode.ToString(), StringComparison.Ordinal))
            {
                SelectionModeComboBox.SelectedItem = comboBoxItem;
                break;
            }
        }
    }
}
