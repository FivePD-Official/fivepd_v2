using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Server.Extensions;

namespace FivePD.Server.Models.AssistanceServiceModels;

/// <summary>
/// Static class providing access to default vehicle configs.
/// </summary>
public static class DefaultVehicleConfigs
{
    /// <summary>
    /// Gets the default ambulance vehicle config.
    /// </summary>
    public static VehicleConfig<Dictionary<string, object>> Ambulance => new()
    {
        Type = VehicleConfigType.Ambulance,
        Name = "Ambulance",
        Model = "ambulance",
        Livery = 0,
        Extras = new List<int>(),
    };

    /// <summary>
    /// Gets the default police vehicle config.
    /// </summary>
    public static VehicleConfig<Dictionary<string, object>> Police => new()
    {
        Type = VehicleConfigType.Police,
        Name = "Police",
        Model = "police",
        Livery = 0,
        Extras = new List<int>(),
    };

    /// <summary>
    /// Gets the default tow truck vehicle config.
    /// </summary>
    public static VehicleConfig<TowTruckVehicleConfigOptions> TowTruck => new()
    {
        Type = VehicleConfigType.TowTruck,
        Name = "Tow Truck",
        Model = "towtruck",
        Livery = 0,
        Extras = new List<int>(),
        Options = new TowTruckVehicleConfigOptions()
        {
            SirenActive = false,
            TowedVehiclePositionOffset = new Vector3(1, 1, 1),
        },
    };
}