namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct MessageGetAllPageableRequest()
{
    public int Page { get; set; }

    public RequestFilters Filters { get; set; } = default;

    public record struct RequestFilters
    {
    }
}