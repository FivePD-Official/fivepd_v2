using FivePD.Server.Models.LooseDataSet;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditSetAttributeRequest
{
    public string SetId { get; set; }

    public LooseDataAttribute Attribute { get; set; }
}