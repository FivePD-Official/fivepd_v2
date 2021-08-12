using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.Gamemode.Server.Interfaces;
using API = CitizenFX.Core.Native.API;

namespace FivePD.Gamemode.Server.Services
{
    public class SpawnService : ISpawnService
    {
        private static readonly Hash CreateAutomobile = (Hash) API.GetHashKey("CREATE_AUTOMOBILE");

        /**
         * Spawns a ped via RPC natives
         * <param name="model">A ped model for the ped to use</param>
         * <param name="location">A location where the ped will spawn</param>
         * <param name="heading">The heading the ped will use</param>
         * <returns>The spawned ped</returns>
         */
        public async Task<Ped> SpawnPed(uint model, Vector3 location, float heading)
        {
            var handle = API.CreatePed(26, model, location.X, location.Y, location.Z, heading, true, false);

            while (!API.DoesEntityExist(handle))
            {
                await BaseScript.Delay(100);
            }

            return new Ped(handle);
        }
        
        /**
         * Spawns a vehicle directly on the server
         * <param name="model">A ped model for the ped to use</param>
         * <param name="location">A location where the ped will spawn</param>
         * <param name="heading">The heading the ped will use</param>
         * <returns>The spawned vehicle</returns>
         */
        public async Task<Vehicle> SpawnVehicle(uint model, Vector3 location, float heading)
        {
            var handle = Function.Call<int>(CreateAutomobile, model, location.X, location.Y,
                location.Z, heading);

            // TODO: Check if we really need this delay. The server should own the vehicle immediately, anyways.
            while (!API.DoesEntityExist(handle))
            {
                await BaseScript.Delay(100);
            }

            return new Vehicle(handle);
        }
        
        /**
         * Spawns a prop (object) via RPC natives
         * <param name="model">A model for the prop to use</param>
         * <param name="location">A location where the prop will spawn</param>
         * <returns>The spawned prop</returns>
         */
        public async Task<Prop> SpawnProp(int model, Vector3 location)
        {
            var handle = API.CreateObject(model, location.X, location.Y, location.Z, true, false, false);

            while (!API.DoesEntityExist(handle))
            {
                await BaseScript.Delay(100);
            }

            return new Prop(handle);
        }
    }
}