using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Common.Models;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Models;

/// <summary>
/// The base player class for the gamemode.
/// Provides a wrapper around game functionality for easier interactions with the framework.
/// </summary>
public class FPlayer : FStatefulContainer
{
    private static readonly List<FPlayer> CachedPlayers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FPlayer"/> class.
    /// </summary>
    /// <param name="netId">The network identifier that corresponds to the player.</param>
    public FPlayer(string netId)
    {
        this.NetId = netId;

        // Cache the player
        CachedPlayers.Add(this);

        // Sets up duty status tracking
        this.DutyStatus = new FStateBagObservable<FDutyStatus>(
            this,
            "FivePD:DutyStatus",
            FDutyStatus.OffDuty);
    }

    /// <summary>
    /// Gets the player's network identifier.
    /// </summary>
    public string NetId { get; private set; }

    /// <summary>
    /// Gets the player's duty status observable.
    /// </summary>
    public FStateBagObservable<FDutyStatus> DutyStatus { get; private set; }

    /// <summary>
    /// Attempts to get the player with the specified netId.
    /// </summary>
    /// <param name="netId">The Cfx network identifier.</param>
    /// <returns>The FPlayer.</returns>
    public static FPlayer FromNetId(string netId)
    {
        try
        {
            return CachedPlayers.First(player => player.NetId == netId);
        }
        catch (Exception)
        {
            return new FPlayer(netId);
        }
    }

    /// <summary>
    /// Attempts to get the player with the specified netId.
    /// </summary>
    /// <param name="netId">The Cfx network identifier.</param>
    /// <returns>The FPlayer.</returns>
    public static FPlayer FromNetId(int netId)
    {
        return FromNetId(netId.ToString());
    }

    /// <summary>
    /// Gets the underlying Cfx player.
    /// </summary>
    /// <returns>A Cfx player.</returns>
    public Player GetUnderlyingPlayer()
    {
        return new Player(Cfx.API.GetPlayerFromServerId(int.Parse(this.NetId)));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.NetId;
    }

    /// <inheritdoc />
    public override string GetStateKey()
    {
        return $"player:{this.NetId}";
    }

    /// <inheritdoc />
    public override StateBag GetStateBag()
    {
        return this.GetUnderlyingPlayer().State;
    }
}