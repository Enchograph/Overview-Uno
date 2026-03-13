using System;
using System.Collections.Generic;
using System.Net.Http;
using Overview.Client.Application.Ai;
using Overview.Client.Application.Auth;
using Overview.Client.Application.Home;
using Overview.Client.Application.Items;
using Overview.Client.Application.Lists;
using Overview.Client.Application.Notifications;
using Overview.Client.Application.Settings;
using Overview.Client.Application.Sync;
using Overview.Client.Application.Widgets;
using Overview.Client.Infrastructure.Api.Ai;
using Overview.Client.Domain.Rules;
using Overview.Client.Infrastructure.Api.Auth;
using Overview.Client.Infrastructure.Api.Sync;
using Overview.Client.Infrastructure.Diagnostics;
using Overview.Client.Infrastructure.Notifications;
using Overview.Client.Infrastructure.Persistence.Repositories;
using Overview.Client.Infrastructure.Persistence.Services;
using Overview.Client.Infrastructure.Platform;
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
            registry.Resolve<IHomeLayoutService>(),
            registry.Resolve<IHomeTimelineInteractionService>(),
            registry.Resolve<IItemService>()));
        registry.RegisterSingleton(() => new TimeSelectionViewModel(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<ITimeSelectionService>()));
        registry.RegisterSingleton(() => new ListPageViewModel(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<IItemService>(),
            registry.Resolve<IListPageService>()));
        registry.RegisterSingleton(() => new AiPageViewModel(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<IAiChatService>(),
            registry.Resolve<ITimeSelectionService>()));
        registry.RegisterSingleton(() => new AddItemPageViewModel(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<IItemService>(),
            registry.Resolve<IUserSettingsService>()));
        registry.RegisterSingleton(() => new SettingsPageViewModel(
            registry.Resolve<IAuthenticationService>(),
            registry.Resolve<IUserSettingsService>(),
            registry.Resolve<ISyncOrchestrationService>(),
            registry.Resolve<IPlatformCapabilities>()));
        registry.RegisterSingleton(() => new HttpClient());
        registry.RegisterSingleton<IOverviewLoggerFactory>(() => NullOverviewLoggerFactory.Instance);
        registry.RegisterSingleton<IPlatformCapabilities>(() => PlatformCapabilities.Current);
        registry.RegisterSingleton<INotificationScheduler>(() => new PlatformNotificationScheduler());
#if __WASM__
        registry.RegisterSingleton<INotificationStateStore>(() => new InMemoryNotificationStateStore());
        registry.RegisterSingleton<IWidgetSnapshotStore>(() => new InMemoryWidgetSnapshotStore());
        registry.RegisterSingleton<ISqliteConnectionFactory>(() => new SqliteConnectionFactory());
        registry.RegisterSingleton<IItemRepository>(() => new InMemoryItemRepository());
        registry.RegisterSingleton<IUserSettingsRepository>(() => new InMemoryUserSettingsRepository());
        registry.RegisterSingleton<IAiChatMessageRepository>(() => new InMemoryAiChatMessageRepository());
        registry.RegisterSingleton<ISyncChangeRepository>(() => new InMemorySyncChangeRepository());
        registry.RegisterSingleton<IAuthSessionStore>(() => new InMemoryAuthSessionStore());
        registry.RegisterSingleton<ISyncStateStore>(() => new InMemorySyncStateStore());
        registry.RegisterSingleton<IDeviceIdStore>(() => new InMemoryDeviceIdStore());
#else
        registry.RegisterSingleton<INotificationStateStore>(() => new FileNotificationStateStore());
        registry.RegisterSingleton<IWidgetSnapshotStore>(() => new FileWidgetSnapshotStore());
        registry.RegisterSingleton<ISqliteConnectionFactory>(() => new SqliteConnectionFactory());
        registry.RegisterSingleton<IItemRepository>(() => new SqliteItemRepository(registry.Resolve<ISqliteConnectionFactory>()));
        registry.RegisterSingleton<IUserSettingsRepository>(() => new SqliteUserSettingsRepository(registry.Resolve<ISqliteConnectionFactory>()));
        registry.RegisterSingleton<IAiChatMessageRepository>(() => new SqliteAiChatMessageRepository(registry.Resolve<ISqliteConnectionFactory>()));
        registry.RegisterSingleton<ISyncChangeRepository>(() => new SqliteSyncChangeRepository(registry.Resolve<ISqliteConnectionFactory>()));
        registry.RegisterSingleton<IAuthSessionStore>(() => new FileAuthSessionStore());
        registry.RegisterSingleton<ISyncStateStore>(() => new FileSyncStateStore());
        registry.RegisterSingleton<IDeviceIdStore>(() => new FileDeviceIdStore());
#endif
        registry.RegisterSingleton<IWidgetRenderer>(() => new PlatformWidgetRenderer());
        registry.RegisterSingleton<ITimeRuleService>(() => new TimeRuleService());
        registry.RegisterSingleton<IHomeInteractionRuleService>(() => new HomeInteractionRuleService());
        registry.RegisterSingleton<IReminderRuleService>(() => new ReminderRuleService());
        registry.RegisterSingleton(() => TimeProvider.System);
        registry.RegisterSingleton<IAuthRemoteClient>(() => new AuthRemoteClient(registry.Resolve<HttpClient>()));
        registry.RegisterSingleton<IAiRemoteClient>(() => new AiRemoteClient(registry.Resolve<HttpClient>()));
        registry.RegisterSingleton<IAuthenticationService>(() => new AuthenticationService(
            registry.Resolve<IAuthRemoteClient>(),
            registry.Resolve<IAuthSessionStore>(),
            registry.Resolve<IOverviewLoggerFactory>(),
            registry.Resolve<IWidgetRefreshService>()));
        registry.RegisterSingleton<INotificationRefreshService>(() => new NotificationRefreshService(
            registry.Resolve<IItemRepository>(),
            registry.Resolve<IUserSettingsRepository>(),
            registry.Resolve<IReminderRuleService>(),
            registry.Resolve<INotificationScheduler>(),
            registry.Resolve<INotificationStateStore>(),
            registry.Resolve<TimeProvider>()));
        registry.RegisterSingleton<IWidgetRefreshService>(() => new WidgetRefreshService(
            registry.Resolve<IItemRepository>(),
            registry.Resolve<IUserSettingsRepository>(),
            registry.Resolve<IAiChatMessageRepository>(),
            registry.Resolve<ITimeRuleService>(),
            registry.Resolve<IWidgetSnapshotStore>(),
            registry.Resolve<IWidgetRenderer>(),
            registry.Resolve<TimeProvider>()));
        registry.RegisterSingleton<ISyncRemoteClient>(() => new SyncRemoteClient(registry.Resolve<HttpClient>()));
        registry.RegisterSingleton<IItemService>(() => new ItemService(
            registry.Resolve<IItemRepository>(),
            registry.Resolve<ISyncChangeRepository>(),
            registry.Resolve<IDeviceIdStore>(),
            registry.Resolve<INotificationRefreshService>(),
            registry.Resolve<IWidgetRefreshService>()));
        registry.RegisterSingleton<IUserSettingsService>(() => new UserSettingsService(
            registry.Resolve<IUserSettingsRepository>(),
            registry.Resolve<ISyncChangeRepository>(),
            registry.Resolve<IDeviceIdStore>(),
            registry.Resolve<INotificationRefreshService>(),
            registry.Resolve<IWidgetRefreshService>()));
        registry.RegisterSingleton<IHomeLayoutService>(() => new HomeLayoutService(
            registry.Resolve<IItemService>(),
            registry.Resolve<IUserSettingsService>(),
            registry.Resolve<ITimeRuleService>(),
            registry.Resolve<IHomeInteractionRuleService>()));
        registry.RegisterSingleton<IHomeTimelineInteractionService>(() => new HomeTimelineInteractionService(
            registry.Resolve<IHomeInteractionRuleService>()));
        registry.RegisterSingleton<IListPageService>(() => new ListPageService(
            registry.Resolve<IItemService>(),
            registry.Resolve<IUserSettingsService>()));
        registry.RegisterSingleton<IAiOrchestrationService>(() => new AiOrchestrationService(
            registry.Resolve<IItemService>(),
            registry.Resolve<IUserSettingsService>()));
        registry.RegisterSingleton<IAiChatService>(() => new AiChatService(
            registry.Resolve<IAiChatMessageRepository>(),
            registry.Resolve<IAiOrchestrationService>(),
            registry.Resolve<IItemService>(),
            registry.Resolve<IUserSettingsService>(),
            registry.Resolve<IAiRemoteClient>(),
            registry.Resolve<TimeProvider>()));
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
            registry.Resolve<TimeProvider>(),
            registry.Resolve<INotificationRefreshService>(),
            registry.Resolve<IWidgetRefreshService>()));
        registry.RegisterSingleton<ISyncLifecycleCoordinator>(() => new SyncLifecycleCoordinator(
            registry.Resolve<ISyncOrchestrationService>()));
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
