#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct VehicleGetAllPageableRequest()
{
    public int Page { get; set; }

    public RequestFilters Filters { get; set; } = default;

    public record struct RequestFilters
    {
        public string LicensePlate { get; set; }
    }
}