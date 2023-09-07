using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common.Builders;
using FivePD.Common.ConfigModels;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.Models;

namespace FivePD.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Represents a wrapper around a <see cref="CommonFPed"/> to provide additional functionality.
/// </summary>
public sealed class FPed : FEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPed">The FPed to use.</param>
    public FPed(CommonFPed fPed)
    {
        this.Properties = new(this);
        this.Task = new(this);
        this.Weapons = new(this);

        this.NetworkId = fPed.NetworkId;
        this.Birthdate = fPed.Birthdate;
        this.Firstname = fPed.Firstname;
        this.Lastname = fPed.Lastname;
        this.Gender = fPed.Gender;
        this.Items = fPed.Items;
        this.Warrants = fPed.Warrants;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPedBuilder">The FPedBuilder to use.</param>
    public FPed(FPedBuilder fPedBuilder)
    {
        this.Licenses = fPedBuilder.Licenses;
        this.Properties = new(this);
        this.Task = new(this);
        this.Weapons = new(this);

        this.NetworkId = fPedBuilder.NetworkId;
        this.Birthdate = fPedBuilder.Birthdate;
        this.Firstname = fPedBuilder.Firstname;
        this.Lastname = fPedBuilder.Lastname;
        this.Gender = fPedBuilder.Gender;
        this.Items = fPedBuilder.Items;
        this.Warrants = fPedBuilder.Warrants;
        this.Licenses = fPedBuilder.Licenses;
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
    /// Gets the underlying <see cref="Ped"/>.
    /// </summary>
    public Ped GetUnderlyingPed => Entity.FromNetworkId(this.NetworkId) as Ped;

    /// <summary>
    /// Gets the owner of the <see cref="FPed"/>.
    /// </summary>
    public Player Owner => this.GetUnderlyingPed.Owner;

    /// <summary>
    /// Gets the entity's network identifier.
    /// </summary>
    public int NetworkId { get; }

    /// <summary>
    /// Gets the gender of the ped.
    /// </summary>
    public FGender Gender { get; internal set; }

    /// <summary>
    /// Gets the firstname of the ped.
    /// </summary>
    public string Firstname { get; }

    /// <summary>
    /// Gets the lastname of the ped.
    /// </summary>
    public string Lastname { get; }

    /// <summary>
    /// Gets the birthdate of the ped.
    /// </summary>
    public DateTime Birthdate { get; }

    /// <summary>
    /// Gets the items of the ped.
    /// </summary>
    public List<Item> Items { get; }

    /// <summary>
    /// Gets the warrants of the ped.
    /// </summary>
    public List<string> Warrants { get; }

    /// <summary>
    /// Gets the licenses of the ped.
    /// </summary>
    public List<License> Licenses { get; }

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

    /// <summary>
    /// Created an <see cref="FPed"/> wrapper from the specified <see cref="Ped"/>.
    /// </summary>
    /// <param name="networkId">The network ID of the ped.</param>
    /// <returns>The FPed.</returns>
    public static FPed FromNetId(int networkId)
    {
        return new FPed(new CommonFPed(new CommonFPedBuilder(networkId)));
    }

    /// <summary>
    /// Checks if the ped has a license.
    /// </summary>
    /// <param name="licenseType">The license type to check.</param>
    /// <returns>True if the ped has the license, false otherwise.</returns>
    public bool HasLicense(LicenseType licenseType) => this.Licenses.Exists(x => x.Type == licenseType);

    /// <summary>
    /// Gets the license of the specified type.
    /// </summary>
    /// <param name="licenseType">The license type to get.</param>
    /// <returns>The license.</returns>
    public License GetLicense(LicenseType licenseType) => this.Licenses.FirstOrDefault(x => x.Type == licenseType);
}