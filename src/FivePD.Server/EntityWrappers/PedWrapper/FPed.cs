using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.ConfigModels;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Represents a wrapper around a <see cref="CommonFPed"/> to provide additional functionality.
/// </summary>
public class FPed : FEntity, IFStatefulContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPed">The FPed to use.</param>
    public FPed(CommonFPed fPed)
    {
        this.NetworkId = fPed.NetworkId;

        this.Birthdate = fPed.Birthdate;
        this.Firstname = fPed.Firstname;
        this.Lastname = fPed.Lastname;
        this.Gender = fPed.Gender;
        this.Items = fPed.Items;
        this.Warrants = fPed.Warrants;
        this.Licenses = new();
        this.AlcoholLevel = fPed.AlcoholLevel;
        this.Drugs = new();

        this.Create();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPedBuilder">The FPedBuilder to use.</param>
    public FPed(FPedBuilder fPedBuilder)
    {
        this.NetworkId = fPedBuilder.NetworkId;

        this.Birthdate = fPedBuilder.Birthdate;
        this.Firstname = fPedBuilder.Firstname;
        this.Lastname = fPedBuilder.Lastname;
        this.Gender = fPedBuilder.Gender;
        this.Items = fPedBuilder.Items;
        this.Warrants = fPedBuilder.Warrants;
        this.Licenses = fPedBuilder.Licenses;
        this.AlcoholLevel = fPedBuilder.AlcoholLevel;
        this.Drugs = fPedBuilder.Drugs;

        this.Create();
    }

    /// <summary>
    /// Gets the ped's observable with a value indicating if the ped is stopped.
    /// </summary>
    public FStateBagObservable<bool> Stopped { get; private set; }

    /// <summary>
    /// Gets the ped's observable with a value indicating if the ped has generated data.
    /// </summary>
    public FStateBagObservable<bool> HasGeneratedData { get; private set; }

    /// <summary>
    /// Gets the ped's observable with a value indicating if the ped is detained.
    /// </summary>
    public FStateBagObservable<bool> Detained { get; private set; }

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
    public List<PedLicense> Licenses { get; }

    /// <summary>
    /// Gets the alcohol level of the ped.
    /// </summary>
    public double AlcoholLevel { get; }

    /// <summary>
    /// Gets the drugs of the ped.
    /// </summary>
    public List<Drug> Drugs { get; }

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

    /// <inheritdoc />
    public string GetStateKey()
    {
        return $"ped:{this.GetUnderlyingPed.NetworkId}";
    }

    /// <inheritdoc />
    public StateBag GetStateBag()
    {
        return this.GetUnderlyingPed?.State;
    }

    /// <summary>
    /// Checks if the ped has a license.
    /// </summary>
    /// <param name="pedLicenseType">The license type to check.</param>
    /// <returns>True if the ped has the license, false otherwise.</returns>
    public bool HasLicense(PedLicenseType pedLicenseType) => this.Licenses.Exists(x => x.Type == pedLicenseType);

    /// <summary>
    /// Gets the license of the specified type.
    /// </summary>
    /// <param name="pedLicenseType">The license type to get.</param>
    /// <returns>The license.</returns>
    public PedLicense GetLicense(PedLicenseType pedLicenseType) => this.Licenses.FirstOrDefault(x => x.Type == pedLicenseType);

    /// <summary>
    /// Gets a value indicating whether the ped's alcohol level is over the legal limit.
    /// </summary>
    /// <returns>True if the ped's alcohol level is over the legal limit, false otherwise.</returns>
    public bool AlcoholOverLegalLimit() => this.AlcoholLevel > ServiceLocator.GetService<IConfigService>().GeneralConfig.Peds.Alcohol.LegalLimit;

#pragma warning disable
    private void Create()
    {
        this.Properties = new(this);
        this.Task = new(this);
        this.Weapons = new(this);

        this.Stopped = new FStateBagObservable<bool>(this, StateBagKeys.Ped.Stopped, false);
        this.HasGeneratedData = new FStateBagObservable<bool>(this, StateBagKeys.Ped.HasGeneratedData, false);
        this.Detained = new FStateBagObservable<bool>(this, StateBagKeys.Ped.Detained, false);
    }
#pragma warning restore
}