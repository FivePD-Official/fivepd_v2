using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.EntityWrappers;

/// <summary>
/// The base player class.
/// Provides a wrapper around game functionality for easier interactions with the framework.
/// </summary>
public class FPlayer : FStatefulContainer, IFPlayerWithMenus
{
    private static readonly List<FPlayer> CachedPlayers = new();
    private List<Menu> _menus = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FPlayer"/> class.
    /// </summary>
    /// <param name="netId">The network identifier that corresponds to the player.</param>
    public FPlayer(string netId)
    {
        this.NetId = netId;

        // Cache the player
        CachedPlayers.Add(this);

        this.DutyStatus = new FStateBagObservable<FDutyStatus>(this, StateBagKeys.DutyStatus, FDutyStatus.OffDuty);
        this.Availability = new FStateBagObservable<FPlayerAvailability>(this, StateBagKeys.Availability, FPlayerAvailability.NotReceiving);
        this.Status = new FStateBagObservable<string>(this, StateBagKeys.Status, string.Empty);
    }

    /// <summary>
    /// Gets the player's network identifier.
    /// </summary>
    public string NetId { get; private set; }

    /// <summary>
    /// Gets the player's unit.
    /// </summary>
    /// <returns>A unit.</returns>
    public Unit Unit => ServiceLocator.GetService<IUnitService>().GetPlayerUnit(this);

    /// <summary>
    /// Gets the player's duty status observable.
    /// </summary>
    public FStateBagObservable<FDutyStatus> DutyStatus { get; private set; }

    /// <summary>
    /// Gets the player's call availability observable.
    /// </summary>
    public FStateBagObservable<FPlayerAvailability> Availability { get; private set; }

    /// <summary>
    /// Gets the player's status observable.
    /// </summary>
    public FStateBagObservable<string> Status { get; private set; }

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
    /// Attempts to get the player with the specified player's netId.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The FPlayer.</returns>
    public static FPlayer FromPlayer(Player player)
    {
        return FromNetId(player.Handle);
    }

    /// <summary>
    /// Gets the underlying Cfx player.
    /// </summary>
    /// <returns>A Cfx player.</returns>
    public Player GetUnderlyingPlayer()
    {
        return CreateInstance<Player>(this.NetId);
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

    /// <summary>
    /// Gets the menu with the type of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="type">The type of the menu2.</param>
    /// <typeparam name="T">The type of the menu.</typeparam>
    /// <returns>Returns a menu or null.</returns>
    public T GetMenu<T>(Type type = null)
        where T : Menu
    {
        type ??= typeof(T);
        return (T)this._menus.FirstOrDefault(m => m.GetType() == type);
    }

    /// <inheritdoc />
    public override StateBag GetStateBag()
    {
        return this.GetUnderlyingPlayer().State;
    }

    /// <inheritdoc/>
    void IFPlayerWithMenus.SetMenus(List<Menu> menus)
    {
        this._menus = menus;
    }

    /// <inheritdoc/>
    MenuItem IFPlayerWithMenus.FindMenuItem(string hashcode)
    {
        return this._menus
            .Select(m => m.Items.FirstOrDefault(x => x.GetHashCode().ToString() == hashcode))
            .FirstOrDefault(found => found != null);
    }

#pragma warning disable S3011

    // !! HACK ALERT !! : Cfx does not let us
    // manually create a player from
    // a network ID. This solution is
    // much faster than the alternative...
    // so it will be okay for now.
    private static T CreateInstance<T>(params object[] args)
    {
        var type = typeof(T);
        if (type.FullName == null)
        {
            throw new ArgumentException("Invalid type");
        }

        var instance = type.Assembly.CreateInstance(
            type.FullName,
            false,
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            args,
            null,
            null);

        return (T)instance;
    }
#pragma warning restore S3011
}