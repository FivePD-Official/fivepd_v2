// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Threading.Tasks;
using FivePD.Gamemode.API;

namespace FivePD.Gamemode.IA.AmbientEvents
{
    /**
     * <summary>Responsible for creating and management various ambient
     * events around players (e.x.: carjackings or speeders).</summary>
     */
    [AddonMetadata("AmbientEventProvider", "1.0.0", 100)]
    public class AmbientEvents : IGamemodeAddon
    {
        /**
         * <inheritdoc />
         */
        public Task OnStarted()
        {
            Console.WriteLine("I started!");

            return Task.CompletedTask;
        }

        /**
         * <inheritdoc />
         */
        public Task OnStopped()
        {
            Console.WriteLine("I started!");

            return Task.FromResult(0);
        }
    }
}