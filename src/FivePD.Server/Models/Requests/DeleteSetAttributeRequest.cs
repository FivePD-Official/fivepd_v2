#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct DeleteSetAttributeRequest
{
    public string SetId { get; set; }

    public int AttributeId { get; set; }
}