using System;
using CitizenFX.Core;
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
        this._eventHandlerDictionary[Events.Vector3.GetRandomOffset] += new Action<string, int>((callbackId, entityNetworkId) =>
        {
            var offset = this.GetRandomOffset(entityNetworkId);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(offset));
        });

        this._eventHandlerDictionary[Events.Vector3.GetPointOnRoadSide] += new Action<string, int>((callbackId, entityNetworkId) =>
        {
            var offsetWithHeading = this.GetRandomOffsetWithHeading(entityNetworkId);
            var point = new Vector3();
            Cfx.API.GetPointOnRoadSide(offsetWithHeading.Location.X, offsetWithHeading.Location.Y, offsetWithHeading.Location.Z, 1, ref point);
            var pointOnRoadSide = offsetWithHeading with { Location = point };
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, JsonConvert.SerializeObject(pointOnRoadSide));
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
        var x = this._random.Next(-1 * val, val);
        var y = this._random.Next(-1 * val, val);

        return entity.GetOffsetPosition(new Vector3(x, y, 0));
    }

    private Vector3WithHeading GetRandomOffsetWithHeading(int entityNetworkId)
    {
        var offsetPosition = this.GetRandomOffset(entityNetworkId);
        var outLocation = new Vector3();
        var outHeading = new float();
        Cfx.API.GetClosestVehicleNodeWithHeading(offsetPosition.X, offsetPosition.Y, offsetPosition.Z, ref outLocation, ref outHeading, 0, 0, 0);

        return new Vector3WithHeading
        {
            Location = outLocation,
            Heading = outHeading
        };
    }
}