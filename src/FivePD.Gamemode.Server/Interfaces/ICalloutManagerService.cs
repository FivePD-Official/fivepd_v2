using System.Collections.Generic;
using FivePD.Gamemode.Server.API;

namespace FivePD.Gamemode.Server.Interfaces;

#pragma warning disable

public interface ICalloutManagerService
{
    public List<Callout> ActiveCallouts { get; }
}