using System;
using FivePD.Common.Attributes;

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

    public static T GetService<T>()
    {
        return (T)serviceProvider.GetService(typeof(T));
    }
}