#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct VehicleConfigsRequest()
{
    public int Page { get; set; }

    public RequestFilters Filters { get; set; } = default;

    public record struct RequestFilters
    {
        public int Type { get; set; }

        public string Name { get; set; }

        public string Model { get; set; }
    }
}