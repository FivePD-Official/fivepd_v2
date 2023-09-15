using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Models;
using FivePD.Common;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

internal class EntityService : IEntityService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IPedService _pedService;

    public EntityService(EventHandlerDictionary eventHandlerDictionary, IPedService pedService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._pedService = pedService;
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Entity.GetClosestEntityToPlayer] += new Action<string, string>(this.GetClosestEntityToPlayer);
    }
    
    private void GetClosestEntityToPlayer(string eventCallbackId, string type)
    {
        IEnumerable<Entity> entities = new List<Entity>();
        if (type == "ped")
        {
            entities = World.GetAllPeds();
        }
        else if (type == "vehicle")
        {
            entities = World.GetAllVehicles();
        }

        var closestEntity = entities
            .OrderBy(x => World.GetDistance(x.Position, Game.PlayerPed.Position))
            .FirstOrDefault(x =>
                Cfx.API.NetworkGetEntityIsNetworked(x.Handle) && x.NetworkId != Game.PlayerPed.NetworkId &&
                x.IsInRangeOf(Game.PlayerPed.Position, 15f));

        if (closestEntity is null)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, "-1");
            return;
        }

        if (type == "ped")
        {
            var ped = Entity.FromNetworkId(closestEntity.NetworkId) as Ped;
            this._pedService.GeneratePed(ped);
        }


        BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, closestEntity.NetworkId);
    }
}