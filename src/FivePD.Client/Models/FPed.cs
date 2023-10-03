using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
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
        this.Detained = new FStateBagObservable<bool>(this, StateBagKeys.Ped.Detained, false);
        this.Movement = new FStateBagObservable<FPedMovement>(this, StateBagKeys.Ped.Movement, FPedMovement.None);
    }

    public FStateBagObservable<bool> Stopped { get; private set; }

    public FStateBagObservable<bool> HasGeneratedData { get; private set; }

    public FStateBagObservable<bool> Detained { get; private set; }

    public FStateBagObservable<FPedMovement> Movement { get; private set; }

    public string GetStateKey()
    {
        return $"ped:{this.Ped.NetworkId}";
    }

    public StateBag GetStateBag()
    {
        return this.Ped.State;
    }
}