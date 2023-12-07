using System.Collections.Generic;

namespace FivePD.Common.ConfigModels;

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
    /// Gets the vehicles that'll be used for the tow truck service.
    /// </summary>
    public List<TowTruckVehicle> TowTruck { get; init; } = new();

    /// <summary>
    /// Defines a police vehicle's properties.
    /// </summary>
    public sealed record PoliceVehicle : ServiceVehicle
    {
        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public required string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the spike strip is available on this vehicle.
        /// </summary>
        public bool SpikeStrip { get; init; }
    }

    /// <summary>
    /// Defines a tow truck's properties.
    /// </summary>
    public sealed record TowTruckVehicle : ServiceVehicle
    {
        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public PositionConfig TowedVehiclePositionOffset { get; init; } = default;

        /// <summary>
        /// Gets a value indicating whether the siren should be activated or not.
        /// </summary>
        public bool SirenActive { get; init; } = false;
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
        public int Livery { get; init; } = 0;

        /// <summary>
        /// Gets the extras of this vehicle.
        /// </summary>
        public List<int> Extras { get; init; } = new();
    }
}