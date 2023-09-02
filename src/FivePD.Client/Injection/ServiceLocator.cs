using System;

namespace FivePD.Client.Injection;

#pragma warning disable

internal static class ServiceLocator
{
    private static ServiceProvider serviceProvider;

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

    public static void Dispose()
    {
        serviceProvider.Dispose();
    }
}