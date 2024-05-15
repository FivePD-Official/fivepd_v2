using FivePD.Server.Models.VariableData;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct AddSetAttributeRequest
{
    public string SetId { get; set; }

    public string AttributeType { get; set; }

    public VariableDataAttribute Attribute { get; set; }
}