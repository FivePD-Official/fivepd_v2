using FivePD.Server.Models.VariableData;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditSetAttributeRequest
{
    public string SetId { get; set; }

    public VariableDataAttribute Attribute { get; set; }
}