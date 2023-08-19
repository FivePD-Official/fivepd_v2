using System;
using System.Collections.Generic;
using System.Diagnostics;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Interfaces;

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
    /// Gets the active callouts.
    /// </summary>
    public List<CalloutHolder> ActiveCallouts { get; }
}

internal sealed record CalloutHolder(Callout Callout, Type Type, Stopwatch Timer);