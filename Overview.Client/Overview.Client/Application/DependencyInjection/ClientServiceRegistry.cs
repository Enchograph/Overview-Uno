using System;
using System.Collections.Generic;
using System.Net.Http;
using Overview.Client.Application.Auth;
using Overview.Client.Infrastructure.Api.Auth;
using Overview.Client.Infrastructure.Api.Sync;
using Overview.Client.Infrastructure.Diagnostics;
using Overview.Client.Infrastructure.Notifications;
using Overview.Client.Infrastructure.Settings;
using Overview.Client.Infrastructure.Widgets;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Application.DependencyInjection;

internal sealed class ClientServiceRegistry
{
    private readonly Dictionary<Type, Func<object>> registrations = new();

    public static ClientServiceRegistry CreateDefault()
    {
        var registry = new ClientServiceRegistry();
        registry.RegisterSingleton(() => new ShellViewModel());
        registry.RegisterSingleton(() => new HomePageViewModel());
        registry.RegisterSingleton(() => new ListPageViewModel());
        registry.RegisterSingleton(() => new AiPageViewModel());
        registry.RegisterSingleton(() => new AddItemPageViewModel());
        registry.RegisterSingleton(() => new SettingsPageViewModel());
        registry.RegisterSingleton(() => new HttpClient());
        registry.RegisterSingleton<IOverviewLoggerFactory>(() => NullOverviewLoggerFactory.Instance);
        registry.RegisterSingleton<INotificationScheduler>(() => new NoOpNotificationScheduler());
        registry.RegisterSingleton<IWidgetSnapshotStore>(() => new InMemoryWidgetSnapshotStore());
        registry.RegisterSingleton<IAuthSessionStore>(() => new FileAuthSessionStore());
        registry.RegisterSingleton<IAuthRemoteClient>(() => new AuthRemoteClient(registry.Resolve<HttpClient>()));
        registry.RegisterSingleton<IAuthenticationService>(() => new AuthenticationService(
            registry.Resolve<IAuthRemoteClient>(),
            registry.Resolve<IAuthSessionStore>(),
            registry.Resolve<IOverviewLoggerFactory>()));
        registry.RegisterSingleton<ISyncRemoteClient>(() => new SyncRemoteClient(registry.Resolve<HttpClient>()));
        return registry;
    }

    public void RegisterSingleton<TService>(Func<TService> factory)
        where TService : class
    {
        var lazy = new Lazy<TService>(factory);
        registrations[typeof(TService)] = () => lazy.Value;
    }

    public TService Resolve<TService>()
        where TService : class
    {
        if (!registrations.TryGetValue(typeof(TService), out var factory))
        {
            throw new InvalidOperationException($"Service {typeof(TService).FullName} is not registered.");
        }

        return (TService)factory();
    }
}
