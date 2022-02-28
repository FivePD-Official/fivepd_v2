// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
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
                .As<INuiService>();

            var provider = services.Build();

            var screenDecorService = provider.Resolve<IScreenDecorService>();
            screenDecorService.TextToDraw = "Five~b~PD~s~ - v2.0.0 - Development Build - ~r~DO NOT DISTRIBUTE";
            screenDecorService.BeginTextDraw();

            var nuiService = provider.Resolve<NuiService>();
            nuiService.RegisterEventHandlers();
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