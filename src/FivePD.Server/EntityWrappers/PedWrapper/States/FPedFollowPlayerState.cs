using FivePD.Common.Enums;
using FivePD.Server.Interfaces;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedFollowPlayerState : FPedState
{
    private readonly IInternalInteractionService _interactionService;
    private readonly FPlayer _player;
    
    public FPedFollowPlayerState(FPed ped, FPlayer player)
        : base(ped)
    {
        this._interactionService = ServiceLocator.GetService<IInternalInteractionService>();
        this._player = player;
    }

    public override void Enter()
    {
        this.Ped.Movement.Value = FPedMovement.Follow;
        this._interactionService.TogglePedFollowPlayer(this._player, this.Ped.GetUnderlyingPed, true);
    }

    public override void Exit()
    {
        this._interactionService.TogglePedFollowPlayer(this._player, this.Ped.GetUnderlyingPed, false);
    }
}