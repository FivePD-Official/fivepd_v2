using FivePD.Common.Enums;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct IncidentHistoryGetAllRequest()
{
    public int Page { get; set; }

    public IncidentHistoryGetAllFilters Filters { get; set; } = default;

    public record struct IncidentHistoryGetAllFilters
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public int Type { get; set; }
    }
}