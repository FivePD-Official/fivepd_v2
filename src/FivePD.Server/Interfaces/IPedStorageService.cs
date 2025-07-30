using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// A service for managing and accessing ped data.
/// </summary>
public interface IPedStorageService
{
    /// <summary>
    /// Gets a readonly dictionary of all peds on the server.
    /// </summary>
    public ReadOnlyDictionary<int, FPed> Peds { get; }

    /// <summary>
    /// Tries to get a <see cref="FPed"/> by its network ID.
    /// </summary>
    /// <param name="networkId">The ped's network ID.</param>
    /// <returns>The ped or null.</returns>
    public FPed TryGetPed(int networkId);

    /// <summary>
    /// Gets the <see cref="PedData"/> of the given <see cref="FPed"/>.
    /// </summary>
    /// <param name="ped">The ped.</param>
    /// <returns>The PedHistory of the ped.</returns>
    public PedData GetPedData(FPed ped);
}

/// <summary>
/// A service for managing and accessing ped data.
/// </summary>
internal interface IInternalPedStorageService
{
    /// <summary>
    /// Gets a dictionary of all peds on the server.
    /// </summary>
    public ConcurrentDictionary<int, FPed> InternalPeds { get; }
}