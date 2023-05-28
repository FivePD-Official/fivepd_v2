using CitizenFX.Core;
using FivePD.Common.Builders;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Represents a wrapper around a <see cref="CommonFPed"/> to provide additional functionality.
/// </summary>
public sealed class FPed : FEntity
{
    private readonly CommonFPed _fPed;
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPed">The FPed to use.</param>
    /// <param name="eventService">The <see cref="IEventService"/> to use.</param>
    public FPed(CommonFPed fPed, IEventService eventService)
    {
        this._fPed = fPed;
        this._eventService = eventService;
        this.Properties = new(this, eventService);
        this.Task = new(this, eventService);
        this.Weapons = new(this, eventService);
    }

    /// <summary>
    /// Gets the properties of the <see cref="FPed"/>.
    /// </summary>
    public FPedProperties Properties { get; private set; }

    /// <summary>
    /// Gets the tasks of the <see cref="FPed"/>.
    /// </summary>
    public FPedTasks Task { get; private set; }

    /// <summary>
    /// Gets the weapons of the <see cref="FPed"/>.
    /// </summary>
    public FWeaponCollection Weapons { get; private set; }

    /// <summary>
    /// Gets the underlying <see cref="CommonFPed"/>.
    /// </summary>
    public CommonFPed GetFPed => this._fPed;

    /// <summary>
    /// Gets the underlying <see cref="Ped"/>.
    /// </summary>
    public Ped GetUnderlyingPed => (Ped)Entity.FromNetworkId(this._fPed.NetworkId);

    /// <summary>
    /// Gets the owner of the <see cref="FPed"/>.
    /// </summary>
    public Player Owner => this.GetUnderlyingPed.Owner;

#pragma warning disable
    // TODO - PLS REFACTOR ME
    // this is NOT good practice, but it's the only way to get the FPed from the FPlayer at the moment
    // an example usage is when a menu item is selected and we want to set some properties on the player's ped: "DutyMenu.LoadoutItem.OnSelect"
    public static FPed FromFPlayer(FPlayer player, IEventService eventService)
    {
        return new FPed(new CommonFPed(new CommonFPedBuilder(player.GetUnderlyingPlayer().Character.NetworkId)), eventService);
    }
#pragma warning restore
}