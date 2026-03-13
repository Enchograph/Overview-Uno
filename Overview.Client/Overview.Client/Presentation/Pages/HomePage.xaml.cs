using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<HomePageViewModel>();
    }
}
