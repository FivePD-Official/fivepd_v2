using System.Collections.Generic;
using System.Collections.ObjectModel;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.Interfaces;

#pragma warning disable

public interface IUnitService
{
    public ReadOnlyCollection<Unit> Units { get; }

    public Unit CreateUnit(string callsign, FPlayer player);

    public Unit CreateUnit(string callsign, IEnumerable<FPlayer> players);

    public Unit GetPlayerUnit(FPlayer player);

    public void RemovePlayerFromUnit(FPlayer player);
}