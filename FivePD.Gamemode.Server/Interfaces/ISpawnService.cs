using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivePD.Gamemode.Server.Interfaces
{
    public interface ISpawnService
    {
        /**
         * Spawns a ped via RPC natives
         * <param name="model">A ped model for the ped to use</param>
         * <param name="location">A location where the ped will spawn</param>
         * <param name="heading">The heading the ped will use</param>
         * <returns>The spawned ped</returns>
         */
        Task<Ped> SpawnPed(uint model, Vector3 location, float heading);

        /**
         * Spawns a vehicle directly on the server
         * <param name="model">A ped model for the ped to use</param>
         * <param name="location">A location where the ped will spawn</param>
         * <param name="heading">The heading the ped will use</param>
         * <returns>The spawned vehicle</returns>
         */
        Task<Vehicle> SpawnVehicle(uint model, Vector3 location, float heading);

        /**
         * Spawns a prop (object) via RPC natives
         * <param name="model">A model for the prop to use</param>
         * <param name="location">A location where the prop will spawn</param>
         * <returns>The spawned prop</returns>
         */
        Task<Prop> SpawnProp(int model, Vector3 location);
    }
}