using System.Collections.Generic;

namespace FivePD.Gamemode.Common.ConfigModels;

/// <summary>
/// Contains the definition of the vehicles config file.
/// </summary>
public sealed record Vehicles
{
    /// <summary>
    /// Gets police vehicles that can spawned with the duty menu.
    /// </summary>
    public List<PoliceVehicle> Police { get; init; } = new();

    /// <summary>
    /// Gets the vehicles that'll be used for the ambulance service.
    /// </summary>
    public List<ServiceVehicle> Ambulance { get; init; } = new();

    /// <summary>
    /// Defines a police vehicle's properties.
    /// </summary>
    public sealed record PoliceVehicle : ServiceVehicle
    {
        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public required string Name { get; init; } = string.Empty;
    }

    /// <summary>
    /// Defines a service vehicle's properties.
    /// </summary>
    public record ServiceVehicle
    {
        /// <summary>
        /// Gets the model name of this vehicle.
        /// </summary>
        public required string Model { get; init; }

        /// <summary>
        /// Gets the livery of this vehicle.
        /// </summary>
        public int Livery { get; init; }

        /// <summary>
        /// Gets the extras of this vehicle.
        /// </summary>
        public List<string> Extras { get; init; } = new();
    }
}