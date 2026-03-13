using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class AddItemPage : Page
{
    public AddItemPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<AddItemPageViewModel>();
    }
}
