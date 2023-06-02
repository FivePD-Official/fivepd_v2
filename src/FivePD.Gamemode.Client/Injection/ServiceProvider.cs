using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FivePD.Gamemode.Client.Injection;

/// <summary>
/// Provides an interface to resolve registered services. Built using an <see cref="ServiceProviderBuilder"/>.
/// </summary>
public class ServiceProvider
{
    private readonly Dictionary<string, object> _instances = new();
    private readonly List<Registration> _registrations;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
    /// </summary>
    /// <param name="registrations">The registrations to use in creation.</param>
    public ServiceProvider(List<Registration> registrations)
    {
        this._registrations = registrations;
    }

    /// <summary>
    /// Resolves a matching instance of the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to search for and return.</param>
    /// <returns>A constructed instance of the given <paramref name="type"/>.</returns>
    public object Resolve(Type type)
    {
        var matchingRegistration = this._registrations.SingleOrDefault(r => r.Types.Any(t => t == type));

        if (matchingRegistration == null)
        {
            throw new ArgumentException("Could not resolve type " + type.AssemblyQualifiedName);
        }

        if (matchingRegistration.IsSingleton)
        {
            return this.ResolveSingleInstance(matchingRegistration);
        }

        return this.CreateInstance(matchingRegistration);
    }

    /// <summary>
    /// Resolves a matching instance of the given <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to search for and return.</typeparam>
    /// <returns>A constructed instance of the given <typeparamref name="T"/>.</returns>
    public T Resolve<T>()
    {
        var resolveType = typeof(T);
        return (T)this.Resolve(resolveType);
    }

    /// <summary>
    /// Resolves all matching instances of the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to search for and return.</param>
    /// <returns>The constructed instances of the given <paramref name="type"/>.</returns>
    public IEnumerable<object> ResolveAll(Type type)
    {
        var matchingRegistrations = this._registrations.Where(r => r.Types.Any(t => t == type)).ToList();

        if (matchingRegistrations.Any(x => x is null))
        {
            throw new ArgumentException("Could not resolve type " + type.AssemblyQualifiedName);
        }

        var instances = new List<object>();
        foreach (var registration in matchingRegistrations)
        {
            if (registration.IsSingleton)
            {
                instances.Add(this.ResolveSingleInstance(registration));
                continue;
            }

            instances.Add(this.CreateInstance(registration));
        }

        return instances;
    }

    /// <summary>
    /// Resolves all matching instances of the given <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to search for and return.</typeparam>
    /// <returns>The constructed instances of the given <typeparamref name="T"/>.</returns>
    public IEnumerable<T> ResolveAll<T>()
    {
        var resolveType = typeof(T);
        return this.ResolveAll(resolveType).Cast<T>();
    }

    private object ResolveSingleInstance(Registration registration)
    {
        var assemblyQualifiedName = registration.InstanceType.AssemblyQualifiedName;

        if (this._instances.ContainsKey(assemblyQualifiedName ?? throw new InvalidOperationException()))
        {
            return this._instances[assemblyQualifiedName];
        }

        var instance = this.CreateInstance(registration);
        this._instances.Add(assemblyQualifiedName, instance);

        return instance;
    }

    private object CreateInstance(Registration registration)
    {
        if (registration.Factory != null)
        {
            return registration.Factory();
        }

        var constructor = registration.InstanceType.GetConstructors().FirstOrDefault();
        var parameters = constructor?.GetParameters().Select(this.ResolveConstructorParameter).ToArray();

        var instance = Activator.CreateInstance(registration.InstanceType, parameters);

        return instance;
    }

    private object ResolveConstructorParameter(ParameterInfo parameterInfo)
    {
        return this.Resolve(parameterInfo.ParameterType);
    }
}