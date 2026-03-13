using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Components;

public sealed partial class ItemDetailCard : UserControl
{
    public event EventHandler<Guid>? EditRequested;

    public ItemDetailCard()
    {
        this.InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (DataContext is ItemDetailViewModel viewModel)
        {
            LocationPanel.Visibility = viewModel.HasLocation ? Visibility.Visible : Visibility.Collapsed;
            DescriptionPanel.Visibility = viewModel.HasDescription ? Visibility.Visible : Visibility.Collapsed;
            EditButton.Visibility = viewModel.CanEdit ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            LocationPanel.Visibility = Visibility.Collapsed;
            DescriptionPanel.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Collapsed;
        }
    }

    private void OnEditButtonClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is ItemDetailViewModel { ItemId: Guid itemId })
        {
            EditRequested?.Invoke(this, itemId);
        }
    }
}
