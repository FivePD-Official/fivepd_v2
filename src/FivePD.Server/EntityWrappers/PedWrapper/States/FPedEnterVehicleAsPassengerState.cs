using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers.VehicleWrapper;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedEnterVehicleAsPassengerState : FPedState
{
    public FPedEnterVehicleAsPassengerState(FPed fPed, FVehicle fVehicle)
        : base(fPed, fVehicle)
    {
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedEnterVehicleAsPassengerState.");
        
        if (this.FVehicle is null)
        {
            return Task.CompletedTask;
        }
        
        this.FPed.Stopped.Value = true;
        this.FPed.Movement.Value = FPedMovement.None;

        this.FPed.Task.FEnterVehicle(this.FVehicle.GetUnderlyingVehicle);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}
