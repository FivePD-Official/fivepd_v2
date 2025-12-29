using System.Collections.Generic;
using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class VehicleConfigUpdate<TOptions> : ChangeTracker
{
    private VehicleConfigType _type;
    private string? _name;
    private string? _model;
    private int _livery;
    private List<int>? _extras;
    private TOptions? _options;

    public VehicleConfigType Type
    {
        get => this._type;
        set => this.SetField(ref this._type, value, nameof(Type));
    }

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

    public int Livery
    {
        get => this._livery;
        set => this.SetField(ref this._livery, value, nameof(Livery));
    }

    public List<int>? Extras
    {
        get => this._extras;
        set => this.SetField(ref this._extras, value, nameof(Extras));
    }

    public TOptions? Options
    {
        get => this._options;
        set => this.SetField(ref this._options, value, nameof(Options));
    }
}