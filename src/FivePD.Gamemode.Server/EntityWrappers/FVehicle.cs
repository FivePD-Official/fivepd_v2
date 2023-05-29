using FivePD.Common.Models;

namespace FivePD.Gamemode.Server.EntityWrappers;

/// <summary>
/// Asd.
/// </summary>
public class FVehicle : FEntity
{
    private readonly CommonFVehicle _vehicle;

    /// <summary>
    /// Initializes a new instance of the <see cref="FVehicle"/> class.
    /// </summary>
    /// <param name="vehicle">The FPed to use.</param>
    public FVehicle(CommonFVehicle vehicle)
    {
        this._vehicle = vehicle;
    }

    /// <summary>
    /// asdasd.
    /// </summary>
    /// <returns>Gset.</returns>
    public CommonFVehicle Get()
    {
        return this._vehicle;
    }
}