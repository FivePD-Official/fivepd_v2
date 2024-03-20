using System.Collections.Generic;
using System.Collections.ObjectModel;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

#pragma warning disable

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IUnitService" />
[Obfuscate]
public class UnitService : IUnitService, IInternalUnitService
{
    private readonly List<Unit> _units = new();

    List<Unit> IInternalUnitService.Units => this._units;

    /// <inheritdoc />
    public ReadOnlyCollection<Unit> Units()
    {
        return this._units.AsReadOnly();
    }
}