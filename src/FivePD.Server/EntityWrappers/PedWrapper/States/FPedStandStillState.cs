using FivePD.Common.Enums;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedStandStillState : FPedState
{
    public FPedStandStillState(FPed ped)
        : base(ped)
    {
    }

    public override void Enter()
    {
        this.Ped.Movement.Value = FPedMovement.None;
        this.Ped.Task.StandStill(-1);
    }

    public override void Exit()
    {
    }
}