using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

#pragma warning disable

public class PedStorageService : IPedStorageService, IInternalPedStorageService
{
    private readonly ConcurrentDictionary<int, FPed> _peds = new();

    public ReadOnlyDictionary<int, FPed> Peds => new(this._peds);

    ConcurrentDictionary<int, FPed> IInternalPedStorageService.InternalPeds => this._peds;

    /// <inheritdoc/>
    public FPed TryGetPed(int networkId)
    {
        var found = this._peds.TryGetValue(networkId, out var ped);
        if (found)
        {
            return ped;
        }

        ped = FPed.FromNetId(networkId);
        this._peds[networkId] = ped;

        return ped;
    }
}