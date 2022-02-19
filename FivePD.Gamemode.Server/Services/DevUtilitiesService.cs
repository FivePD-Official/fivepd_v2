// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services
{
    /**
     * <inheritdoc />
     */
    public class DevUtilitiesService : IDevUtilitiesService
    {
        /**
         * <inheritdoc />
         */
        public void RegisterDevelopmentCommands()
        {
            // todo: replace with proper usage of command system (once it exists)
            Cfx.API.RegisterCommand(
                "fpd_clear",
                new Action<int, List<object>, string>(
                    (source, args, rawCommand) => ClearAllEntities()),
                false);
        }

        private static void ClearAllEntities()
        {
            foreach (int entity in Cfx.API.GetAllPeds())
            {
                Cfx.API.DeleteEntity(entity);
            }

            foreach (int entity in Cfx.API.GetAllVehicles())
            {
                Cfx.API.DeleteEntity(entity);
            }

            foreach (int entity in Cfx.API.GetAllObjects())
            {
                Cfx.API.DeleteEntity(entity);
            }
        }
    }
}