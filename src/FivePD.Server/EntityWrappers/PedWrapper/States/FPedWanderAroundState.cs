using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedWanderAroundState : FPedState
{
    public FPedWanderAroundState(FPed fPed)
        : base(fPed)
    {
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedWanderAroundState.");

        this.FPed.Stopped.Value = false;
        this.FPed.Movement.Value = FPedMovement.None;
        this.FPed.Task.WanderAround();
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}