using CitizenFX.Core;

namespace FivePD.Server.Models.AssistanceServiceModels;

#pragma warning disable

public class TowTruckVehicleConfigOptions
{
    public bool SirenActive { get; set; }

    public Vector3 TowedVehiclePositionOffset { get; set; }
}