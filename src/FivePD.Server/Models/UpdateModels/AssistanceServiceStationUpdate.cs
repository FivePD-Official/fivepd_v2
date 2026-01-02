using CitizenFX.Core;
using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class AssistanceServiceStationUpdate : ChangeTracker
{
    private string? _name;
    private Vector3 _position;

    public string? Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value, nameof(Name));
    }

    public Vector3 Position
    {
        get => this._position;
        set => this.SetField(ref this._position, value, nameof(Position));
    }
}
