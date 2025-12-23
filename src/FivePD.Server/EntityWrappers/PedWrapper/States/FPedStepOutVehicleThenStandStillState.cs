using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedStepOutVehicleThenStandStillState : FPedState
{
    public FPedStepOutVehicleThenStandStillState(FPed fPed)
        : base(fPed)
    {
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedStepOutVehicleThenStandStillState.");

        this.FPed.Stopped.Value = true;
        this.FPed.Movement.Value = FPedMovement.None;
        this.FPed.Task.LeaveVehicle();
        this.FPed.Task.StandStill(-1);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}
