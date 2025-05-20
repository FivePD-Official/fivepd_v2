using System;
using CitizenFX.Core;
using FivePD.Client.DTO;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Models;
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
        this._eventHandlerDictionary[Events.Vector3.GetRandomOffset] += new Action<string, string>((callbackId, serializedRequest) =>
        {
            var request = JsonConvert.DeserializeObject<RandomOffsetDTO>(serializedRequest);
            var offset = this.GetRandomOffset(request.EntityNetworkId, request.OffsetMin, request.OffsetMax);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(offset));
        });
        
        this._eventHandlerDictionary[Events.Vector3.GetPointOnRoadSide] += new Action<string, string>((callbackId, serializedRequest) =>
        {
            var request = JsonConvert.DeserializeObject<RandomOffsetDTO>(serializedRequest);
            var offsetWithHeading = this.GetRandomOffsetWithHeading(request.EntityNetworkId, request.OffsetMin, request.OffsetMax);
            var point = new Vector3();
            Cfx.API.GetPointOnRoadSide(offsetWithHeading.Location.X, offsetWithHeading.Location.Y, offsetWithHeading.Location.Z, 1, ref point);
            var pointOnRoadSide = offsetWithHeading with { Location = point };
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(pointOnRoadSide));
        });
    }

    private Vector3 GetRandomOffset(int entityNetworkId, int offsetMin, int offsetMax)
    {
        var entity = Entity.FromNetworkId(entityNetworkId);
        if (entity is null)
        {
            return Vector3.Zero;
        }

        var val = this._random.Next(offsetMin, offsetMax);
        var x = this._random.Next(-1 * val, val);
        var y = this._random.Next(-1 * val, val);

        return entity.GetOffsetPosition(new Vector3(x, y, 0));
    }

    private Vector3WithHeading GetRandomOffsetWithHeading(int entityNetworkId, int offsetMin, int offsetMax)
    {
        var offsetPosition = this.GetRandomOffset(entityNetworkId, offsetMin, offsetMax);
        var outLocation = new Vector3();
        var outHeading = 0.0f;
        Cfx.API.GetClosestVehicleNodeWithHeading(offsetPosition.X, offsetPosition.Y, offsetPosition.Z, ref outLocation, ref outHeading, 0, 0, 0);

        return new Vector3WithHeading
        {
            Location = outLocation,
            Heading = outHeading
        };
    }
}