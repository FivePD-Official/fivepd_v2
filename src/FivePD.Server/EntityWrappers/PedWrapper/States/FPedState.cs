using System.Threading.Tasks;
using FivePD.Server.EntityWrappers.VehicleWrapper;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public abstract class FPedState
{
    protected readonly FPed FPed;
    protected readonly FVehicle FVehicle;

    protected FPedState(FPed fPed, FVehicle fVehicle = null)
    {
        this.FPed = fPed;
        this.FVehicle = fVehicle;
    }

    public abstract Task Enter();

    public abstract Task Exit();
}