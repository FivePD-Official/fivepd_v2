using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class LoadoutConfigUpdate : ChangeTracker
{
    private string? _name;

    public string? Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value, nameof(Name));
    }
}