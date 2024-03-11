using System.Collections.Generic;
using System.Collections.ObjectModel;
using FivePD.Server.Models;

#pragma warning disable

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

internal interface IInternalUnitService
{
    List<Unit> Units { get; }
}