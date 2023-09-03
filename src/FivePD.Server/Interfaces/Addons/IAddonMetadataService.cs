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
    /// Gets the loaded callout types.
    /// </summary>
    public ReadOnlyCollection<Type> CalloutTypes { get; }

    /// <summary>
    /// Gets information about the executing addon.
    /// </summary>
    /// <returns>An <see cref="AddonDefinition"/> for the currently executing assembly.</returns>
    public AddonDefinition GetExecutingAddonDefinition();

    // TODO: change to internal LoadAssemblies and FindAndRegisterAddons

    /// <summary>
    /// Loads.
    /// </summary>
    /// <param name="definitions">The definitions to load.</param>
    public void LoadAssemblies(List<AddonDefinition> definitions);

    /// <summary>
    /// Aasd.
    /// </summary>
    public void FindAndRegisterAddons();
}

/// <summary>
/// Provides easy access to information about addons.
/// </summary>
internal interface IInternalAddonMetadataService
{
    /// <summary>
    /// Gets the loaded callout types.
    /// </summary>
    public ReadOnlyCollection<CalloutType> CalloutTypes { get; }

    /// <summary>
    /// Gets.
    /// </summary>
    public List<IExtension> Extensions { get; }

    /// <summary>
    /// Instantiates a callout from a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The callout.</returns>
    public Callout InstantiateCallout(Type type);
}

internal sealed record CalloutType(Type Type, string AssemblyName);