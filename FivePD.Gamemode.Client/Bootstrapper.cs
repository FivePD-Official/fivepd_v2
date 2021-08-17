using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Injection;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Services;
using API = CitizenFX.Core.Native.API;

namespace FivePD.Gamemode.Client
{
    public class Bootstrapper : BaseScript
    {
        public Bootstrapper()
        {
            var services = new ServiceProviderBuilder();
            
            services
                .Register(() => EventHandlers)
                .As<EventHandlerDictionary>();

            services
                .RegisterType<EntityService>()
                .IsSingleton()
                .As<IEntityService>();

            var provider = services.Build();

            var spawnService = provider.Resolve<IEntityService>();
            
            // TODO: Remove. Only for demo purposes.
            API.RegisterCommand("spawn", new Action<int, List<object>, string>(async (source, args, rawCommand) =>
            {
                await spawnService.SpawnPed(PedHash.Cop01SMY, Game.PlayerPed.Position + 5f, Game.PlayerPed.Heading);
                await spawnService.SpawnVehicle(VehicleHash.Police, Game.PlayerPed.Position + 3f, Game.PlayerPed.Heading);
            }), false);
        }
        
        // TODO: Remove. Only for demo purposes.
        [Command("open")]
        public void MenuOpen()
        {
            API.SendNuiMessage("{\"type\":\"open\"}");
            API.SetNuiFocus(true, true);
        }

        // TODO: Remove. Only for demo purposes.
        [Command("close")]
        public void MenuClose()
        {
            API.SetNuiFocus(false, false);
            API.SendNuiMessage("{\"type\":\"close\"}");
        }
    }
}