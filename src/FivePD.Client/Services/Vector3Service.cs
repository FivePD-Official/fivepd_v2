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
            var location = this.GetRandomOffset(entityNetworkId);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(location));
        });
        
        this._eventHandlerDictionary[Events.Vector3.GetPointOnRoadSide] += new Action<string, int>((callbackId, entityNetworkId) =>
        {
            var location = this.GetRandomOffset(entityNetworkId);
            var point = new Vector3();
            Cfx.API.GetPointOnRoadSide(location.X, location.Y, location.Z, 1, ref point);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(point));
        });
    }
    
    private Vector3 GetRandomOffset(int entityNetworkId)
    {
        var entity = Entity.FromNetworkId(entityNetworkId);

        if (entity is null)
        {
            return Vector3.Zero;
        }
        
        var val = this._random.Next(150, 250);
        var x = this._random.Next(-1*val, val);
        var y = this._random.Next(-1*val, val);
        var pos = entity.GetOffsetPosition(new Vector3(x,y,0));
        var outLocation = new Vector3();
        var outHeading = new float();
        Cfx.API.GetClosestVehicleNodeWithHeading(pos.X, pos.Y, pos.Z, ref outLocation, ref outHeading, 0, 0, 0);
        return outLocation;
    }
}
