using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class SceneManagementService : ISceneManagementService
{
    private readonly Dictionary<Guid, SpeedZone> _speedZones = new();

    private readonly PlayerList _playerList;
    private readonly IEntityService _entityService;
    private readonly ICfxService _cfxService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneManagementService"/> class.
    /// </summary>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    /// <param name="entityService">The <see cref="IEntityService"/> to use.</param>
    /// <param name="cfxService">The <see cref="ICfxService"/> to use.</param>
    public SceneManagementService(
        PlayerList playerList,
        IEntityService entityService,
        ICfxService cfxService)
    {
        this._playerList = playerList;
        this._entityService = entityService;
        this._cfxService = cfxService;
    }

    /// <inheritdoc />
    public List<SpeedZone> SpeedZones()
    {
        return this._speedZones.Select(x => x.Value).ToList();
    }

    /// <inheritdoc />
    public Guid CreateSpeedZone(Vector3 position, int radius, int speed)
    {
        var id = Guid.NewGuid();
        var speedZone = new SpeedZone
        {
            Position = position,
            Radius = radius,
            Speed = speed,
        };
        this._speedZones[id] = speedZone;

        var dto = JsonConvert.SerializeObject(new CreateSpeedZoneDTO
        {
            Id = id,
            SpeedZone = speedZone,
        });
        foreach (var player in this._playerList)
        {
            player.TriggerEvent(Events.SceneManagement.CreateSpeedZone, dto);
        }

        return id;
    }

    /// <inheritdoc />
    public bool DeleteSpeedZone(Guid id)
    {
        if (!this._speedZones.ContainsKey(id))
        {
            return false;
        }

        this._speedZones.Remove(id);
        foreach (var player in this._playerList)
        {
            player.TriggerEvent(Events.SceneManagement.DeleteSpeedZone, id.ToString());
        }

        return true;
    }

    /// <inheritdoc />
    public void ReleaseSpeedZoneTraffic(Guid id)
    {
        if (!this._speedZones.TryGetValue(id, out var speedZone))
        {
            return;
        }

        #pragma warning disable

        // .Where(x => x.Position.DistanceToSquared(speedZone.Position) <= speedZone.Radius)
        var vehicles = this._entityService.GetAllVehicles().ToList();
        Debug.WriteLine($"vehicles: {vehicles.Count}");
        foreach (var vehicle in vehicles)
        {
            var pedHandle = Cfx.API.GetPedInVehicleSeat(vehicle.Handle, -1);
            Debug.WriteLine($"handle: {pedHandle}");
            if (Entity.FromHandle(pedHandle) is not Ped ped)
            {
                continue;
            }

            Debug.WriteLine($"CruiseWithVehicle: {pedHandle}");
            var fped = FPed.FromPed(ped);
            this._cfxService.SetEntityAsMissionEntity(fped.GetUnderlyingPed.NetworkId, true, true);
            fped.Task.CruiseWithVehicle(vehicle, 10f);
            this._cfxService.SetEntityAsMissionEntity(fped.GetUnderlyingPed.NetworkId, false, false);
        }
    }
}