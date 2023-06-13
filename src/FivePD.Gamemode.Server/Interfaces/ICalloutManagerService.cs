using System;
using System.Collections.Generic;
using System.Diagnostics;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.EntityWrappers;

namespace FivePD.Gamemode.Server.Interfaces;

#pragma warning disable

public interface ICalloutManagerService
{
    public List<Callout> ActiveCallouts { get; }
    
    public Callout GetPlayerCallout(FPlayer player);

    public void CompleteCalloutOfPlayer(FPlayer player);
}

internal interface IInternalCalloutManagerService
{
    public List<Type> CalloutTypes { get; }
    public List<CalloutHolder> ActiveCallouts { get; }
}

internal sealed record CalloutHolder(Callout Callout, Stopwatch Timer);