// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.Common;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.Services
{
    /**
     * <inheritdoc />
     */
    internal class EntityService : IEntityService
    {
        private static readonly Hash CreateAutomobile = (Hash)API.GetHashKey("CREATE_AUTOMOBILE");

        private readonly EventHandlerDictionary _eventHandlerDictionary;

        /** <summary>
        * Initializes a new instance of the <see cref="EntityService"/> class.
        * </summary>
        * <param name="eventHandlerDictionary">An event handler dictionary to use.</param>
        */
        internal EntityService(EventHandlerDictionary eventHandlerDictionary)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
        }

        /**
         * <inheritdoc />
         * This implementation uses local functions to "redirect" the normal task-returning methods to voids.
         */
        public void RegisterEventHandlers()
        {
            this._eventHandlerDictionary.Add(
                Events.EntityManagement.RequestPed,
                new Action<uint, Vector3, float, NetworkCallbackDelegate>(
                    this.OnSpawnPedAction));

            this._eventHandlerDictionary.Add(
                Events.EntityManagement.RequestVehicle,
                new Action<uint, Vector3, float, NetworkCallbackDelegate>(
                    this.OnSpawnVehicleAction));

            this._eventHandlerDictionary.Add(
                Events.EntityManagement.RequestProp,
                new Action<int, Vector3, NetworkCallbackDelegate>(
                    this.OnSpawnPropAction));
        }

        /**
         * <inheritdoc />
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
         * <inheritdoc />
         */
        public async Task<Vehicle> SpawnVehicle(uint model, Vector3 location, float heading)
        {
            var handle = Function.Call<int>(CreateAutomobile, model, location.X, location.Y, location.Z, heading);

            // TODO: Check if we really need this delay. The server should own the vehicle immediately, anyways.
            while (!API.DoesEntityExist(handle))
            {
                await BaseScript.Delay(100);
            }

            return new Vehicle(handle);
        }

        /**
         * <inheritdoc />
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

        /**
         * Used as an internal event handler.
         */
        private async void OnSpawnPedAction(uint model, Vector3 location, float heading, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var task = await this.SpawnPed(model, location, heading);

            await networkCallbackDelegate.Invoke(task.NetworkId);
        }

        /**
         * Used as an internal event handler.
         */
        private async void OnSpawnVehicleAction(uint model, Vector3 location, float heading, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var task = await this.SpawnVehicle(model, location, heading);

            await networkCallbackDelegate.Invoke(task.NetworkId);
        }

        /**
         * Used as an internal event handler.
         */
        private async void OnSpawnPropAction(int model, Vector3 location, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var task = await this.SpawnProp(model, location);

            await networkCallbackDelegate.Invoke(task.NetworkId);
        }
    }
}