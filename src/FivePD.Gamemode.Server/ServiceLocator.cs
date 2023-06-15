using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace FivePD.Gamemode.Server;

/// <summary>
/// Provides methods for retrieving services.
/// !! Only use this class if you can't use dependency injection !!.
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider serviceProvider;

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    /// <param name="type">The type of the service to get.</param>
    /// <returns>The service object of type <paramref name="type"/> or null if there is no such service.</returns>
    public static object GetService(Type type)
    {
        return serviceProvider.GetService(type);
    }

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service to get.</typeparam>
    /// <returns>The service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
    public static T GetService<T>()
    {
        return (T)serviceProvider.GetService(typeof(T));
    }

    /// <summary>
    /// Gets all services with the specified type.
    /// </summary>
    /// <param name="type">The type of the services to get.</param>
    /// <returns>An enumeration of services of type <paramref name="type"/>.</returns>
    public static IEnumerable<object> GetServices(Type type)
    {
        return serviceProvider.GetServices(type);
    }

    /// <summary>
    /// Gets all services with the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the services to get.</typeparam>
    /// <returns>An enumeration of services of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> GetServices<T>()
    {
        return (IEnumerable<T>)serviceProvider.GetServices(typeof(T));
    }

    /// <summary>
    /// Sets the service provider.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    internal static void SetServiceProvider(IServiceProvider provider)
    {
        serviceProvider = provider;
    }
}