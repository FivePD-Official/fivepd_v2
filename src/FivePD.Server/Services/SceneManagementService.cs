using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.DTO;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.ISceneManagementService" />
public class SceneManagementService : ISceneManagementService, IInternalSceneManagementService
{
    private readonly Dictionary<Guid, SpeedZoneWithPlayer> _speedZones = new();

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
        return this._speedZones.Select(x => x.Value.Zone).ToList();
    }

    /// <inheritdoc />
    public Guid CreateSpeedZone(Vector3 position, int radius, int speed)
    {
        return this.CreateSpeedZone(null, position, radius, speed);
    }

    /// <inheritdoc />
    Guid IInternalSceneManagementService.CreateSpeedZone(FPlayer player, Vector3 position, int radius, int speed)
    {
        return this.CreateSpeedZone(player, position, radius, speed);
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

        // TODO: multiplying by 20 doesn't seem right, but without it the distance is way off. the distance between the origin and origin+radius/2 == radius for some reason
        // TODO: also might be worth to check the distance calculation in the EntityService as well
        var vehicles = this._entityService.GetAllVehicles()
            .Where(x => Math.Sqrt(x.Position.DistanceToSquared2D(speedZone.Zone.Position)) <= speedZone.Zone.Radius)
            .ToList();
        foreach (var vehicle in vehicles)
        {
            var pedHandle = Cfx.API.GetPedInVehicleSeat(vehicle.Handle, -1);
            if (Entity.FromHandle(pedHandle) is not Ped ped)
            {
                continue;
            }

            var fped = FPed.FromPed(ped);
            this._cfxService.SetEntityAsMissionEntity(fped.GetUnderlyingPed.NetworkId, true, true);
            fped.Task.CruiseWithVehicle(vehicle, 10f);
            this._cfxService.SetEntityAsMissionEntity(fped.GetUnderlyingPed.NetworkId, false, false);
        }
    }

    private Guid CreateSpeedZone(FPlayer player, Vector3 position, int radius, int speed)
    {
        var id = Guid.NewGuid();
        var speedZone = new SpeedZone
        {
            Position = position,
            Radius = radius,
            Speed = speed,
        };
        this._speedZones[id] = new SpeedZoneWithPlayer(speedZone, player);

        var dto = JsonConvert.SerializeObject(new CreateSpeedZoneDTO
        {
            Id = id,
            SpeedZone = speedZone,
        });
        foreach (var p in this._playerList)
        {
            p.TriggerEvent(Events.SceneManagement.CreateSpeedZone, dto);
        }

        return id;
    }

    [UIEvent(Events.SceneManagement.GetAllSpeedZones)]
    private void GetAllSpeedZonesUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var speedZonesWithPlayers = this._speedZones.Select(a => new SpeedZoneWithPlayerDTO
        {
            SpeedZone = a.Value.Zone,
            PlayerName = a.Value.Player.GetUnderlyingPlayer().Name,
        }).ToList();
        networkCallbackDelegate.InvokeWithJson(speedZonesWithPlayers);
    }

    private sealed record SpeedZoneWithPlayer(SpeedZone Zone, FPlayer Player);
}