using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.Common;
using FivePD.Gamemode.Server.Interfaces;
using API = CitizenFX.Core.Native.API;

namespace FivePD.Gamemode.Server.Services
{
    public class EntityService : IEntityService
    {
        private static readonly Hash CreateAutomobile = (Hash) API.GetHashKey("CREATE_AUTOMOBILE");
        
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        
        public EntityService(EventHandlerDictionary eventHandlerDictionary)
        {
            _eventHandlerDictionary = eventHandlerDictionary;
        }
        
        /**
         * <inheritdoc />
         * This implementation uses local functions to "redirect" the normal task-returning methods to voids.
         */
        public void RegisterEventHandlers()
        {
            _eventHandlerDictionary.Add(Events.EntityManagement.RequestPed, new Action<uint, Vector3, float, NetworkCallbackDelegate>(
                SpawnPedAction
            ));
            
            _eventHandlerDictionary.Add(Events.EntityManagement.RequestVehicle, new Action<uint, Vector3, float, NetworkCallbackDelegate>(
                SpawnVehicleAction
            ));
            
            _eventHandlerDictionary.Add(Events.EntityManagement.RequestProp, new Action<int, Vector3, NetworkCallbackDelegate>(
                SpawnPropAction
            ));
        }

        public async Task<Ped> SpawnPed(uint model, Vector3 location, float heading)
        {
            var handle = API.CreatePed(26, model, location.X, location.Y, location.Z, heading, true, false);

            while (!API.DoesEntityExist(handle))
            {
                await BaseScript.Delay(100);
            }

            return new Ped(handle);
        }

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
        
        public async Task<Prop> SpawnProp(int model, Vector3 location)
        {
            var handle = API.CreateObject(model, location.X, location.Y, location.Z, true, false, false);

            while (!API.DoesEntityExist(handle))
            {
                await BaseScript.Delay(100);
            }

            return new Prop(handle);
        }
        
        /**
         * Used as an internal event handler.
         */
        private async void SpawnPedAction(uint model, Vector3 location, float heading, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var task = await SpawnPed(model, location, heading);

            await networkCallbackDelegate.Invoke(task.NetworkId);
        }
        
        /**
         * Used as an internal event handler.
         */
        private async void SpawnVehicleAction(uint model, Vector3 location, float heading, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var task = await SpawnVehicle(model, location, heading);

            await networkCallbackDelegate.Invoke(task.NetworkId);
        }
        
        /**
         * Used as an internal event handler.
         */
        private async void SpawnPropAction(int model, Vector3 location, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var task = await SpawnProp(model, location);

            await networkCallbackDelegate.Invoke(task.NetworkId);
        }
    }
}