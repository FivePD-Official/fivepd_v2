using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Client.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class EntityService : IEntityService
    {
        /// <inheritdoc />
        public Task<Ped> SpawnPedAsync(uint model, Vector3 location, float heading)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<Vehicle> SpawnVehicleAsync(uint model, Vector3 location, float heading)
        {
            bool hasServerResponded = false;
            int vehicleNetworkId = 0;

            BaseScript.TriggerServerEvent(Events.EntityManagement.RequestVehicle, model, location, heading, new Action<int>(networkId =>
            {
                hasServerResponded = true;
                vehicleNetworkId = networkId;
            }));

            while (!hasServerResponded)
            {
                await BaseScript.Delay(100);
            }

            return (Vehicle)Entity.FromNetworkId(vehicleNetworkId);
        }

        /// <inheritdoc />
        public Task<Prop> SpawnPropAsync(int model, Vector3 location)
        {
            throw new NotImplementedException();
        }
    }
}