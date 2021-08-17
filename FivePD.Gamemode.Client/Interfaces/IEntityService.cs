using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivePD.Gamemode.Client.Interfaces
{
    public interface IEntityService
    {
        /**
         * Requests that the server spawn a ped.
         * <param name="model">A ped model for the ped to use.</param>
         * <param name="location">A location where the ped will spawn.</param>
         * <param name="heading">The heading the ped will use.</param>
         * <returns>The spawned ped.</returns>
         */
        Task<Ped> SpawnPed(PedHash model, Vector3 location, float heading);

        /**
         * Requests that the server spawn a vehicle.
         * <param name="model">A ped model for the ped to use.</param>
         * <param name="location">A location where the ped will spawn.</param>
         * <param name="heading">The heading the ped will use.</param>
         * <returns>The spawned vehicle.</returns>
         */
        Task<Vehicle> SpawnVehicle(VehicleHash model, Vector3 location, float heading);

        /**
         * Requests that the server spawn a prop (object).
         * <param name="model">A model for the prop to use.</param>
         * <param name="location">A location where the prop will spawn.</param>
         * <returns>The spawned prop.</returns>
         */
        Task<Prop> SpawnProp(int model, Vector3 location);
    }
}