using CitizenFX.Core;
using FivePD.Common.Builders;
using FivePD.Common.Models;

namespace FivePD.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Represents a wrapper around a <see cref="CommonFPed"/> to provide additional functionality.
/// </summary>
public sealed class FPed : FEntity
{
    private readonly CommonFPed _fPed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPed">The FPed to use.</param>
    public FPed(CommonFPed fPed)
    {
        this._fPed = fPed;
        this.Properties = new(this);
        this.Task = new(this);
        this.Weapons = new(this);
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
    public Ped GetUnderlyingPed => Entity.FromNetworkId(this._fPed.NetworkId) as Ped;

    /// <summary>
    /// Gets the owner of the <see cref="FPed"/>.
    /// </summary>
    public Player Owner => this.GetUnderlyingPed.Owner;

    /// <summary>
    /// Created an <see cref="FPed"/> wrapper from the specified <see cref="FPlayer"/>.
    /// </summary>
    /// <param name="player">The <see cref="FPlayer"/> instance to use.</param>
    /// <returns>The FPed.</returns>
    public static FPed FromFPlayer(FPlayer player)
    {
        return new FPed(new CommonFPed(new CommonFPedBuilder(player.GetUnderlyingPlayer().Character.NetworkId)));
    }

    /// <summary>
    /// Created an <see cref="FPed"/> wrapper from the specified <see cref="Ped"/>.
    /// </summary>
    /// <param name="ped">The <see cref="Ped"/> instance to use.</param>
    /// <returns>The FPed.</returns>
    public static FPed FromPed(Ped ped)
    {
        return new FPed(new CommonFPed(new CommonFPedBuilder(ped.NetworkId)));
    }
}