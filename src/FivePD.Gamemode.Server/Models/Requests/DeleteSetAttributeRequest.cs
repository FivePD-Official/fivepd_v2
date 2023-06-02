#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

public record struct DeleteSetAttributeRequest
{
    public string SetId { get; set; }

    public int AttributeId { get; set; }
}