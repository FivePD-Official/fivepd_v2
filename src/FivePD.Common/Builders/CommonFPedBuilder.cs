using System;
using System.Collections.Generic;
using FivePD.Common.ConfigModels;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using Newtonsoft.Json;

namespace FivePD.Common.Builders;

/// <summary>
/// Used for the construction of <see cref="CommonFPed"/> objects.
/// </summary>
public class CommonFPedBuilder
{
    /// <summary>
    /// The random instance.
    /// </summary>
    protected static readonly Random Random = new(Guid.NewGuid().GetHashCode());

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFPedBuilder"/> class.
    /// </summary>
    /// <param name="networkId">The network identifier of the ped.</param>
    public CommonFPedBuilder(int networkId)
    {
        this.NetworkId = networkId;
    }

    /// <summary>
    /// Gets or sets the network identifier of the ped.
    /// </summary>
    public int NetworkId { get; protected set; }

    /// <summary>
    /// Gets or sets the gender of the ped.
    /// </summary>
    [JsonProperty]
    public FGender Gender { get; protected set; }

    /// <summary>
    /// Gets or sets the first name of the ped.
    /// </summary>
    public string Firstname { get; protected set; }

    /// <summary>
    /// Gets or sets the last name of the ped.
    /// </summary>
    public string Lastname { get; protected set; }

    /// <summary>
    /// Gets or sets the birthdate of the ped.
    /// </summary>
    public DateTime Birthdate { get; protected set; }

    /// <summary>
    /// Gets or sets the items of the ped.
    /// </summary>
    public List<Item> Items { get; protected set; } = new();

    /// <summary>
    /// Gets or sets the warrants of the ped.
    /// </summary>
    public List<string> Warrants { get; protected set; } = new();

    /// <summary>
    /// Sets the gender of the ped.
    /// </summary>
    /// <param name="gender">The gender to set.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public CommonFPedBuilder WithGender(FGender gender)
    {
        this.Gender = gender;
        return this;
    }
}