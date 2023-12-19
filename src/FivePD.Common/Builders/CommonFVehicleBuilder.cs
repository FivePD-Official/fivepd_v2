using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Common.ConfigModels;
using FivePD.Common.Models;
using Newtonsoft.Json;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Common.Builders;

/// <summary>
/// Used for the construction of <see cref="CommonFVehicle"/> objects.
/// </summary>
public class CommonFVehicleBuilder
{
    protected static readonly Random Random = new(Guid.NewGuid().GetHashCode());

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFVehicleBuilder"/> class.
    /// </summary>
    /// <param name="networkId">The network identifier of the ped.</param>
    public CommonFVehicleBuilder(int networkId)
    {
        this.NetworkId = networkId;
    }

    [JsonProperty]
    public int NetworkId { get; private set; }

    [JsonProperty]
    public int OwnerNetworkId { get; protected set; }

    [JsonProperty]
    public List<int> PassengerNetworkIds { get; protected set; }

    [JsonProperty]
    public string LicensePlate { get; protected set; }

    [JsonProperty]
    public string Color { get; protected set; }

    [JsonProperty]
    public string Brand { get; protected set; }

    public List<Item> Items { get; protected set; }

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

    /// <summary>
    /// Initializes a new <see cref="CommonFVehicle"/> instance.
    /// </summary>
    /// <returns>Returns the newly created <see cref="CommonFVehicle"/> instance.</returns>
    public CommonFVehicle Build()
    {
        return new CommonFVehicle(this);
    }
}