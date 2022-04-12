// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc />
    public class PedService : IPedService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IResourceService _resourceService;
        private readonly ConcurrentDictionary<int, FPed> _peds = new ConcurrentDictionary<int, FPed>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PedService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="resourceService">The <see cref="IResourceService" /> to use.</param>
        public PedService(EventHandlerDictionary eventHandlerDictionary, IResourceService resourceService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._resourceService = resourceService;
        }

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this._eventHandlerDictionary[Events.Ped.Generate] += new Action<string>(this.GeneratePed);
        }

        private void GeneratePed(string serializedFped)
        {
            var fped = JsonConvert.DeserializeObject<FPedBuilder>(serializedFped);
            if (fped is null || this._peds.ContainsKey(fped.NetworkId))
            {
                return;
            }

            var ped = fped
                .WithFirstname("John")
                .WithLastname("Doe")
                .WithBirthdate()
                .WithItems(this._resourceService.GetItems())
                .Build();

            this._peds[fped.NetworkId] = ped;
        }
    }
}