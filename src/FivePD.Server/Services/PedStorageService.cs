using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

#pragma warning disable

public class PedStorageService : IPedStorageService, IInternalPedStorageService
{
    private readonly ConcurrentDictionary<int, FPed> _peds = new();

    private readonly DatabaseContext _databaseContext;

    public ReadOnlyDictionary<int, FPed> Peds => new(this._peds);

    ConcurrentDictionary<int, FPed> IInternalPedStorageService.InternalPeds => this._peds;

    public PedStorageService(DatabaseContext databaseContext)
    {
        this._databaseContext = databaseContext;
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
        return ped is null ? null : this._databaseContext.PedHistories.FirstOrDefault(x => x.Id == ped.PedHistoryId);
    }
}