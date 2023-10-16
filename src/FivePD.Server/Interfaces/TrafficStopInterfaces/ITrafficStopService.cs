using FivePD.Server.API;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces.TrafficStopInterfaces;

/// <summary>
/// Represents a service for managing traffic stops.
/// </summary>
public interface ITrafficStopService
{
    /// <summary>
    /// Generates a traffic stop.
    /// </summary>
    /// <param name="unit">The <see cref="Unit"/> to assign to the traffic stop.</param>
    /// <param name="vehicle">The <see cref="FVehicle"/> to assign to the traffic stop.</param>
    /// <returns>The generated <see cref="TrafficStop"/>.</returns>
    public TrafficStop GenerateTrafficStop(Unit unit, FVehicle vehicle);

    /// <summary>
    /// Assigns a <see cref="Unit"/> to a <see cref="TrafficStop"/>.
    /// </summary>
    /// <param name="unit">The <see cref="Unit"/> to assign.</param>
    /// <param name="trafficStop">The <see cref="TrafficStop"/> to assign the <see cref="Unit"/> to.</param>
    /// <returns>Whether the <see cref="Unit"/> was assigned to the <see cref="TrafficStop"/>.</returns>
    public bool AssignUnitToTrafficStop(Unit unit, TrafficStop trafficStop);
}