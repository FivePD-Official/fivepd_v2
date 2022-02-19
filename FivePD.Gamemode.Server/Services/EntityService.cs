// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services
{
    /**
     * <inheritdoc />
     */
    public class EntityService : IEntityService
    {
        private static readonly Cfx.Hash CreateAutomobile = (Cfx.Hash)Cfx.API.GetHashKey("CREATE_AUTOMOBILE");

        /** <summary>
        * Initializes a new instance of the <see cref="EntityService"/> class.
        * </summary>
        */
        public EntityService()
        {
        }

        /**
         * <inheritdoc />
         */
        public async Task<Ped> SpawnPed(uint model, Vector3 location, float heading)
        {
            var handle = Cfx.API.CreatePed(26, model, location.X, location.Y, location.Z, heading, true, false);

            while (!Cfx.API.DoesEntityExist(handle))
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
            var handle = Cfx.Function.Call<int>(CreateAutomobile, model, location.X, location.Y, location.Z, heading);

            // TODO: Check if we really need this delay. The server should own the vehicle immediately, anyways.
            while (!Cfx.API.DoesEntityExist(handle))
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
            var handle = Cfx.API.CreateObject(model, location.X, location.Y, location.Z, true, false, false);

            while (!Cfx.API.DoesEntityExist(handle))
            {
                await BaseScript.Delay(100);
            }

            return new Prop(handle);
        }
    }
}