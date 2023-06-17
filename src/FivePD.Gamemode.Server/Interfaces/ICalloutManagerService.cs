using System;
using System.Collections.Generic;
using System.Diagnostics;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.EntityWrappers;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Responsible for managing callouts.
/// </summary>
public interface ICalloutManagerService
{
    /// <summary>
    /// Invoked when a callout is completed.
    /// </summary>
    public event EventHandler<Callout> OnCalloutCompleted;

    /// <summary>
    /// Gets the active callouts.
    /// </summary>
    /// <returns>A list of callouts.</returns>
    public List<Callout> ActiveCallouts();

    /// <summary>
    /// Gets the attached callout of the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The callout.</returns>
    public Callout GetPlayerCallout(FPlayer player);

    /// <summary>
    /// Completes the callout of the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// TODO: make this internal?
    public void CompleteCalloutOfPlayer(FPlayer player);
}

/// <summary>
/// Responsible for internal callout management.
/// </summary>
internal interface IInternalCalloutManagerService
{
    /// <summary>
    /// Gets all available callout types.
    /// </summary>
    public List<Type> CalloutTypes { get; }

    /// <summary>
    /// Gets the active callouts.
    /// </summary>
    public List<CalloutHolder> ActiveCallouts { get; }
}

internal sealed record CalloutHolder(Callout Callout, Stopwatch Timer);