using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;

namespace FivePD.Client.Models;

#pragma warning disable

internal sealed class FPed : IFStatefulContainer
{
    private readonly Ped _ped;

    public FPed(Ped ped)
    {
        this._ped = ped;

        this.Stopped = new FStateBagObservable<bool>(this, StateBagKeys.Ped.Stopped, false);
    }

    public FStateBagObservable<bool> Stopped { get; private set; }

    public string GetStateKey()
    {
        return $"ped:{this._ped.NetworkId}";
    }

    public StateBag GetStateBag()
    {
        return this._ped.State;
    }
}