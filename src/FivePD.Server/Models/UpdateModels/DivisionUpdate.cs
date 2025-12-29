using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class DivisionUpdate : ChangeTracker
{
    private string? _name;
    private int _departmentId;

    public string? Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value, nameof(Name));
    }

    public int DepartmentId
    {
        get => this._departmentId;
        set => this.SetField(ref this._departmentId, value, nameof(DepartmentId));
    }
}