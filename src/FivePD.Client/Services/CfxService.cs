using System;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Common;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

public class CfxService : ICfxService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    public CfxService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary["fivepd:cfx:getClosestVehicle"] += new Action<string, Vector3, float>((callbackId, location, radius) =>
        {
            Debug.WriteLine("asdasd");
            Debug.WriteLine($"{callbackId}");
            Debug.WriteLine($"{location}");
            Debug.WriteLine($"{radius}");
            var closestVehicle = World.GetAllVehicles().FirstOrDefault(x => World.GetDistance(x.Position, location) <= radius);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, closestVehicle?.NetworkId ?? 0);
        });
        this._eventHandlerDictionary["fivepd:cfx:AttachEntityToEntity"] += new Action<
            int,
            int,
            int,
            float,
            float,
            float,
            float,
            float,
            float,
            bool,
            bool,
            bool,
            bool,
            int,
            bool>((i, i1, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) =>
        {
            var entity1 = Entity.FromNetworkId(i);
            var entity2 = Entity.FromNetworkId(i1);
            Cfx.API.AttachEntityToEntity(entity1.Handle, entity2.Handle, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        });
        this._eventHandlerDictionary["fivepd:cfx:SetPedIntoVehicle"] += new Action<
            int,
            int,
            int>((i, i1, i2) =>
        {
            var entity1 = Entity.FromNetworkId(i);
            entity1.State["asd"] = true;
            Cfx.API.SetPedIntoVehicle(entity1.Handle, i1, i2);
        });
    }
}