using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable

public class VariableDataEntityUpdate : ChangeTracker
{
    private string _setId;

    public string SetId
    {
        get => this._setId;
        set => this.SetField(ref this._setId, value, nameof(SetId));
    }
}