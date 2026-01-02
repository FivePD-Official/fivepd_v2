using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class WeaponUpdate : ChangeTracker
{
    private string? _name;
    private string? _model;

    public string? Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value, nameof(Name));
    }

    public string? Model
    {
        get => this._model;
        set => this.SetField(ref this._model, value, nameof(Model));
    }
}