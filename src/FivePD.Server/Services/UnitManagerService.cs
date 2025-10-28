using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IUnitManagerService" />
[Obfuscate]
public class UnitManagerService : IUnitManagerService, IInternalUnitManagerService, IOnInit
{
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IConfirmationService _confirmationService;
    private readonly IConfigService _configService;
    private readonly IInternalUnitService _unitService;
    private readonly IPlayerDataService _playerDataService;
    private readonly IPlayerHandlerService _playerHandlerService;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitManagerService"/> class.
    /// </summary>
    /// <param name="gameNotificationService">The <see cref="IGameNotificationService"/> to use.</param>
    /// <param name="confirmationService">The <see cref="IConfirmationService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="unitService">The <see cref="IUnitService"/> to use.</param>
    /// <param name="playerDataService">The <see cref="IPlayerDataService"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="IPlayerHandlerService"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public UnitManagerService(
        IGameNotificationService gameNotificationService,
        IConfirmationService confirmationService,
        IConfigService configService,
        IUnitService unitService,
        IPlayerDataService playerDataService,
        IPlayerHandlerService playerHandlerService,
        INetworkEventService networkEventService)
    {
        this._gameNotificationService = gameNotificationService;
        this._confirmationService = confirmationService;
        this._configService = configService;
        this._unitService = (IInternalUnitService)unitService;
        this._playerDataService = playerDataService;
        this._playerHandlerService = playerHandlerService;
        this._networkEventService = networkEventService;

#pragma warning disable
        // TEST DISPATCH
        // var unit = new Unit("U-10");
        // unit.Availability.Value = FUnitAvailability.Receiving;
        // unit.Status.Value = "10-8";
        // this._units.Add(unit);
    }

    /// <inheritdoc/>
    public event EventHandler<Unit> OnUnitCreated;

    /// <inheritdoc/>
    public event EventHandler<Unit> OnUnitRemoved;

    /// <inheritdoc/>
    public event EventHandler<UnitPlayerEventArgs> OnPlayerJoinedUnit;

    /// <inheritdoc/>
    public event EventHandler<UnitPlayerEventArgs> OnPlayerLeftUnit;

    /// <inheritdoc/>
    public event EventHandler<Unit> OnPanicModeEnabled;

    /// <inheritdoc/>
    public event EventHandler<Unit> OnPanicModeDisabled;

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._playerDataService.PlayerDutyStatusChanged += (_, args) =>
        {
            var playerDetails = this._playerHandlerService.FindUserOfPlayer(args.Player.GetUnderlyingPlayer());
            if (args.Status == FDutyStatus.OffDuty)
            {
                this.RemovePlayerFromUnit(args.Player);
            }
            else
            {
                ((IInternalUnitManagerService)this).CreateUnit(string.Join(string.Empty, playerDetails.Callsign), args.Player);
            }
        };
    }

    /// <inheritdoc/>
    Unit IInternalUnitManagerService.CreateUnit(string callsign, FPlayer player)
    {
        if (this._unitService.Units.FirstOrDefault(x => x.Callsign == callsign) is not null)
        {
            return null;
        }

        var unit = new Unit(callsign, player);
        unit.Availability.Value = FUnitAvailability.Receiving;
        this._unitService.Units.Add(unit);

        player.UnitCallsign.Value = callsign;

        this.OnUnitCreated?.Invoke(this, unit);

        return unit;
    }

    /// <inheritdoc/>
    Unit IInternalUnitManagerService.CreateUnit(string callsign, IEnumerable<FPlayer> players)
    {
        if (this._unitService.Units.FirstOrDefault(x => x.Callsign == callsign) is not null)
        {
            return null;
        }

        var fPlayers = players.ToList();

        var unit = new Unit(callsign, fPlayers);
        unit.Availability.Value = FUnitAvailability.Receiving;
        this._unitService.Units.Add(unit);

        foreach (var player in fPlayers)
        {
            player.UnitCallsign.Value = callsign;
        }

        this.OnUnitCreated?.Invoke(this, unit);

        return unit;
    }

    /// <inheritdoc />
    public Unit GetPlayerUnit(FPlayer player)
    {
        return this._unitService.Units.FirstOrDefault(x => x.Players.Any(y => y.NetId == player.NetId));
    }

    /// <inheritdoc />
    public void RemovePlayerFromUnit(FPlayer player)
    {
        var unit = this.GetPlayerUnit(player);
        if (unit is null)
        {
            return;
        }

        unit.RemovePlayer(player);
        this.OnPlayerLeftUnit?.Invoke(this, new UnitPlayerEventArgs
        {
            Player = player,
            Unit = unit,
        });

        player.GetUnderlyingPlayer().TriggerEvent(Events.Incident.OnCompleted);

        if (unit.Players.Count == 0)
        {
            this._unitService.Units.Remove(unit);
            this.OnUnitRemoved?.Invoke(this, unit);
        }
    }

    /// <inheritdoc />
    public void EnablePanicMode(Unit unit)
    {
        if (unit.PanicMode.Value)
        {
            return;
        }

        unit.PanicMode.Value = true;
        this.OnPanicModeEnabled?.Invoke(this, unit);
    }

    /// <inheritdoc />
    public void DisablePanicMode(Unit unit)
    {
        if (!unit.PanicMode.Value)
        {
            return;
        }

        unit.PanicMode.Value = false;
        this.OnPanicModeDisabled?.Invoke(this, unit);
    }

    /// <inheritdoc />
    public void AddPlayerToUnit(FPlayer player, Unit unit)
    {
        if (this._configService.GeneralConfig.Unit.MaxMembers > 0 && unit.Players.Count >= this._configService.GeneralConfig.Unit.MaxMembers)
        {
            return;
        }

        unit.AddPlayer(player);
        this.OnPlayerJoinedUnit?.Invoke(this, new UnitPlayerEventArgs
        {
            Player = player,
            Unit = unit,
        });
    }

    [UIEvent(Events.UI.Unit.GetAll)]
    private void GetAllUnitsUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var units = this._unitService.Units
            .Select(unit => new UnitDTO(unit.Callsign, unit.TrafficStop is not null || unit.Callout is not null,
                unit.Players.Select(p => new SimplePlayerDTO(p.NetId, p.GetUnderlyingPlayer().Name))))
            .ToList();
        var page = new Page<UnitDTO>
        {
            Items = units,
            CurrentPage = 1,
            ItemCount = units.Count,
            PageCount = 1,
            PageSize = units.Count,
        };
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Unit.Join)]
    private void JoinUnitUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<UnitCallsignRequest>(serializedRequest);
        var unit = this._unitService.Units.FirstOrDefault(x => x.Callsign == request.Callsign);
        if (unit is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        if (this._configService.GeneralConfig.Unit.MaxMembers > 0 && unit.Players.Count >= this._configService.GeneralConfig.Unit.MaxMembers)
        {
            this._networkEventService.TriggerEventWithResponse(
                player, 
                guid,
                false,
                "UI.LABEL.UNIT.ERROR.JOIN.MAX_MEMBERS.TITLE",
                "UI.LABEL.UNIT.ERROR.JOIN.MAX_MEMBERS.DESCRIPTION");
            return;
        }

        var fPlayer = FPlayer.FromNetId(player.Handle);
        this.AddPlayerToUnit(fPlayer, unit);

        var playerJoinDictionary = new Dictionary<string, string>
        {
            ["callsign"] = unit.Callsign,
        };

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "UI.LABEL.SUCCESS.JOIN", "UI.LABEL.UNIT.SUCCESS.PLAYER.JOIN", playerJoinDictionary);
        this._gameNotificationService.Notify(
            fPlayer,
            new GameNotificationOptions
            {
                Title = "FivePD",
                Message = "UI.LABEL.UNIT.SUCCESS.PLAYER.JOIN",
                MessageParams = playerJoinDictionary,
            });
        foreach (var p in unit.Players.Where(p => p.NetId != player.Handle))
        {
            this._gameNotificationService.Notify(
                p,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "UI.LABEL.UNIT.SUCCESS.NOT_PLAYER.JOIN",
                    MessageParams = new Dictionary<string, string>
                    {
                        ["name"] = fPlayer.GetUnderlyingPlayer().Name,
                    }
                });
        }
    }

    [UIEvent(Events.UI.Unit.RequestToJoin)]
    private void RequestToJoinUnitUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request =  JsonConvert.DeserializeObject<UnitCallsignRequest>(serializedRequest);
        var unit = this._unitService.Units.FirstOrDefault(x => x.Callsign == request.Callsign);
        if (unit is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        if (this._configService.GeneralConfig.Unit.MaxMembers > 0 && unit.Players.Count >= this._configService.GeneralConfig.Unit.MaxMembers)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "UI.LABEL.UNIT.ERROR.JOIN.MAX_MEMBERS.TITLE",
                "UI.LABEL.UNIT.ERROR.JOIN.MAX_MEMBERS.DESCRIPTION");
            return;
        }

        foreach (var unitPlayer in unit.Players.Where(p => p.NetId != player.Handle))
        {
            this._confirmationService.TriggerConfirmation(
                unitPlayer,
                new ConfirmationProperties
                {
                    Title = "FivePD",
                    Message = "UNIT.REQUEST_TO_JOIN.NOT_PLAYER.NOTIFICATION",
                    MessageParams = new Dictionary<string, string>
                    {
                        ["name"] = player.Name,
                    },
                    Keys = new Dictionary<string, string>
                    {
                        { "F1", "ACTION.ACCEPT.TITLE" },
                    },
                    PlayAudio = this._configService.GeneralConfig.Audio.Files.UnitJoinRequest,
                },
                response =>
                {
                    if (!response)
                    {
                        return;
                    }

                    var fPlayer = FPlayer.FromNetId(player.Handle);
                    this.AddPlayerToUnit(fPlayer, unit);

                    this._gameNotificationService.Notify(
                        fPlayer,
                        new GameNotificationOptions
                        {
                            Title = "FivePD",
                            Message = "UI.LABEL.UNIT.SUCCESS.PLAYER.JOIN",
                            MessageParams = new Dictionary<string, string>
                            {
                                ["callsign"] = unit.Callsign,
                            },
                        });
                    foreach (var p in unit.Players.Where(p => p.NetId != player.Handle))
                    {
                        this._gameNotificationService.Notify(
                            p,
                            new GameNotificationOptions
                            {
                                Title = "FivePD",
                                Message = "UI.LABEL.UNIT.SUCCESS.NOT_PLAYER.JOIN",
                                MessageParams = new Dictionary<string, string>
                                {
                                    ["name"] = fPlayer.GetUnderlyingPlayer().Name,
                                }
                            });
                    }
                });
        }
    }

    [UIEvent(Events.UI.Unit.Leave)]
    private void LeaveUnitUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<UnitCallsignRequest>(serializedRequest);
        var unit = this._unitService.Units.FirstOrDefault(x => x.Callsign == request.Callsign);
        if (unit is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var fPlayer = FPlayer.FromNetId(player.Handle);
        this.RemovePlayerFromUnit(fPlayer);

        var playerLeaveDictionary = new Dictionary<string, string>
        {
            ["callsign"] = unit.Callsign,
        };

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "UI.LABEL.SUCCESS.LEAVE", "UI.LABEL.UNIT.SUCCESS.PLAYER.LEAVE", playerLeaveDictionary);
        this._gameNotificationService.Notify(
            fPlayer,
            new GameNotificationOptions
            {
                Title = "FivePD",
                Message = "UI.LABEL.UNIT.SUCCESS.PLAYER.LEAVE",
                MessageParams = playerLeaveDictionary,
            });
        foreach (var p in unit.Players.Where(p => p.NetId != player.Handle))
        {
            this._gameNotificationService.Notify(
                p,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "UI.LABEL.UNIT.SUCCESS.NOT_PLAYER.LEAVE",
                    MessageParams = new Dictionary<string, string>
                    {
                        ["name"] = fPlayer.GetUnderlyingPlayer().Name,
                    }
                });
        }
    }
}