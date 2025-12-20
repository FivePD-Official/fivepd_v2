using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedDriveVehiclePullOverState : FPedState
{
    private readonly ICfxService _cfxService;

    public FPedDriveVehiclePullOverState(FPed fPed, FVehicle fVehicle)
        : base(fPed, fVehicle)
    {
        this._cfxService = ServiceLocator.GetService<ICfxService>();
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedDriveVehiclePullOverState.");
        this.FPed.Task.ClearAll();
        this._cfxService.TaskVehicleTempAction(this.FPed.NetworkId, this.FVehicle.NetworkId, 24, int.MaxValue);
        this.FPed.Task.StandStill(-1);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}