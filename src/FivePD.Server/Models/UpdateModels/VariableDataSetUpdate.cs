using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable

public class VariableDataSetUpdate : ChangeTracker
{
    private string _id;

    public string Id
    {
        get => this._id;
        set => this.SetField(ref this._id, value, nameof(Id));
    }
}