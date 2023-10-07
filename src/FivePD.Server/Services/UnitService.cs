using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class UnitService : IUnitService
{
    private readonly INotificationService _notificationService;
    private readonly IConfirmationService _confirmationService;
    private readonly IConfigService _configService;

    private readonly List<Unit> _units = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitService"/> class.
    /// </summary>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    /// <param name="confirmationService">The <see cref="IConfirmationService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    public UnitService(INotificationService notificationService, IConfirmationService confirmationService, IConfigService configService)
    {
        this._notificationService = notificationService;
        this._confirmationService = confirmationService;
        this._configService = configService;

        #pragma warning disable
        // TEST DISPATCH
        // var unit = new Unit("U-10");
        // unit.Availability.Value = FUnitAvailability.Receiving;
        // unit.Status.Value = "10-8";
        // this._units.Add(unit);
        #pragma warning restore
    }

    /// <inheritdoc/>
    public event EventHandler<Unit> OnUnitCreated;

    /// <inheritdoc/>
    public event EventHandler<Unit> OnUnitRemoved;

    /// <inheritdoc/>
    public event EventHandler<UnitPlayerEventArgs> OnPlayerJoinedUnit;

    /// <inheritdoc/>
    public event EventHandler<UnitPlayerEventArgs> OnPlayerLeftUnit;

    /// <inheritdoc />
    public ReadOnlyCollection<Unit> Units()
    {
        return this._units.AsReadOnly();
    }

    /// <inheritdoc/>
    Unit IUnitService.CreateUnit(string callsign, FPlayer player)
    {
        if (this._units.FirstOrDefault(x => x.Callsign == callsign) is not null)
        {
            return null;
        }

        var unit = new Unit(callsign, player);
        unit.Availability.Value = FUnitAvailability.Receiving;
        this._units.Add(unit);

        player.UnitCallsign.Value = callsign;

        this.OnUnitCreated?.Invoke(this, unit);

        return unit;
    }

    /// <inheritdoc/>
    Unit IUnitService.CreateUnit(string callsign, IEnumerable<FPlayer> players)
    {
        if (this._units.FirstOrDefault(x => x.Callsign == callsign) is not null)
        {
            return null;
        }

        var fPlayers = players.ToList();

        var unit = new Unit(callsign, fPlayers);
        unit.Availability.Value = FUnitAvailability.Receiving;
        this._units.Add(unit);

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
        return this._units.FirstOrDefault(x => x.Players.Any(y => y.NetId == player.NetId));
    }

    /// <inheritdoc />
    public void RemovePlayerFromUnit(FPlayer player)
    {
        var unit = this.GetPlayerUnit(player);
        if (unit is null)
        {
            return;
        }

        ((IInternalUnit)unit).RemovePlayer(player);
        this.OnPlayerLeftUnit?.Invoke(this, new UnitPlayerEventArgs
        {
            Player = player,
            Unit = unit,
        });

        if (unit.Players.Count == 0)
        {
            this._units.Remove(unit);
            this.OnUnitRemoved?.Invoke(this, unit);
        }
    }

    /// <inheritdoc />
    public void AddPlayerToUnit(FPlayer player, Unit unit)
    {
        if (this._configService.GeneralConfig.Unit.MaxMembers > 0 && unit.Players.Count >= this._configService.GeneralConfig.Unit.MaxMembers)
        {
            return;
        }

        ((IInternalUnit)unit)?.AddPlayer(player);
        this.OnPlayerJoinedUnit?.Invoke(this, new UnitPlayerEventArgs
        {
            Player = player,
            Unit = unit,
        });
    }

    [UIEvent(Events.UI.Unit.GetAll)]
    private void GetAllUnitsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var units = this._units
            .Select(unit => new UnitDTO(unit.Callsign, unit.Players.Select(p => new SimplePlayerDTO(p.NetId, p.GetUnderlyingPlayer().Name))))
            .ToList();
        var page = new Page<UnitDTO>
        {
            Items = units,
            CurrentPage = 1,
            ItemCount = units.Count,
            PageCount = 1,
            PageSize = units.Count,
        };

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.Unit.Join, true)]
    private void JoinUnitUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<UnitCallsignRequest>();
        var unit = this._units.FirstOrDefault(x => x.Callsign == request.Callsign);
        if (unit is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        if (this._configService.GeneralConfig.Unit.MaxMembers > 0 && unit.Players.Count >= this._configService.GeneralConfig.Unit.MaxMembers)
        {
            networkCallbackDelegate.InvokeWithResponse(
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

        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.JOIN", "UI.LABEL.UNIT.SUCCESS.PLAYER.JOIN", playerJoinDictionary);
        this._notificationService.ShowNotification(fPlayer, "UI.LABEL.UNIT.SUCCESS.PLAYER.JOIN", playerJoinDictionary);
        this._notificationService.ShowNotification(
            unit.Players.Where(p => p.NetId != player.Handle),
            "UI.LABEL.UNIT.SUCCESS.NOT_PLAYER.JOIN",
            new Dictionary<string, string>
            {
                ["name"] = fPlayer.GetUnderlyingPlayer().Name,
            });
    }

    [UIEvent(Events.UI.Unit.RequestToJoin, true)]
    private void RequestToJoinUnitUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<UnitCallsignRequest>();
        var unit = this._units.FirstOrDefault(x => x.Callsign == request.Callsign);
        if (unit is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        if (this._configService.GeneralConfig.Unit.MaxMembers > 0 && unit.Players.Count >= this._configService.GeneralConfig.Unit.MaxMembers)
        {
            networkCallbackDelegate.InvokeWithResponse(
                false,
                "UI.LABEL.UNIT.ERROR.JOIN.MAX_MEMBERS.TITLE",
                "UI.LABEL.UNIT.ERROR.JOIN.MAX_MEMBERS.DESCRIPTION");
            return;
        }

        foreach (var unitPlayer in unit.Players.Where(p => p.NetId != player.Handle))
        {
            this._confirmationService.TriggerConfirmation(
                unitPlayer.GetUnderlyingPlayer(),
                new ConfirmationProperties
                {
                    Notification = new ConfirmationProperties.NotificationProperties
                    {
                        Content = "UNIT.REQUEST_TO_JOIN.NOT_PLAYER.NOTIFICATION",
                        ContentParams = new Dictionary<string, string>
                        {
                            ["name"] = player.Name,
                        },
                    },
                },
                response =>
                {
                    if (!response)
                    {
                        return;
                    }

                    var fPlayer = FPlayer.FromNetId(player.Handle);
                    this.AddPlayerToUnit(fPlayer, unit);

                    var playerJoinDictionary = new Dictionary<string, string>
                    {
                        ["callsign"] = unit.Callsign,
                    };
                    this._notificationService.ShowNotification(fPlayer, "UI.LABEL.UNIT.SUCCESS.PLAYER.JOIN", playerJoinDictionary);
                    this._notificationService.ShowNotification(
                        unit.Players.Where(p => p.NetId != player.Handle),
                        "UI.LABEL.UNIT.SUCCESS.NOT_PLAYER.JOIN",
                        new Dictionary<string, string>
                        {
                            ["name"] = fPlayer.GetUnderlyingPlayer().Name,
                        });
                });
        }
    }

    [UIEvent(Events.UI.Unit.Leave, true)]
    private void LeaveUnitUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<UnitCallsignRequest>();
        var unit = this._units.FirstOrDefault(x => x.Callsign == request.Callsign);
        if (unit is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var fPlayer = FPlayer.FromNetId(player.Handle);
        this.RemovePlayerFromUnit(fPlayer);

        var playerLeaveDictionary = new Dictionary<string, string>
        {
            ["callsign"] = unit.Callsign,
        };

        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.LEAVE", "UI.LABEL.UNIT.SUCCESS.PLAYER.LEAVE", playerLeaveDictionary);
        this._notificationService.ShowNotification(fPlayer, "UI.LABEL.UNIT.SUCCESS.PLAYER.LEAVE", playerLeaveDictionary);
        this._notificationService.ShowNotification(
            unit.Players.Where(p => p.NetId != player.Handle),
            "UI.LABEL.UNIT.SUCCESS.NOT_PLAYER.LEAVE",
            new Dictionary<string, string>
            {
                ["name"] = fPlayer.GetUnderlyingPlayer().Name,
            });
    }
}