using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class WeaponComponentUpdate : ChangeTracker
{
    private string? _name;
    private string? _hashKey;

    public string? Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value, nameof(Name));
    }

    public string? HashKey
    {
        get => this._hashKey;
        set => this.SetField(ref this._hashKey, value, nameof(HashKey));
    }
}