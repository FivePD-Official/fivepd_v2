using System;
using System.Collections.Generic;
using FivePD.Gamemode.Common.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace FivePD.Gamemode.Server;

#pragma warning disable

public static class ServiceLocator
{
    private static IServiceProvider serviceProvider;

    [NoUsage]
    internal static void SetServiceProvider(IServiceProvider provider)
    {
        serviceProvider = provider;
    }

    public static object GetService(Type type)
    {
        return serviceProvider.GetService(type);
    }

    public static T GetService<T>()
    {
        return (T)serviceProvider.GetService(typeof(T));
    }

    public static IEnumerable<object> GetServices(Type type)
    {
        return serviceProvider.GetServices(type);
    }

    public static IEnumerable<T> GetServices<T>()
    {
        return (IEnumerable<T>)serviceProvider.GetServices(typeof(T));
    }
}