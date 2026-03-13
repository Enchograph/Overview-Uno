using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Overview.Client.Application.Navigation;
using Overview.Client.Application.Sync;
using Overview.Client.Presentation.Layout;
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
        SizeChanged += OnSizeChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyAdaptiveLayout(ActualWidth);
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
        SizeChanged -= OnSizeChanged;
        await syncLifecycleCoordinator.HandleShellUnloadedAsync().ConfigureAwait(true);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyAdaptiveLayout(e.NewSize.Width);
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

    private void ApplyAdaptiveLayout(double width)
    {
        var useNavigationRail = AdaptiveLayout.UseNavigationRail(width);

        NavigationPanel.Orientation = useNavigationRail ? Orientation.Vertical : Orientation.Horizontal;
        NavigationBorder.Padding = useNavigationRail ? new Thickness(12, 18, 12, 18) : new Thickness(12);
        NavigationBorder.BorderThickness = useNavigationRail
            ? new Thickness(0, 0, 1, 0)
            : new Thickness(0, 1, 0, 0);

        RootGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
        RootGrid.RowDefinitions[1].Height = useNavigationRail ? new GridLength(0) : GridLength.Auto;
        RootGrid.ColumnDefinitions[0].Width = useNavigationRail ? GridLength.Auto : new GridLength(1, GridUnitType.Star);
        RootGrid.ColumnDefinitions[1].Width = useNavigationRail ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        Grid.SetRow(ContentFrame, 0);
        Grid.SetColumn(ContentFrame, useNavigationRail ? 1 : 0);
        Grid.SetRowSpan(ContentFrame, useNavigationRail ? 2 : 1);
        Grid.SetColumnSpan(ContentFrame, useNavigationRail ? 1 : 2);

        Grid.SetRow(NavigationBorder, useNavigationRail ? 0 : 1);
        Grid.SetColumn(NavigationBorder, 0);
        Grid.SetRowSpan(NavigationBorder, useNavigationRail ? 2 : 1);
        Grid.SetColumnSpan(NavigationBorder, useNavigationRail ? 1 : 2);

        foreach (var child in NavigationPanel.Children.OfType<Button>())
        {
            child.MinWidth = useNavigationRail ? 112 : 0;
            child.HorizontalAlignment = useNavigationRail ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
        }
    }
}
