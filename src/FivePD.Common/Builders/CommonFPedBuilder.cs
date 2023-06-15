using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Gamemode.Common.ConfigModels;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Common.Models;
using Newtonsoft.Json;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Common.Builders;

/// <summary>
/// Used for the construction of <see cref="CommonFPed"/> objects.
/// </summary>
public class CommonFPedBuilder
{
    private static readonly Random Random = new(Guid.NewGuid().GetHashCode());

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFPedBuilder"/> class.
    /// </summary>
    /// <param name="networkId">The network identifier of the ped.</param>
    public CommonFPedBuilder(int networkId)
    {
        this.NetworkId = networkId;
    }

    public int NetworkId { get; private set; }

    [JsonProperty]
    public FGender Gender { get; private set; }

    public string Firstname { get; private set; }

    public string Lastname { get; private set; }

    public Date Birthdate { get; private set; }

    public List<Item> Items { get; private set; }

    public CommonFPedBuilder WithFirstname(string name)
    {
        this.Firstname = name;
        return this;
    }

    public CommonFPedBuilder WithFirstname(List<string> names)
    {
        this.Firstname = names[Random.Next(names.Count)].Trim();
        return this;
    }

    public CommonFPedBuilder WithLastname(string name)
    {
        this.Lastname = name;
        return this;
    }

    public CommonFPedBuilder WithLastname(List<string> names)
    {
        this.Lastname = names[Random.Next(names.Count)].Trim();
        return this;
    }

    public CommonFPedBuilder WithGender(FGender gender)
    {
        this.Gender = gender;
        return this;
    }

    public CommonFPedBuilder WithBirthdate()
    {
        int[] monthsWith30Days = { 4, 6, 9, 11 };

        int year = Random.Next(1960, 2001);
        int month = Random.Next(1, 13);
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

        int day = Random.Next(1, maxDay);

        // TODO: Refactor to DateTime
        this.Birthdate = new Date(year, month, day);
        return this;
    }

    public CommonFPedBuilder WithBirthdate(Date birthdate)
    {
        this.Birthdate = birthdate;
        return this;
    }

    public CommonFPedBuilder WithItems(List<Item> items)
    {
        this.Items = items
            .Where(item => item.Location is Item.ItemLocation.Ped or Item.ItemLocation.Everywhere)
            .OrderBy(item => Guid.NewGuid())
            .Take(Random.Next(0, 6))
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