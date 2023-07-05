using CitizenFX.Core;
using FivePD.Common.Builders;
using FivePD.Common.Models;

namespace FivePD.Server.EntityWrappers.VehicleWrapper;

/// <summary>
/// Asd.
/// </summary>
public class FVehicle : FEntity
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
    }

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
    public Player Owner => this.GetUnderlyingVehicle.Owner;

    /// <summary>
    /// Created an <see cref="FVehicle"/> wrapper from the specified <see cref="Vehicle"/>.
    /// </summary>
    /// <param name="vehicle">The <see cref="Vehicle"/> instance to use.</param>
    /// <returns>The FPed.</returns>
    public static FVehicle FromVehicle(Vehicle vehicle)
    {
        return new FVehicle(new CommonFVehicle(new CommonFVehicleBuilder(vehicle.NetworkId)));
    }
}