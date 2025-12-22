using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
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
    /// <param name="clearArea">A boolean value representing whether the area should be cleared.</param>
    /// <returns>The spawned ped.</returns>
    public Task<FPed> SpawnPedAsync(uint model, Vector3 location, float heading, bool clearArea = false);

    /// <summary>
    /// Spawns a vehicle directly on the server.
    /// </summary>
    /// <param name="model">A ped model for the ped to use.</param>
    /// <param name="location">A location where the ped will spawn.</param>
    /// <param name="heading">The heading the ped will use.</param>
    /// <param name="clearArea">A boolean value representing whether the area should be cleared.</param>
    /// <returns>The spawned vehicle.</returns>
    public Task<FVehicle> SpawnVehicleAsync(uint model, Vector3 location, float heading, bool clearArea = false);

    /// <summary>
    /// Spawns a prop (object) via RPC natives.
    /// </summary>
    /// <param name="model">A model for the prop to use.</param>
    /// <param name="location">A location where the prop will spawn.</param>
    /// <param name="clearArea">A boolean value representing whether the area should be cleared.</param>
    /// <returns>The spawned prop.</returns>
    public Task<Prop> SpawnPropAsync(int model, Vector3 location, bool clearArea = false);

    /// <summary>
    /// Gets all peds.
    /// </summary>
    /// <returns>A list of all peds.</returns>
    public List<Ped> GetAllPeds();

    /// <summary>
    /// Gets all peds.
    /// </summary>
    /// <returns>A list of all peds.</returns>
    public List<FPed> GetAllFPeds();

    /// <summary>
    /// Gets the closest ped to the specified entity.
    /// !! Use this if you do not need generated data for the closest ped.
    /// </summary>
    /// <param name="entity">The entity to use.</param>
    /// <param name="rangeSquared">The squared range to use.</param>
    /// <returns>The closest ped.</returns>
    public Ped GetClosestPedToEntity(Entity entity, float rangeSquared = SquaredDistances._15);

    /// <summary>
    /// Gets the closest ped to the specified entity.
    /// !! Use this if you do not need generated data for the closest ped.
    /// </summary>
    /// <param name="entity">The entity to use.</param>
    /// <param name="rangeSquared">The squared range to use.</param>
    /// <returns>The closest ped.</returns>
    public FPed GetClosestFPedToEntity(Entity entity, float rangeSquared = SquaredDistances._15);

    /// <summary>
    /// Gets all peds in range of the specified position.
    /// </summary>
    /// <param name="pos">The position to use.</param>
    /// <param name="rangeSquared">The squared range to use.</param>
    /// <returns>The peds.</returns>
    public List<Ped> GetPedsInRangeOfPosition(Vector3 pos, float rangeSquared = SquaredDistances._15);

    /// <summary>
    /// Gets all peds in range of the specified entity.
    /// </summary>
    /// <param name="entity">The entity to use.</param>
    /// <param name="rangeSquared">The squared range to use.</param>
    /// <returns>The peds.</returns>
    public List<Ped> GetPedsInRangeOfEntity(Entity entity, float rangeSquared = SquaredDistances._15);

    /// <summary>
    /// Gets all vehicles.
    /// </summary>
    /// <returns>A list of all vehicles.</returns>
    public List<Vehicle> GetAllVehicles();

    /// <summary>
    /// Gets all vehicles.
    /// </summary>
    /// <returns>A list of all vehicles.</returns>
    public List<FVehicle> GetAllFVehicles();

    /// <summary>
    /// Gets the closest vehicle to the specified entity.
    /// !! Use this if you do not need generated data for the closest vehicle.
    /// </summary>
    /// <param name="entity">The entity to use.</param>
    /// <param name="rangeSquared">The squared range to use.</param>
    /// <returns>The closest vehicle.</returns>
    public Vehicle GetClosestVehicleToEntity(Entity entity, float rangeSquared = SquaredDistances._15);

    /// <summary>
    /// Gets the closest vehicle to the specified entity.
    /// !! Use this if you do not need generated data for the closest vehicle.
    /// </summary>
    /// <param name="entity">The entity to use.</param>
    /// <param name="rangeSquared">The squared range to use.</param>
    /// <returns>The closest vehicle.</returns>
    public FVehicle GetClosestFVehicleToEntity(Entity entity, float rangeSquared = SquaredDistances._15);

    /// <summary>
    /// Gets all vehicles in range of the specified position.
    /// </summary>
    /// <param name="pos">The position to use.</param>
    /// <param name="rangeSquared">The squared range to use.</param>
    /// <returns>The vehicles.</returns>
    public List<Vehicle> GetVehiclesInRangeOfPosition(Vector3 pos, float rangeSquared = SquaredDistances._15);

    /// <summary>
    /// Gets all vehicles in range of the specified entity.
    /// </summary>
    /// <param name="entity">The entity to use.</param>
    /// <param name="rangeSquared">The squared range to use.</param>
    /// <returns>The vehicles.</returns>
    public List<Vehicle> GetVehiclesInRangeOfEntity(Entity entity, float rangeSquared = SquaredDistances._15);

    /// <summary>
    /// Gets the closest entity to a player.
    /// !! Use this if you need generated data for the closest entity.
    /// </summary>
    /// <param name="player">The player to use.</param>
    /// <typeparam name="T">The type of entity to get.</typeparam>
    /// <returns>The closest entity.</returns>
    public Task<T> GetClosestEntityToPlayerAsync<T>(Player player)
        where T : FEntity;

    /// <summary>
    /// Deletes all vehicles and peds within the specified radius of the specified entity.
    /// </summary>
    /// <param name="pos">The position to use.</param>
    /// <param name="radius">The radius to use.</param>
    public void ClearArea(Vector3 pos, float radius = SquaredDistances._15);
}