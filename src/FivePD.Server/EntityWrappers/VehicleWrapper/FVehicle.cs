using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.ConfigModels;
using FivePD.Common.Models;

namespace FivePD.Server.EntityWrappers.VehicleWrapper;

/// <summary>
/// Asd.
/// </summary>
public class FVehicle : FEntity, IFStatefulContainer
{
    private readonly CommonFVehicle _fVehicle;

    /// <summary>
    /// Initializes a new instance of the <see cref="FVehicle"/> class.
    /// </summary>
    /// <param name="fVehicle">The FPed to use.</param>
    public FVehicle(CommonFVehicle fVehicle)
    {
        this._fVehicle = fVehicle;
        this.Properties = new(this);

        this.NetworkId = fVehicle.NetworkId;
        this.LicensePlate = fVehicle.LicensePlate;
        this.Color = fVehicle.Color;
        this.Brand = fVehicle.Brand;
        this.Items = fVehicle.Items;

        this.Stopped = new FStateBagObservable<bool>(this, StateBagKeys.Vehicle.Stopped, false);
        this.HasGeneratedData = new FStateBagObservable<bool>(this, StateBagKeys.Vehicle.HasGeneratedData, false);
    }

    /// <summary>
    /// Gets the vehicle's network identifier.
    /// </summary>
    public int NetworkId { get; }

    /// <summary>
    /// Gets the vehicle's license plate.
    /// </summary>
    public string LicensePlate { get; }

    /// <summary>
    /// Gets the vehicle's color.
    /// </summary>
    public string Color { get; }

    /// <summary>
    /// Gets the vehicle's brand.
    /// </summary>
    public string Brand { get; }

    /// <summary>
    /// Gets the items of the vehicle.
    /// </summary>
    public List<Item> Items { get; }

    /// <summary>
    /// Gets the vehicle's observable with a value indicating if the vehicle has been stopped.
    /// </summary>
    public FStateBagObservable<bool> Stopped { get; private set; }

    /// <summary>
    /// Gets the vehicle's observable with a value indicating if the vehicle has generated data.
    /// </summary>
    public FStateBagObservable<bool> HasGeneratedData { get; private set; }

    /// <summary>
    /// Gets the properties of the <see cref="FVehicle"/>.
    /// </summary>
    public FVehicleProperties Properties { get; private set; }

    /// <summary>
    /// Gets the underlying <see cref="CommonFVehicle"/>.
    /// </summary>
    public CommonFVehicle GetFVehicle => this._fVehicle;

    /// <summary>
    /// Gets the underlying <see cref="Ped"/>.
    /// </summary>
    public Vehicle GetUnderlyingVehicle => Entity.FromNetworkId(this._fVehicle.NetworkId) as Vehicle;

    /// <summary>
    /// Gets the owner of the <see cref="FVehicle"/>.
    /// </summary>
    public Player Owner => this.GetUnderlyingVehicle?.Owner;

    /// <summary>
    /// Created an <see cref="FVehicle"/> wrapper from the specified <see cref="Vehicle"/>.
    /// </summary>
    /// <param name="vehicle">The <see cref="Vehicle"/> instance to use.</param>
    /// <returns>The FPed.</returns>
    public static FVehicle FromVehicle(Vehicle vehicle)
    {
        return new FVehicle(new CommonFVehicle(new CommonFVehicleBuilder(vehicle.NetworkId)));
    }

    /// <inheritdoc />
    public string GetStateKey()
    {
        return $"vehicle:{this.GetUnderlyingVehicle?.NetworkId ?? 0}";
    }

    /// <inheritdoc />
    public StateBag GetStateBag()
    {
        return this.GetUnderlyingVehicle?.State;
    }
}