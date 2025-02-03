using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using FivePD.Server.EntityWrappers.PedWrapper;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IPedStorageService
{
    public ReadOnlyDictionary<int, FPed> Peds { get; }

    /// <summary>
    /// Tries to get a <see cref="FPed"/> by its network ID.
    /// </summary>
    /// <param name="networkId">The ped's network ID.</param>
    /// <returns>The ped or null.</returns>
    public FPed TryGetPed(int networkId);
}

internal interface IInternalPedStorageService
{
    public ConcurrentDictionary<int, FPed> InternalPeds { get; }
}