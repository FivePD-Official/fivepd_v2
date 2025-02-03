using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.ConfigModels;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;

namespace FivePD.Server.EntityWrappers.VehicleWrapper;

/// <summary>
/// Asd.
/// </summary>
public class FVehicle : FEntity, IFStatefulContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FVehicle"/> class.
    /// </summary>
    /// <param name="builder">The FPed to use.</param>
    public FVehicle(FVehicleBuilder builder)
    {
        this.Properties = new(this);

        this.NetworkId = builder.NetworkId;
        this.OwnerNetworkId = builder.OwnerNetworkId;

        this.Stopped = new FStateBagObservable<bool>(this, StateBagKeys.Vehicle.Stopped, false);
        this.HasGeneratedData = new FStateBagObservable<bool>(this, StateBagKeys.Vehicle.HasGeneratedData, false);
    }

    /// <summary>
    /// Gets the vehicle's network identifier.
    /// </summary>
    public int NetworkId { get; }

    /// <summary>
    /// Gets the PedHistory id.
    /// </summary>
    public int VehicleHistoryId { get; internal set; }

    /// <summary>
    /// Gets the network identifier of the vehicle's owner ped.
    /// </summary>
    public int OwnerNetworkId { get; }

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
    /// Gets the underlying <see cref="Vehicle"/>.
    /// </summary>
    public Vehicle GetUnderlyingVehicle => Entity.FromNetworkId(this.NetworkId) as Vehicle;

    /// <summary>
    /// Gets the owner of the <see cref="FVehicle"/>.
    /// </summary>
    public Player Owner => this.GetUnderlyingVehicle?.Owner;

    /// <summary>
    /// Created an <see cref="FVehicle"/> wrapper from the specified <see cref="Vehicle"/>.
    /// </summary>
    /// <param name="vehicle">The <see cref="Vehicle"/> instance to use.</param>
    /// <returns>The FVehicle.</returns>
    public static FVehicle FromVehicle(Vehicle vehicle)
    {
        return new FVehicle(new FVehicleBuilder(new CommonFVehicleBuilder(vehicle.NetworkId)));
    }

    /// <summary>
    /// Created an <see cref="FVehicle"/> wrapper from the specified <see cref="Vehicle"/>.
    /// </summary>
    /// <param name="networkId">The network ID of the vehicle.</param>
    /// <returns>The FVehicle.</returns>
    public static FVehicle FromNetId(int networkId)
    {
        return new FVehicle(new FVehicleBuilder(new CommonFVehicleBuilder(networkId)));
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