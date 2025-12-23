using CitizenFX.Core;

namespace FivePD.Client.Interfaces;

#pragma warning disable

internal interface ITaskService
{
    /// <summary>
    /// Tasks the ped to enter the vehicle into the preferred seat or any available seat.
    /// </summary>
    /// <returns>1 if successful, 0 if failed, -1 if invalid parameters</returns>
    public int EnterVehicle(Ped ped, Vehicle vehicle, VehicleSeat preferredSeat = VehicleSeat.Any, int timeout = -1, float speed = 1.0f, int flag = 1);
}
