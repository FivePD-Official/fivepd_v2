using FivePD.Server.Database;
using FivePD.Server.Models.VariableData;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable

public class VariableDataAttributeUpdate : ChangeTracker
{
    private string _setId;
    private VariableDataType _dataType;
    private string _addonId;
    private string _name;
    private int _displayDisplayOrder;
    private bool _required;
    private bool _hidden;
    private bool _readonly;
    private bool _showInGrid;
    private string _placeholder;
    private string _label;
    private string _helpLabel;
    private VariableDataAttribute.AttributeOptions _options;

    public string SetId
    {
        get => this._setId;
        set => this.SetField(ref this._setId, value, nameof(SetId));
    }
    
    public VariableDataType DataType
    {
        get => this._dataType;
        set => this.SetField(ref this._dataType, value, nameof(DataType));
    }
    
    public string AddonId
    {
        get => this._addonId;
        set => this.SetField(ref this._addonId, value, nameof(AddonId));
    }
    
    public string Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value, nameof(Name));
    }
    
    public int DisplayOrder
    {
        get => this._displayDisplayOrder;
        set => this.SetField(ref this._displayDisplayOrder, value, nameof(this.DisplayOrder));
    }
    
    public bool Required
    {
        get => this._required;
        set => this.SetField(ref this._required, value, nameof(Required));
    }
    
    public bool Hidden
    {
        get => this._hidden;
        set => this.SetField(ref this._hidden, value, nameof(Hidden));
    }
    
    public bool Readonly
    {
        get => this._readonly;
        set => this.SetField(ref this._required, value, nameof(Readonly));
    }
    
    public bool ShowInGrid
    {
        get => this._showInGrid;
        set => this.SetField(ref this._showInGrid, value, nameof(ShowInGrid));
    }
    
    public string Placeholder
    {
        get => this._placeholder;
        set => this.SetField(ref this._placeholder, value, nameof(Placeholder));
    }
    
    public string Label
    {
        get => this._label;
        set => this.SetField(ref this._label, value, nameof(Label));
    }

    public string HelpLabel
    {
        get => this._helpLabel;
        set => this.SetField(ref this._helpLabel, value, nameof(HelpLabel));
    }
 
    public VariableDataAttribute.AttributeOptions Options
    {
        get => this._options;
        set => this.SetField(ref this._options, value, nameof(Options));
    }
}