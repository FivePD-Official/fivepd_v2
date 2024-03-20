using FivePD.Common.Enums;
using FivePD.Server.Interfaces;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedGrabbedState : FPedState
{
    private readonly ICfxService _cfxService;
    private readonly FPed _playerPed;

    public FPedGrabbedState(FPed ped, FPed playerPed)
        : base(ped)
    {
        this._cfxService = ServiceLocator.GetService<ICfxService>();
        this._playerPed = playerPed;
    }

    public override void Enter()
    {
        this.Ped.Movement.Value = FPedMovement.Grab;
        this.Ped.GetUnderlyingPed.Heading = this._playerPed.GetUnderlyingPed.Heading;
        this._playerPed.Task.PlayAnimation(
            "doors@",
            "door_sweep_r_hand_medium",
            5f,
            -1,
            FAnimationFlags.StayInEndFrame | FAnimationFlags.UpperBodyOnly | FAnimationFlags.AllowRotation);
        this._cfxService.AttachEntityToEntity(
            this.Ped.NetworkId,
            this._playerPed.GetUnderlyingPed.NetworkId,
            0,
            0.2f,
            0.4f,
            0f,
            0f,
            0f,
            0f,
            false,
            false,
            false,
            false,
            2,
            false);
    }

    public override void Exit()
    {
        this._cfxService.DetachEntity(this.Ped.NetworkId, true, false);
        this._playerPed.Task.ClearAllImmediately();
        this._playerPed.Task.ClearAnimation("doors@", "door_sweep_r_hand_medium");
    }
}