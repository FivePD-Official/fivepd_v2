using System;
using System.Collections.Generic;
using FivePD.Common.Builders;
using FivePD.Common.ConfigModels;
using FivePD.Common.Enums;

namespace FivePD.Common.Models;

/// <summary>
/// The base ped class.
/// Provides a wrapper around game functionality for easier interactions with the framework.
/// </summary>
public class CommonFPed : CommonFEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFPed"/> class.
    /// </summary>
    /// <param name="builder">The builder object that'll be used to initialize this instance.</param>
    public CommonFPed(CommonFPedBuilder builder)
        : base(builder.NetworkId)
    {
        this.Gender = builder.Gender;
        this.Firstname = builder.Firstname;
        this.Lastname = builder.Lastname;
        this.Birthdate = builder.Birthdate;
        this.Items = builder.Items;
        this.Warrants = builder.Warrants;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFPed"/> class
    /// from an existing Cfx handle.
    /// </summary>
    /// <param name="networkId">The entity's network ID.</param>
    public CommonFPed(int networkId)
        : base(networkId)
    {
    }

    /// <summary>
    /// Gets the ped's history ID.
    /// </summary>
    public int PedHistoryId { get; internal set; }

    /// <summary>
    /// Gets the ped's gender.
    /// </summary>
    public FGender Gender { get; internal set; }

    /// <summary>
    /// Gets the ped's firstname.
    /// </summary>
    public string Firstname { get; }

    /// <summary>
    /// Gets the ped's lastname.
    /// </summary>
    public string Lastname { get; }

    /// <summary>
    /// Gets the ped's birthdate.
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

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Ped - network id: {this.NetworkId}, name: {this.Firstname} {this.Lastname}, birthdate: {this.Birthdate}";
    }
}