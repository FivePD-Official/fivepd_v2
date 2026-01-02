using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable

public class VariableDataValueUpdate : ChangeTracker
{
    private int _attributeId;
    private string _entityId;
    private string _value;

    public int AttributeId
    {
        get => this._attributeId;
        set => this.SetField(ref this._attributeId, value, nameof(AttributeId));
    }

    public string EntityId
    {
        get => this._entityId;
        set => this.SetField(ref this._entityId, value, nameof(EntityId));
    }
    
    public string Value
    {
        get => this._value;
        set => this.SetField(ref this._value, value, nameof(Value));
    }
}