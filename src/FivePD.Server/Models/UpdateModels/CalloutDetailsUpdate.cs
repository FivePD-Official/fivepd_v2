using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class CalloutDetailsUpdate : ChangeTracker
{
    private bool _active;
    private string? _typeName;
    private string? _assemblyName;
    private CalloutDetailsRulesetType _rulesetType;

    public bool Active
    {
        get => this._active;
        set => this.SetField(ref this._active, value, nameof(Active));
    }

    public string? TypeName
    {
        get => this._typeName;
        set => this.SetField(ref this._typeName, value, nameof(TypeName));
    }

    public string? AssemblyName
    {
        get => this._assemblyName;
        set => this.SetField(ref this._assemblyName, value, nameof(AssemblyName));
    }

    public CalloutDetailsRulesetType RulesetType
    {
        get => this._rulesetType;
        set => this.SetField(ref this._rulesetType, value, nameof(RulesetType));
    }
}