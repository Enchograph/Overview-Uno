using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Presentation.Pages;

public sealed partial class ShellPage : Page
{
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
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        NavigateTo("Home");
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
