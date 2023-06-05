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
using FivePD.Gamemode.Server.QueryHelpers;

namespace FivePD.Gamemode.Server.Services;

#pragma warning disable

public class UnitService : IUnitService
{
    private readonly List<Unit> _units = new();

    public ReadOnlyCollection<Unit> Units => this._units.AsReadOnly();

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
}