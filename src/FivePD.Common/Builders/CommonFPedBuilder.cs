using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Options for the <see cref="CommonFPedBuilder.WithWarrants(WithWarrantsOptions)"/> method.
    /// </summary>
    /// <param name="Warrants">The warrants to use.</param>
    /// <param name="IncludeAll">
    /// Specifies whether to include all warrants. It should be true only when ped history is loaded.
    /// Does not respect <see cref="ChanceForAnyWarrant"/> and <see cref="MaxWarrants"/>.
    /// </param>
    /// <param name="ChanceForAnyWarrant">The chance for any warrant to be included.</param>
    /// <param name="MaxWarrants">The maximum number of warrants to include.</param>
#pragma warning disable SA1313
    public readonly record struct WithWarrantsOptions(List<string> Warrants, bool IncludeAll, int ChanceForAnyWarrant, int MaxWarrants);
#pragma warning restore SA1313

    private static readonly Random Random = new(Guid.NewGuid().GetHashCode());

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFPedBuilder"/> class.
    /// </summary>
    /// <param name="networkId">The network identifier of the ped.</param>
    public CommonFPedBuilder(int networkId)
    {
        this.NetworkId = networkId;
    }

    /// <summary>
    /// Gets the network identifier of the ped.
    /// </summary>
    public int NetworkId { get; private set; }

    /// <summary>
    /// Gets the gender of the ped.
    /// </summary>
    [JsonProperty]
    public FGender Gender { get; private set; }

    /// <summary>
    /// Gets the first name of the ped.
    /// </summary>
    public string Firstname { get; private set; }

    /// <summary>
    /// Gets the last name of the ped.
    /// </summary>
    public string Lastname { get; private set; }

    /// <summary>
    /// Gets the birthdate of the ped.
    /// </summary>
    public DateTime Birthdate { get; private set; }

    /// <summary>
    /// Gets the items of the ped.
    /// </summary>
    public List<Item> Items { get; private set; }

    /// <summary>
    /// Gets the warrants of the ped.
    /// </summary>
    public List<string> Warrants { get; private set; }

    /// <summary>
    /// Sets the firstname of the ped.
    /// </summary>
    /// <param name="name">The name to set.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public CommonFPedBuilder WithFirstname(string name)
    {
        this.Firstname = name;
        return this;
    }

    /// <summary>
    /// Sets a random firstname for the ped.
    /// </summary>
    /// <param name="names">The names to choose from.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public CommonFPedBuilder WithFirstname(List<string> names)
    {
        this.Firstname = names[Random.Next(names.Count)].Trim();
        return this;
    }

    /// <summary>
    /// Sets the lastname of the ped.
    /// </summary>
    /// <param name="name">The name to set.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public CommonFPedBuilder WithLastname(string name)
    {
        this.Lastname = name;
        return this;
    }

    /// <summary>
    /// Sets a random lastname for the ped.
    /// </summary>
    /// <param name="names">The names to choose from.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public CommonFPedBuilder WithLastname(List<string> names)
    {
        this.Lastname = names[Random.Next(names.Count)].Trim();
        return this;
    }

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

    /// <summary>
    /// Sets a random birthdate for the ped.
    /// </summary>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public CommonFPedBuilder WithBirthdate()
    {
        var monthsWith30Days = new[] { 4, 6, 9, 11 };

        var year = Random.Next(1960, 2001);
        var month = Random.Next(1, 13);
        int maxDay;

        if (month == 2)
        {
            maxDay = 29;
        }
        else if (Array.Exists(monthsWith30Days, x => x == month))
        {
            maxDay = 31;
        }
        else
        {
            maxDay = 32;
        }

        var day = Random.Next(1, maxDay);
        this.Birthdate = new DateTime(year, month, day);

        return this;
    }

    /// <summary>
    /// Sets the birthdate of the ped.
    /// </summary>
    /// <param name="birthdate">The birthdate to set.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public CommonFPedBuilder WithBirthdate(DateTime birthdate)
    {
        this.Birthdate = birthdate;
        return this;
    }

    /// <summary>
    /// Selects and sets a random set of items for the ped.
    /// </summary>
    /// <param name="items">The items to choose from.</param>
    /// <param name="includeAll">Whether to include all specified items or select a random subset.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public CommonFPedBuilder WithItems(IEnumerable<Item> items, bool includeAll = false)
    {
        var filteredItems = items.Where(item => item.Location.ToUpper() == "PED" || item.Location.ToUpper() == "ALL");

        if (!includeAll)
        {
            filteredItems = filteredItems
                .OrderBy(_ => Guid.NewGuid())
                .Take(Random.Next(0, 6));
        }

        this.Items = filteredItems.ToList();

        return this;
    }

    /// <summary>
    /// Sets the warrants according to the options for the ped.
    /// </summary>
    /// <param name="options">The options to use.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public CommonFPedBuilder WithWarrants(WithWarrantsOptions options)
    {
        if (options.IncludeAll)
        {
            this.Warrants = options.Warrants;
            return this;
        }

        if (Random.Next(1, 101) > options.ChanceForAnyWarrant)
        {
            return this;
        }

        var maxWarrants = options.Warrants.Count < options.MaxWarrants ? options.Warrants.Count : options.MaxWarrants;
        this.Warrants = options.Warrants
            .OrderBy(_ => Guid.NewGuid())
            .Take(Random.Next(0, maxWarrants))
            .ToList();

        return this;
    }

    /// <summary>
    /// Initializes a new <see cref="CommonFPed"/> instance.
    /// </summary>
    /// <returns>Returns the newly created <see cref="CommonFPed"/> instance.</returns>
    public CommonFPed Build()
    {
        return new CommonFPed(this);
    }
}