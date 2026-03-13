using System;
using System.Collections.Generic;
using Overview.Client.Presentation.ViewModels;

namespace Overview.Client.Application.DependencyInjection;

internal sealed class ClientServiceRegistry
{
    private readonly Dictionary<Type, Func<object>> registrations = new();

    public static ClientServiceRegistry CreateDefault()
    {
        var registry = new ClientServiceRegistry();
        registry.RegisterSingleton(() => new MainViewModel());
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
