#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

public record struct GetSetRequest
{
    public string SetId { get; set; }

    public int Page { get; set; }
}