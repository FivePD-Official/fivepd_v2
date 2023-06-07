using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.DTO;
using FivePD.Gamemode.Server.Models.Requests;
using FivePD.Gamemode.Server.QueryHelpers;

namespace FivePD.Gamemode.Server.Services;

#pragma warning disable

public class UnitService : IUnitService
{
    private readonly INotificationService _notificationService;
    private readonly IConfirmationService _confirmationService;
    private readonly List<Unit> _units = new();

    public ReadOnlyCollection<Unit> Units => this._units.AsReadOnly();

    public UnitService(INotificationService notificationService, IConfirmationService confirmationService)
    {
        this._notificationService = notificationService;
        this._confirmationService = confirmationService;
    }

    public Unit CreateUnit(string callsign, FPlayer player)
    {
        if (this._units.FirstOrDefault(x => x.Callsign == callsign) is not null)
        {
            return null;
        }

        var unit = new Unit(callsign, player);
        this._units.Add(unit);

        return unit;
    }

    public Unit CreateUnit(string callsign, IEnumerable<FPlayer> players)
    {
        if (this._units.FirstOrDefault(x => x.Callsign == callsign) is not null)
        {
            return null;
        }

        var unit = new Unit(callsign, players);
        this._units.Add(unit);

        return unit;
    }

    public Unit GetPlayerUnit(FPlayer player)
    {
        return this._units.FirstOrDefault(x => x.Players.Any(y => y.NetId == player.NetId));
    }

    public void RemovePlayerFromUnit(FPlayer player)
    {
        var unit = this.GetPlayerUnit(player);
        unit?.RemovePlayer(player);

        if (unit?.Players.Count == 0)
        {
            this._units.Remove(unit);
        }
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

        var fPlayer = FPlayer.FromNetId(player.Handle);
        unit.AddPlayer(fPlayer);

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

        foreach (var unitPlayer in unit.Players.Where(p => p.NetId != player.Handle))
        {
            this._confirmationService.TriggerConfirmation(
                unitPlayer.GetUnderlyingPlayer(),
                288, // F1
                "UNIT.REQUEST_TO_JOIN.NOT_PLAYER.TOOLTIP",
                "UNIT.REQUEST_TO_JOIN.NOT_PLAYER.NOTIFICATION",
                new Dictionary<string, string>
                {
                    ["name"] = player.Name,
                },
                response =>
                {
                    if (!response)
                    {
                        return;
                    }

                    var fPlayer = FPlayer.FromNetId(player.Handle);
                    unit.AddPlayer(fPlayer);

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
        unit.RemovePlayer(fPlayer);

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