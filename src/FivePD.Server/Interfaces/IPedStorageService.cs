using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Models;

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

    /// <summary>
    /// Gets the <see cref="PedHistory"/> of the given <see cref="FPed"/>.
    /// </summary>
    /// <param name="ped">The ped.</param>
    /// <returns>The PedHistory of the ped.</returns>
    public PedHistory GetPedHistory(FPed ped);
}

internal interface IInternalPedStorageService
{
    public ConcurrentDictionary<int, FPed> InternalPeds { get; }
}