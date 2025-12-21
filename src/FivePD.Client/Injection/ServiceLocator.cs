using System;
using System.Collections.Generic;

namespace FivePD.Client.Injection;

#pragma warning disable

internal static class ServiceLocator
{
    private static ServiceProvider serviceProvider;

    public static bool IsInitialized => serviceProvider is not null;

    internal static void SetServiceProvider(ServiceProvider provider)
    {
        serviceProvider = provider;
    }
    
    public static object Resolve(Type type)
    {
        return serviceProvider.Resolve(type);
    }

    public static T Resolve<T>()
    {
        return (T)serviceProvider.Resolve(typeof(T));
    }

    public static IEnumerable<T> ResolveAll<T>()
    {
        return serviceProvider.ResolveAll<T>();
    }

    public static void Dispose()
    {
        serviceProvider.Dispose();
    }
}