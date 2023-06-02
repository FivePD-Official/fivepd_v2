#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

public record struct GetSetEntitiesRequest()
{
    public string SetId { get; set; }

    public int Page { get; set; }

    public RequestFilters Filters { get; set; } = default;

    public record struct RequestFilters
    {
    }
}