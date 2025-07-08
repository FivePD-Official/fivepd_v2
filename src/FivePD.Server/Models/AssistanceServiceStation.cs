using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CitizenFX.Core;

namespace FivePD.Server.Models;

#pragma warning disable

[Table("fivepd_assistance_service_stations")]
public class AssistanceServiceStation
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Vector3 Position { get; set; }

    public IList<AssistanceServiceStationTypeConfig> ServiceConfigs { get; set; }
}