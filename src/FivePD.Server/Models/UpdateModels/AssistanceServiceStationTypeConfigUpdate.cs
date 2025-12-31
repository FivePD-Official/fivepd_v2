using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class AssistanceServiceStationTypeConfigUpdate : ChangeTracker
{
    private int _assistanceServiceStationId;
    private int _typeConfigId;
    private int _capacity;
    private int _activeCapacity;
    
    public int AssistanceServiceStationId
    {
        get => this._assistanceServiceStationId;
        set => this.SetField(ref this._assistanceServiceStationId, value, nameof(AssistanceServiceStationId));
    }

    public int TypeConfigId
    {
        get => this._typeConfigId;
        set => this.SetField(ref this._typeConfigId, value, nameof(TypeConfigId));
    }

    public int Capacity
    {
        get => this._capacity;
        set => this.SetField(ref this._capacity, value, nameof(Capacity));
    }

    public int ActiveCapacity
    {
        get => this._activeCapacity;
        set => this.SetField(ref this._activeCapacity, value, nameof(ActiveCapacity));
    }
}
