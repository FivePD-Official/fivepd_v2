using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class DepartmentAreaCoordinateUpdate : ChangeTracker
{
    private int? _departmentId;
    private float _x;
    private float _y;

    public int? DepartmentId
    {
        get => this._departmentId;
        set => this.SetField(ref this._departmentId, value, nameof(DepartmentId));
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
}