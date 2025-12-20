using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedFleeState : FPedState
{
    private readonly FPed _target;

    public FPedFleeState(FPed fPed, FPed target)
        : base(fPed)
    {
        this._target = target;
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedFleeState.");
        this.FPed.Stopped.Value = false;
        this.FPed.Task.FleeFrom(this._target.GetUnderlyingPed.Position);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}