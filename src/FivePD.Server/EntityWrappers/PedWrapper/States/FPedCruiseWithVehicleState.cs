using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedCruiseWithVehicleState : FPedState
{
    private readonly int _vehicleNetworkId;
    
    public FPedCruiseWithVehicleState(FPed fPed, int vehicleNetworkId)
        : base(fPed)
    {
        this._vehicleNetworkId = vehicleNetworkId;
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedCruiseWithVehicleState.");

        this.FPed.Stopped.Value = false;
        this.FPed.Task.CruiseWithVehicle(this._vehicleNetworkId, 20f);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}