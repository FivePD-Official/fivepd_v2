#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct PedHistoryGetAllRequest()
{
    public int Page { get; set; }

    public RequestFilters Filters { get; set; } = default;

    public record struct RequestFilters
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public int Gender { get; set; }
    }
}