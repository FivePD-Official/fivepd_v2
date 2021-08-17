// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Injection;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Services;

namespace FivePD.Gamemode.Client
{
    /**
     * <summary>The gamemode client entrypoint. Handles the creation and management of all tasks on the client.</summary>
     */
    // ReSharper disable once UnusedType.Global
    internal class Bootstrapper : BaseScript
    {
        /**
         * <summary>
         *     Initializes a new instance of the <see cref="Bootstrapper" /> class. Used as the entrypoint for the Cfx
         *     runtime.
         * </summary>
         */
        internal Bootstrapper()
        {
            var services = new ServiceProviderBuilder();

            services
                .Register(() => this.EventHandlers)
                .As<EventHandlerDictionary>();

            services
                .RegisterType<EntityService>()
                .IsSingleton()
                .As<IEntityService>();

            var provider = services.Build();

            var spawnService = provider.Resolve<IEntityService>();

            // TODO: Remove. Only for demo purposes.
            API.RegisterCommand(
                "spawn",
                new Action<int, List<object>, string>(async (source, args, rawCommand) =>
                {
                    await spawnService.SpawnPed(PedHash.Cop01SMY, Game.PlayerPed.Position + 5f, Game.PlayerPed.Heading);
                    await spawnService.SpawnVehicle(VehicleHash.Police, Game.PlayerPed.Position + 3f, Game.PlayerPed.Heading);
                }),
                false);
        }

        // TODO: Remove. Only for demo purposes.
        #pragma warning disable 1591
        #pragma warning disable SA1600
        [Command("open")]
        internal void MenuOpen()
        {
            API.SendNuiMessage("{\"type\":\"open\"}");
            API.SetNuiFocus(true, true);
        }

        // TODO: Remove. Only for demo purposes.
        [Command("close")]
        internal void MenuClose()
        {
            API.SetNuiFocus(false, false);
            API.SendNuiMessage("{\"type\":\"close\"}");
        }
        #pragma warning restore 1591
        #pragma warning restore SA1600
    }
}