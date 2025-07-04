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
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.ISceneManagementService" />
[Obfuscate]
public class SceneManagementService : ISceneManagementService, IInternalSceneManagementService
{
    private readonly Dictionary<Guid, SpeedZoneWithPlayer> _speedZones = new();

    private readonly PlayerList _playerList;
    private readonly IEntityService _entityService;
    private readonly ICfxService _cfxService;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneManagementService"/> class.
    /// </summary>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    /// <param name="entityService">The <see cref="IEntityService"/> to use.</param>
    /// <param name="cfxService">The <see cref="ICfxService"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public SceneManagementService(
        PlayerList playerList,
        IEntityService entityService,
        ICfxService cfxService,
        INetworkEventService networkEventService)
    {
        this._playerList = playerList;
        this._entityService = entityService;
        this._cfxService = cfxService;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc />
    public event EventHandler<SpeedZoneCreatedEventArgs> SpeedZoneCreated;

    /// <inheritdoc />
    public event EventHandler<SpeedZoneDeletedEventArgs> SpeedZoneDeleted;

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

        var speedZone = this._speedZones[id];
        this.SpeedZoneDeleted?.Invoke(this, new SpeedZoneDeletedEventArgs(speedZone.Player, id, speedZone.Zone));

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

        this.SpeedZoneCreated?.Invoke(this, new SpeedZoneCreatedEventArgs(player, id, speedZone));

        return id;
    }

    [UIEvent(Events.SceneManagement.GetAllSpeedZones)]
    private void GetAllSpeedZonesUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var speedZonesWithPlayers = this._speedZones.Select(a => new SpeedZoneWithPlayerDTO
        {
            Id = a.Key.ToString(),
            SpeedZone = a.Value.Zone,
            PlayerName = a.Value.Player.GetUnderlyingPlayer().Name,
        }).ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, speedZonesWithPlayers);
    }

    [UIEvent(Events.SceneManagement.DeleteUI)]
    private void DeleteUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<StringIdRequest>(serializedRequest);
        if (!Guid.TryParse(request.Id, out var speedZoneId))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        if (!this.DeleteSpeedZone(speedZoneId))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "a", "b");
    }

    private sealed record SpeedZoneWithPlayer(SpeedZone Zone, FPlayer Player);
}