using System;
using System.Collections.Generic;
using System.Net.Http;
using Overview.Client.Application.Ai;
using Overview.Client.Application.Auth;
using Overview.Client.Application.Home;
using Overview.Client.Application.Items;
using Overview.Client.Application.Lists;
using Overview.Client.Application.Settings;
using Overview.Client.Application.Sync;
using Overview.Client.Domain.Rules;
using Overview.Client.Infrastructure.Api.Auth;
using Overview.Client.Infrastructure.Api.Sync;
using Overview.Client.Infrastructure.Diagnostics;
using Overview.Client.Infrastructure.Notifications;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Persistence.Services;
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
        registry.RegisterSingleton(() => new LoginPageViewModel(registry.Resolve<IAuthenticationService>()));
        registry.RegisterSingleton(() => new HomePageViewModel(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<IHomeLayoutService>()));
        registry.RegisterSingleton(() => new TimeSelectionViewModel(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<ITimeSelectionService>()));
        registry.RegisterSingleton(() => new ListPageViewModel());
        registry.RegisterSingleton(() => new AiPageViewModel());
        registry.RegisterSingleton(() => new AddItemPageViewModel(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<IItemService>(),
            registry.Resolve<IUserSettingsService>()));
        registry.RegisterSingleton(() => new SettingsPageViewModel(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<IUserSettingsService>()));
        registry.RegisterSingleton(() => new HttpClient());
        registry.RegisterSingleton<IOverviewLoggerFactory>(() => NullOverviewLoggerFactory.Instance);
        registry.RegisterSingleton<INotificationScheduler>(() => new NoOpNotificationScheduler());
        registry.RegisterSingleton<IWidgetSnapshotStore>(() => new InMemoryWidgetSnapshotStore());
        registry.RegisterSingleton<ISqliteConnectionFactory>(() => new SqliteConnectionFactory());
        registry.RegisterSingleton<IItemRepository>(() => new SqliteItemRepository(registry.Resolve<ISqliteConnectionFactory>()));
        registry.RegisterSingleton<IUserSettingsRepository>(() => new SqliteUserSettingsRepository(registry.Resolve<ISqliteConnectionFactory>()));
        registry.RegisterSingleton<ISyncChangeRepository>(() => new SqliteSyncChangeRepository(registry.Resolve<ISqliteConnectionFactory>()));
        registry.RegisterSingleton<ITimeRuleService>(() => new TimeRuleService());
        registry.RegisterSingleton<IHomeInteractionRuleService>(() => new HomeInteractionRuleService());
        registry.RegisterSingleton<IAuthSessionStore>(() => new FileAuthSessionStore());
        registry.RegisterSingleton<ISyncStateStore>(() => new FileSyncStateStore());
        registry.RegisterSingleton<IDeviceIdStore>(() => new FileDeviceIdStore());
        registry.RegisterSingleton(() => TimeProvider.System);
        registry.RegisterSingleton<IAuthRemoteClient>(() => new AuthRemoteClient(registry.Resolve<HttpClient>()));
        registry.RegisterSingleton<IAuthenticationService>(() => new AuthenticationService(
            registry.Resolve<IAuthRemoteClient>(),
            registry.Resolve<IAuthSessionStore>(),
            registry.Resolve<IOverviewLoggerFactory>()));
        registry.RegisterSingleton<ISyncRemoteClient>(() => new SyncRemoteClient(registry.Resolve<HttpClient>()));
        registry.RegisterSingleton<IItemService>(() => new ItemService(
            registry.Resolve<IItemRepository>(),
            registry.Resolve<ISyncChangeRepository>(),
            registry.Resolve<IDeviceIdStore>()));
        registry.RegisterSingleton<IUserSettingsService>(() => new UserSettingsService(
            registry.Resolve<IUserSettingsRepository>(),
            registry.Resolve<ISyncChangeRepository>(),
            registry.Resolve<IDeviceIdStore>()));
        registry.RegisterSingleton<IHomeLayoutService>(() => new HomeLayoutService(
            registry.Resolve<IItemService>(),
            registry.Resolve<IUserSettingsService>(),
            registry.Resolve<ITimeRuleService>(),
            registry.Resolve<IHomeInteractionRuleService>()));
        registry.RegisterSingleton<IListPageService>(() => new ListPageService(
            registry.Resolve<IItemService>(),
            registry.Resolve<IUserSettingsService>()));
        registry.RegisterSingleton<IAiOrchestrationService>(() => new AiOrchestrationService(
            registry.Resolve<IItemService>(),
            registry.Resolve<IUserSettingsService>()));
        registry.RegisterSingleton<ITimeSelectionService>(() => new TimeSelectionService(
            registry.Resolve<IUserSettingsService>(),
            registry.Resolve<ITimeRuleService>()));
        registry.RegisterSingleton<ISyncOrchestrationService>(() => new SyncOrchestrationService(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<IItemRepository>(),
            registry.Resolve<IUserSettingsRepository>(),
            registry.Resolve<ISyncChangeRepository>(),
            registry.Resolve<ISyncRemoteClient>(),
            registry.Resolve<ISyncStateStore>(),
            registry.Resolve<IDeviceIdStore>(),
            registry.Resolve<IOverviewLoggerFactory>(),
            registry.Resolve<TimeProvider>()));
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
