using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedStandStillThenTurnToPedState : FPedState
{
    private readonly FPed _turnTo;

    public FPedStandStillThenTurnToPedState(FPed fPed, FPed turnTo)
        : base(fPed)
    {
        this._turnTo = turnTo;
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedStandStillAndTurnToPedState.");
        this.FPed.Stopped.Value = true;
        this.FPed.Movement.Value = FPedMovement.None;
        this.FPed.Task.StandStill(-1);
        this.FPed.Task.TurnTo(this._turnTo.GetUnderlyingPed);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}