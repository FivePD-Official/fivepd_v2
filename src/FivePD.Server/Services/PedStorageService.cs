using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Repositories;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class PedStorageService : IPedStorageService, IInternalPedStorageService
{
    private readonly ConcurrentDictionary<int, FPed> _peds = new();

    private readonly PedDataRepository _pedDataRepository;

    public ReadOnlyDictionary<int, FPed> Peds => new(this._peds);

    ConcurrentDictionary<int, FPed> IInternalPedStorageService.InternalPeds => this._peds;

    public PedStorageService(PedDataRepository pedDataRepository)
    {
        this._pedDataRepository = pedDataRepository;
    }

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

    /// <inheritdoc/>
    public PedData GetPedData(FPed ped)
    {
        if (ped is null)
        {
            return null;
        }

        return this._pedDataRepository.FindByIdWithDynamicRelated(ped.PedDataId, new PedDataRepository.PedDataLoadOptions { IncludeLicenses = true });
    }
}