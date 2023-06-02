#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.Requests;

public record struct DeleteEntityRequest
{
    public string SetId { get; set; }

    public int Id { get; set; }
}