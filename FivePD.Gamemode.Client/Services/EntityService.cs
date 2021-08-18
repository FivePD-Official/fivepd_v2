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
using FivePD.Gamemode.Client.Interfaces;

namespace FivePD.Gamemode.Client.Services
{
    /**
     * <inheritdoc />
     */
    public class EntityService : IEntityService
    {
        /**
         * <inheritdoc />
         */
        public async Task<Ped> SpawnPed(PedHash model, Vector3 location, float heading)
        {
            var tcs = new TaskCompletionSource<Ped>();

            async void Action(int networkId)
            {
                while (!API.NetworkDoesNetworkIdExist(networkId))
                {
                    await BaseScript.Delay(100);
                }

                tcs.SetResult(Entity.FromNetworkId(networkId) as Ped);
            }

            BaseScript.TriggerServerEvent(Events.EntityManagement.RequestPed, (uint)model, location, heading, new Action<int>(Action));

            var entity = await tcs.Task;

            return entity;
        }

        /**
         * <inheritdoc />
         */
        public async Task<Vehicle> SpawnVehicle(VehicleHash model, Vector3 location, float heading)
        {
            var tcs = new TaskCompletionSource<Vehicle>();

            async void Action(int networkId)
            {
                while (!API.NetworkDoesNetworkIdExist(networkId))
                {
                    await BaseScript.Delay(100);
                }

                tcs.SetResult(Entity.FromNetworkId(networkId) as Vehicle);
            }

            BaseScript.TriggerServerEvent(Events.EntityManagement.RequestVehicle, (uint)model, location, heading, new Action<int>(Action));

            var entity = await tcs.Task;

            return entity;
        }

        /**
         * <inheritdoc />
         */
        public async Task<Prop> SpawnProp(int model, Vector3 location)
        {
            var tcs = new TaskCompletionSource<Prop>();

            async void Action(int networkId)
            {
                while (!API.NetworkDoesNetworkIdExist(networkId))
                {
                    await BaseScript.Delay(100);
                }

                tcs.SetResult(Entity.FromNetworkId(networkId) as Prop);
            }

            BaseScript.TriggerServerEvent(Events.EntityManagement.RequestProp, model, location, new Action<int>(Action));

            var entity = await tcs.Task;

            return entity;
        }
    }
}