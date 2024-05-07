using System;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

internal class Vector3Service : IVector3Service, IOnInit
{
    private readonly Random _random = new(Guid.NewGuid().GetHashCode());
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    public Vector3Service(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    public void OnInit()
    {
        this._eventHandlerDictionary[Events.Vector3.GetRandomOffset] += new Action<string, int>((callbackId, entityNetworkId) =>
        {
            var entity = Entity.FromNetworkId(entityNetworkId);
            if (entity is null)
            {
                BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(new Vector3(0,0,0)));
                return;
            }
            
            var val = this._random.Next(150, 250);
            var x = this._random.Next(-1*val, val);
            var y = this._random.Next(-1*val, val);
            var pos = entity.GetOffsetPosition(new Vector3(x,y,0));
            var location = new Vector3();
            Cfx.API.GetNthClosestVehicleNode(pos.X, pos.Y, pos.Z, 1, ref location, 0, 0, 0);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(location));
        });
        
        this._eventHandlerDictionary[Events.Vector3.GetPointOnRoadSide] += new Action<string, int, int>((callbackId, entityNetworkId, p3) =>
        {
            var entity = Entity.FromNetworkId(entityNetworkId);
            var val = this._random.Next(150, 250);
            var x = this._random.Next(-1*val, val);
            var y = this._random.Next(-1*val, val);
            var pos = entity.GetOffsetPosition(new Vector3(x,y,0));
            var location = new Vector3();
            Cfx.API.GetNthClosestVehicleNode(pos.X, pos.Y, pos.Z, 1, ref location, 0, 0, 0);
            var point = new Vector3();
            Cfx.API.GetPointOnRoadSide(location.X, location.Y, location.Z, p3, ref point);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(point));
        });
    }
}