using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class ListPage : Page
{
    public ListPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<ListPageViewModel>();
    }
}
