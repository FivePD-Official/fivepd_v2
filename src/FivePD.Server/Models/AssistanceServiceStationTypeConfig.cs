using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models;

#pragma warning disable

[Table("fivepd_assistance_service_station_type_configs")]
public class AssistanceServiceStationTypeConfig
{
    public int Id { get; set; }

    public AssistanceServiceTypeConfig TypeConfig { get; set; }

    public int Capacity { get; set; }

    public int ActiveCapacity { get; set; }
}