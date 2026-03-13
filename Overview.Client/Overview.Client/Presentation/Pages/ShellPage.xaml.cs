using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Overview.Client.Application.Navigation;
using Overview.Client.Application.Sync;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class ShellPage : Page
{
    private readonly ISyncLifecycleCoordinator syncLifecycleCoordinator;
    private readonly Dictionary<AppNavigationTarget, Type> pageMap = new()
    {
        [AppNavigationTarget.Home] = typeof(HomePage),
        [AppNavigationTarget.List] = typeof(ListPage),
        [AppNavigationTarget.Ai] = typeof(AiPage),
        [AppNavigationTarget.Add] = typeof(AddItemPage),
        [AppNavigationTarget.Settings] = typeof(SettingsPage)
    };
    private AppNavigationRequest? pendingNavigationRequest;

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
        HandleNavigationRequest(pendingNavigationRequest ?? App.PeekPendingNavigationRequest());
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        pendingNavigationRequest = e.Parameter as AppNavigationRequest;
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
            NavigateTo(ParseTarget(target), parameter: null);
        }
    }

    public void HandleNavigationRequest(AppNavigationRequest? request)
    {
        if (request is null)
        {
            NavigateTo(AppNavigationTarget.Home, parameter: null);
            return;
        }

        NavigateTo(request.Target, request.ResolveNavigationParameter());
        App.ClearPendingNavigationRequest();
        pendingNavigationRequest = null;
    }

    private void NavigateTo(AppNavigationTarget target, object? parameter)
    {
        if (!pageMap.TryGetValue(target, out var pageType))
        {
            return;
        }

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType, parameter);
            return;
        }

        if (parameter is not null)
        {
            ContentFrame.Navigate(pageType, parameter);
        }
    }

    private static AppNavigationTarget ParseTarget(string target)
    {
        return target switch
        {
            "List" => AppNavigationTarget.List,
            "Ai" => AppNavigationTarget.Ai,
            "Add" => AppNavigationTarget.Add,
            "Settings" => AppNavigationTarget.Settings,
            _ => AppNavigationTarget.Home
        };
    }
}
