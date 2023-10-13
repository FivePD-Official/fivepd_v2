using FivePD.Server.API;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces.TrafficStopInterfaces;

#pragma warning disable

public interface ITrafficStopService
{
    public TrafficStop GenerateTrafficStop(Unit unit, FVehicle vehicle);

    public bool AssignUnitToTrafficStop(Unit unit, TrafficStop trafficStop);
}