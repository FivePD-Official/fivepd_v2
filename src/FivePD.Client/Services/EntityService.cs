using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Common;

namespace FivePD.Client.Services;

#pragma warning disable

internal class EntityService : IEntityService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    public EntityService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
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
            .Where(x => x.NetworkId != Game.PlayerPed.NetworkId && x.IsInRangeOf(Game.PlayerPed.Position, 15f))
            .OrderBy(x => World.GetDistance(x.Position, Game.PlayerPed.Position))
            .FirstOrDefault();
        BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, closestEntity is null ? "-1" : closestEntity.NetworkId);
    }
}