using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;

namespace FivePD.Client.Models;

#pragma warning disable

internal sealed class FPed : IFStatefulContainer
{
    public Ped Ped { get; private set; }

    public FPed(Ped ped)
    {
        this.Ped = ped;

        this.Stopped = new FStateBagObservable<bool>(this, StateBagKeys.Ped.Stopped, false);
        this.HasGeneratedData = new FStateBagObservable<bool>(this, StateBagKeys.Ped.HasGeneratedData, false);
    }

    public FStateBagObservable<bool> Stopped { get; private set; }

    public FStateBagObservable<bool> HasGeneratedData { get; private set; }

    public string GetStateKey()
    {
        return $"ped:{this.Ped.NetworkId}";
    }

    public StateBag GetStateBag()
    {
        return this.Ped.State;
    }
}