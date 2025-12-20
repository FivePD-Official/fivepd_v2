using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers.VehicleWrapper;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedDriveVehicleFleeState : FPedState
{
    public FPedDriveVehicleFleeState(FPed fPed, FVehicle fVehicle)
        : base(fPed, fVehicle)
    {
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedDriveVehicleFleeState.");
        this.FPed.Stopped.Value = false;
        this.FPed.Task.CruiseWithVehicle(this.FVehicle.NetworkId, 60f, (int)FDrivingStyle.Flee);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}