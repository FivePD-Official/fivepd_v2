using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class AssistanceServiceTypeConfigUpdate : ChangeTracker
{
    private string? _typeName;
    private string? _assemblyName;
    private bool _active;
    private bool _enabled;
    private string? _spawnType;
    private List<Vector3>? _manualSpawnLocations;

    public string? TypeName
    {
        get => this._typeName;
        set => this.SetField(ref this._typeName, value, nameof(TypeName));
    }

    public string? AssemblyName
    {
        get => this._assemblyName;
        set => this.SetField(ref this._assemblyName, value, nameof(AssemblyName));
    }

    public bool Active
    {
        get => this._active;
        set => this.SetField(ref this._active, value, nameof(Active));
    }

    public bool Enabled
    {
        get => this._enabled;
        set => this.SetField(ref this._enabled, value, nameof(Enabled));
    }

    public string? SpawnType
    {
        get => this._spawnType;
        set => this.SetField(ref this._spawnType, value, nameof(SpawnType));
    }

    public List<Vector3>? ManualSpawnLocations
    {
        get => this._manualSpawnLocations;
        set => this.SetField(ref this._manualSpawnLocations, value, nameof(ManualSpawnLocations));
    }
}
