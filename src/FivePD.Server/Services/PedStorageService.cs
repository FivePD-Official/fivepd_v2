using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class PedStorageService : IPedStorageService, IInternalPedStorageService
{
    private readonly ConcurrentDictionary<int, FPed> _peds = new();

    private readonly IDatabaseContextFactory _dbContextFactory;

    public ReadOnlyDictionary<int, FPed> Peds => new(this._peds);

    ConcurrentDictionary<int, FPed> IInternalPedStorageService.InternalPeds => this._peds;

    public PedStorageService(IDatabaseContextFactory dbContextFactory)
    {
        this._dbContextFactory = dbContextFactory;
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
        
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        return databaseContext.PedData
            .Include(x => x.Licenses)
            .FirstOrDefault(x => x.Id == ped.PedDataId);
    }
}