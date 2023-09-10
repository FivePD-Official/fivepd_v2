using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for various entity management related tasks.
/// </summary>
public interface IEntityService
{
    /// <summary>
    /// Spawns a ped via RPC natives.
    /// </summary>
    /// <param name="model">A ped model for the ped to use.</param>
    /// <param name="location">A location where the ped will spawn.</param>
    /// <param name="heading">The heading the ped will use.</param>
    /// <returns>The spawned ped.</returns>
    Task<FPed> SpawnPedAsync(uint model, Vector3 location, float heading);

    /// <summary>
    /// Spawns a vehicle directly on the server.
    /// </summary>
    /// <param name="model">A ped model for the ped to use.</param>
    /// <param name="location">A location where the ped will spawn.</param>
    /// <param name="heading">The heading the ped will use.</param>
    /// <returns>The spawned vehicle.</returns>
    Task<FVehicle> SpawnVehicleAsync(uint model, Vector3 location, float heading);

    /// <summary>
    /// Spawns a prop (object) via RPC natives.
    /// </summary>
    /// <param name="model">A model for the prop to use.</param>
    /// <param name="location">A location where the prop will spawn.</param>
    /// <returns>The spawned prop.</returns>
    Task<Prop> SpawnPropAsync(int model, Vector3 location);

    /// <summary>
    /// Gets the closest entity to a player.
    /// </summary>
    /// <param name="player">The player to use.</param>
    /// <typeparam name="T">The type of entity to get.</typeparam>
    /// <returns>The closest entity.</returns>
    public Task<T> GetClosestEntityToPlayerAsync<T>(Player player)
        where T : FEntity;
}