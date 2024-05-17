using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FivePD.Server.API;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces.Addons;

/// <summary>
/// Provides easy access to information about addons.
/// </summary>
public interface IAddonMetadataService
{
    /// <summary>
    /// Gets the loaded callouts.
    /// </summary>
    public ReadOnlyCollection<Type> CalloutTypes { get; }

    /// <summary>
    /// Gets the loaded extensions.
    /// </summary>
    public ReadOnlyCollection<IExtension> Extensions { get; }
}

/// <summary>
/// Provides easy access to information about addons.
/// </summary>
internal interface IInternalAddonMetadataService
{
    /// <summary>
    /// Gets the loaded callouts.
    /// </summary>
    public ReadOnlyCollection<CalloutType> CalloutTypes { get; }

    /// <summary>
    /// Gets the loaded extensions.
    /// </summary>
    public ReadOnlyCollection<IExtension> Extensions { get; }

    /// <summary>
    /// Instantiates a callout from a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The callout.</returns>
    public Callout InstantiateCallout(Type type);

    /// <summary>
    /// Gets information about the executing addon.
    /// </summary>
    /// <returns>An <see cref="AddonDefinition"/> for the currently executing assembly.</returns>
    public AddonDefinition GetExecutingAddonDefinition();

    /// <summary>
    /// Loads assemblies from a list of definitions.
    /// </summary>
    /// <param name="definitions">The definitions to load.</param>
    public void LoadAssemblies(List<AddonDefinition> definitions);

    /// <summary>
    /// Finds and registers addons.
    /// </summary>
    public void FindAndRegisterAddons();

    /// <summary>
    /// Creates callout details entities.
    /// </summary>
    public void CreateCalloutDetailsEntities();
}

internal sealed record CalloutType(Type Type, string AssemblyName);