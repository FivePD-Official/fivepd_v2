using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Gamemode.Common.ConfigModels;
using FivePD.Gamemode.Common.Models;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Common.Builders;

/// <summary>
/// Used for the construction of <see cref="CommonFVehicle"/> objects.
/// </summary>
public class CommonFVehicleBuilder
{
    private static readonly Random Random = new(Guid.NewGuid().GetHashCode());

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFVehicleBuilder"/> class.
    /// </summary>
    /// <param name="networkId">The network identifier of the ped.</param>
    public CommonFVehicleBuilder(int networkId)
    {
        this.NetworkId = networkId;
    }

    public int NetworkId { get; private set; }

    public int OwnerNetworkId { get; private set; }

    public List<int> PassengerNetworkIds { get; private set; }

    public string LicensePlate { get; private set; }

    public string Color { get; private set; }

    public string Brand { get; private set; }

    public List<Item> Items { get; private set; }

    public CommonFVehicleBuilder WithOwner(int networkId)
    {
        this.OwnerNetworkId = networkId;
        return this;
    }

    public CommonFVehicleBuilder WithPassengers(List<int> passengerNetworkIds)
    {
        this.PassengerNetworkIds = passengerNetworkIds;
        return this;
    }

    public CommonFVehicleBuilder WithLicensePlate(string licensePlate)
    {
        this.LicensePlate = licensePlate;
        return this;
    }

    public CommonFVehicleBuilder WithColor(string color)
    {
        this.Color = color;
        return this;
    }

    public CommonFVehicleBuilder WithBrand(string brand)
    {
        this.Brand = brand;
        return this;
    }

    public CommonFVehicleBuilder WithItems(List<Item> items)
    {
        this.Items = items
            .Where(item => item.Location is Item.ItemLocation.Vehicle or Item.ItemLocation.Everywhere)
            .OrderBy(_ => Guid.NewGuid())
            .Take(Random.Next(0, 6))
            .ToList();
        return this;
    }

    /// <summary>
    /// Initializes a new <see cref="CommonFVehicle"/> instance.
    /// </summary>
    /// <returns>Returns the newly created <see cref="CommonFVehicle"/> instance.</returns>
    public CommonFVehicle Build()
    {
        return new CommonFVehicle(this);
    }
}