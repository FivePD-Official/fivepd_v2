// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Concurrent;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc />
    public class VehicleService : IVehicleService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IResourceService _resourceService;
        private readonly ConcurrentDictionary<int, FVehicle> _vehicles = new ConcurrentDictionary<int, FVehicle>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="resourceService">The <see cref="IResourceService" /> to use.</param>
        public VehicleService(EventHandlerDictionary eventHandlerDictionary, IResourceService resourceService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._resourceService = resourceService;
        }

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this._eventHandlerDictionary[Events.Vehicle.Generate] += new Action<string>(this.GenerateVehicle);
        }

        private void GenerateVehicle(string serializedFVehicleBuilder)
        {
            var fVehicleBuilder = JsonConvert.DeserializeObject<FVehicleBuilder>(serializedFVehicleBuilder);
            if (fVehicleBuilder is null || this._vehicles.ContainsKey(fVehicleBuilder.NetworkId))
            {
                return;
            }

            var fVehicle = fVehicleBuilder
                .WithItems(this._resourceService.GetItems())
                .Build();

            this._vehicles[fVehicle.NetworkId] = fVehicle;
        }
    }
}