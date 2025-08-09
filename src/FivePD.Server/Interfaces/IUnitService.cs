using System.Collections.Generic;
using System.Collections.ObjectModel;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides methods for managing units.
/// </summary>
public interface IUnitService
{
    /// <summary>
    /// Gets the units.
    /// </summary>
    /// <returns>A list of units.</returns>
    public ReadOnlyCollection<Unit> Units();
}

/// <summary>
/// Internal interface for managing units.
/// </summary>
internal interface IInternalUnitService
{
    /// <summary>
    /// Gets the list of units.
    /// </summary>
    List<Unit> Units { get; }
}