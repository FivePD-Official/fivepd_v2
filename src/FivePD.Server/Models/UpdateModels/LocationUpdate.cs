using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class LocationUpdate : ChangeTracker
{
    private string? _name;
    private LocationType _type;
    private float _x;
    private float _y;
    private float _z;

    public string? Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value, nameof(Name));
    }

    public LocationType Type
    {
        get => this._type;
        set => this.SetField(ref this._type, value, nameof(Type));
    }

    public float X
    {
        get => this._x;
        set => this.SetField(ref this._x, value, nameof(X));
    }

    public float Y
    {
        get => this._y;
        set => this.SetField(ref this._y, value, nameof(Y));
    }

    public float Z
    {
        get => this._z;
        set => this.SetField(ref this._z, value, nameof(Z));
    }
}