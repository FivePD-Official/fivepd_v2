#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct AddAssistanceServiceStationTypeConfigRequest
{
    public int StationId { get; set; }

    public int TypeConfigId { get; set; }

    public int Capacity { get; set; }
}