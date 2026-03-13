using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class AiPage : Page
{
    public AiPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<AiPageViewModel>();
    }
}
