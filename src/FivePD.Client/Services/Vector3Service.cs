using System;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.DTO;
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
            var serializedJson = this.GetRandomOffset(entityNetworkId);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, serializedJson);
        });
        
        this._eventHandlerDictionary[Events.Vector3.GetPointOnRoadSide] += new Action<string, int>((callbackId, entityNetworkId) =>
        {
            var serializedJson = this.GetRandomOffset(entityNetworkId);
            var deserializedJson = JsonConvert.DeserializeObject<Vector3ServiceDTOs.ServiceSpawnData>(serializedJson);
            var point = new Vector3();
            var outHeading = deserializedJson.heading;
            Cfx.API.GetPointOnRoadSide(deserializedJson.location.X, deserializedJson.location.Y, deserializedJson.location.Z, 1, ref point);
            var data = new Vector3ServiceDTOs.ServiceSpawnData
            {
                location = point,
                heading = outHeading
            };
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(data));
        });
    }
    
    private string GetRandomOffset(int entityNetworkId)
    {
        var entity = Entity.FromNetworkId(entityNetworkId);

        if (entity is null)
        {
            var emptyVector3 = Vector3.Zero;
            var emptyFloat = 0.0f;
            var emptyData = new Vector3ServiceDTOs.ServiceSpawnData
            {
                location = emptyVector3,
                heading = emptyFloat
            };
            return JsonConvert.SerializeObject(emptyData);
        }
        
        var val = this._random.Next(150, 250);
        var x = this._random.Next(-1*val, val);
        var y = this._random.Next(-1*val, val);
        var pos = entity.GetOffsetPosition(new Vector3(x,y,0));
        var outLocation = new Vector3();
        var outHeading = new float();
        Cfx.API.GetClosestVehicleNodeWithHeading(pos.X, pos.Y, pos.Z, ref outLocation, ref outHeading, 0, 0, 0);
        var data = new Vector3ServiceDTOs.ServiceSpawnData
        {
            location = outLocation,
            heading = outHeading
        };
        return JsonConvert.SerializeObject(data);
    }
}
