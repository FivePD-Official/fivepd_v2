using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.Interfaces;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedFollowPlayerState : FPedState
{
    private readonly IInternalInteractionService _interactionService;
    private readonly FPlayer _player;
    
    public FPedFollowPlayerState(FPed fPed, FPlayer player)
        : base(fPed)
    {
        this._interactionService = ServiceLocator.GetService<IInternalInteractionService>();
        this._player = player;
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedFollowPlayerState.");
        this.FPed.Stopped.Value = true;
        this.FPed.Movement.Value = FPedMovement.Follow;
        this._interactionService.TogglePedFollowPlayer(this._player, this.FPed.GetUnderlyingPed, true);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        this._interactionService.TogglePedFollowPlayer(this._player, this.FPed.GetUnderlyingPed, false);
        return Task.CompletedTask;
    }
}