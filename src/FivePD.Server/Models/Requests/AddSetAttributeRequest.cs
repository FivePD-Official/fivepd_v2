using FivePD.Server.Models.LooseData;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct AddSetAttributeRequest
{
    public string SetId { get; set; }

    public string AttributeType { get; set; }

    public LooseDataAttribute Attribute { get; set; }
}