// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Injection;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Services;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client
{
    /**
     * <summary>The gamemode client entrypoint. Handles the creation and management of all tasks on the client.</summary>
     */
    // ReSharper disable once UnusedType.Global
    public class Bootstrapper : BaseScript
    {
        /**
         * <summary>
         *     Initializes a new instance of the <see cref="Bootstrapper" /> class. Used as the entrypoint for the Cfx
         *     runtime.
         * </summary>
         */
        public Bootstrapper()
        {
            var services = new ServiceProviderBuilder();

            services
                .Register(() => this.EventHandlers)
                .As<EventHandlerDictionary>();

            services
                .Register<AddTickHandler>(() => this.OnTickAdd);
            services
                .Register<RemoveTickHandler>(() => this.OnTickRemove);

            services
                .RegisterType<TickService>()
                .As<ITickService>();

            services
                .RegisterType<ScreenDecorService>()
                .As<IScreenDecorService>();

            services
                .RegisterType<NuiService>()
                .As<INuiService>()
                .IsSingleton();

            services
                .RegisterType<KeybindService>()
                .As<IKeybindService>();

            services
                .RegisterType<LocalizationService>()
                .As<ILocalizationService>()
                .IsSingleton();

            services
                .RegisterType<RadialMenuService>()
                .As<IRadialMenuService>();

            services
                .RegisterType<MenuService>()
                .As<IMenuService>();

            var provider = services.Build();

            var screenDecorService = provider.Resolve<IScreenDecorService>();
            screenDecorService.TextToDraw = "Five~b~PD~s~ - v2.0.0 - Development Build - ~r~DO NOT DISTRIBUTE";
            screenDecorService.BeginTextDraw();

            var keybindService = provider.Resolve<KeybindService>();
            keybindService.Register(provider);

            var nuiService = provider.Resolve<NuiService>();
            nuiService.RegisterEventHandlers();

            var localizationService = provider.Resolve<LocalizationService>();
            localizationService.RegisterEventHandlers();
            localizationService.LoadDefaultLocalization();

            var radialMenuService = provider.Resolve<RadialMenuService>();
            var menuService = provider.Resolve<MenuService>();
            localizationService.LoadAvailableLocales(radialMenuService, menuService);
        }

        private void OnTickAdd(Func<Task> task)
        {
            this.Tick += task;
        }

        private void OnTickRemove(Func<Task> task)
        {
            this.Tick -= task;
        }
    }
}