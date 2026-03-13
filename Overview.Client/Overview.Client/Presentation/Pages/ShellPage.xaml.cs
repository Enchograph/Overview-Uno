using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Overview.Client.Application.Sync;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class ShellPage : Page
{
    private readonly ISyncLifecycleCoordinator syncLifecycleCoordinator;
    private readonly Dictionary<string, Type> pageMap = new()
    {
        ["Home"] = typeof(HomePage),
        ["List"] = typeof(ListPage),
        ["Ai"] = typeof(AiPage),
        ["Add"] = typeof(AddItemPage),
        ["Settings"] = typeof(SettingsPage)
    };

    public ShellPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.Resolve<ShellViewModel>();
        syncLifecycleCoordinator = App.Services.Resolve<ISyncLifecycleCoordinator>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await syncLifecycleCoordinator.HandleShellLoadedAsync().ConfigureAwait(true);
        NavigateTo("Home");
    }

    private async void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        await syncLifecycleCoordinator.HandleShellUnloadedAsync().ConfigureAwait(true);
    }

    private void OnNavigationButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string target)
        {
            NavigateTo(target);
        }
    }

    private void NavigateTo(string target)
    {
        if (!pageMap.TryGetValue(target, out var pageType))
        {
            return;
        }

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
