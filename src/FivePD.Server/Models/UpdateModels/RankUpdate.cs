using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class RankUpdate : ChangeTracker
{
    private bool _global;
    private int _requiredXp;
    private string? _name;
    private Rank.Permissions? _permission;

    public bool Global
    {
        get => this._global;
        set => this.SetField(ref this._global, value, nameof(Global));
    }

    public int RequiredXp
    {
        get => this._requiredXp;
        set => this.SetField(ref this._requiredXp, value, nameof(RequiredXp));
    }

    public string? Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value, nameof(Name));
    }

    public Rank.Permissions? Permission
    {
        get => this._permission;
        set => this.SetField(ref this._permission, value, nameof(Permission));
    }
}