using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Server.EntityWrappers.PedWrapper;
using FivePD.Gamemode.Server.EntityWrappers.VehicleWrapper;
using FivePD.Gamemode.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc />
public class EntityService : IEntityService
{
    /// <inheritdoc />
    public async Task<FPed> SpawnPedAsync(uint model, Vector3 location, float heading)
    {
        var handle = Cfx.API.CreatePed(26, model, location.X, location.Y, location.Z, heading, true, false);

        while (!Cfx.API.DoesEntityExist(handle))
        {
            // TODO: maybe use Task instead of BaseScript.Delay?
            await BaseScript.Delay(100);
        }

        Cfx.API.SetEntityDistanceCullingRadius(handle, 10000f);

        return FPed.FromPed(new Ped(handle));
    }

    /// <inheritdoc />
    public async Task<FVehicle> SpawnVehicleAsync(uint model, Vector3 location, float heading)
    {
        // TODO: clear area
        var handle = Cfx.API.CreateVehicle(model, location.X, location.Y, location.Z, heading, true, false);

        while (!Cfx.API.DoesEntityExist(handle))
        {
            await BaseScript.Delay(100);
        }

        Cfx.API.SetEntityDistanceCullingRadius(handle, 10000f);

        return FVehicle.FromVehicle(new Vehicle(handle));
    }

    /// <inheritdoc />
    public async Task<Prop> SpawnPropAsync(int model, Vector3 location)
    {
        var handle = Cfx.API.CreateObject(model, location.X, location.Y, location.Z, true, false, false);

        while (!Cfx.API.DoesEntityExist(handle))
        {
            await BaseScript.Delay(100);
        }

        Cfx.API.SetEntityDistanceCullingRadius(handle, 10000f);

        return new Prop(handle);
    }
}