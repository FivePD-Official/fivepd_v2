using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedTurnToHeadingThenStandStillState : FPedState
{
    private readonly float _heading;
    
    public FPedTurnToHeadingThenStandStillState(FPed fPed, float heading)
        : base(fPed)
    {
        this._heading = heading;
    }

    public override async Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedTurnToHeadingThenStandStillState.");
        this.FPed.Stopped.Value = true;
        this.FPed.Task.AchieveHeading(this._heading);
        await Task.Delay(1000);
        this.FPed.Task.StandStill(-1);
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}