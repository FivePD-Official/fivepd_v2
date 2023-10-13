using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;

namespace FivePD.Client.Models;

#pragma warning disable

internal sealed class FVehicle : IFStatefulContainer
{
    public Vehicle Vehicle { get; private set; }

    public FVehicle(Vehicle vehicle)
    {
        this.Vehicle = vehicle;

        this.Stopped = new FStateBagObservable<bool>(this, StateBagKeys.Vehicle.Stopped, false);
        this.HasGeneratedData = new FStateBagObservable<bool>(this, StateBagKeys.Vehicle.HasGeneratedData, false);
    }

    public FStateBagObservable<bool> Stopped { get; private set; }

    public FStateBagObservable<bool> HasGeneratedData { get; private set; }

    public string GetStateKey()
    {
        return $"vehicle:{this.Vehicle?.NetworkId ?? 0}";
    }

    public StateBag GetStateBag()
    {
        return this.Vehicle?.State;
    }
}