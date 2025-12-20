using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedLeaveVehicleState : FPedState
{
    private readonly ICfxService _cfxService;

    public FPedLeaveVehicleState(FPed fPed, FVehicle fVehicle)
        : base(fPed, fVehicle)
    {
        this._cfxService = ServiceLocator.GetService<ICfxService>();
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedLeaveVehicleState.");
        this._cfxService.TaskVehicleTempAction(this.FPed.NetworkId, this.FVehicle.NetworkId, 24, int.MaxValue);
        this.FPed.Task.LeaveVehicle(this.FVehicle.GetUnderlyingVehicle, false);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}