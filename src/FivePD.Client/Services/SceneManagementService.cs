using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.DTO;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

internal class SceneManagementService : ISceneManagementService, IOnInit
{
    private readonly Dictionary<Guid, SpeedZoneMarker> _speedZoneMarkers = new(); 

    private readonly EventHandlerDictionary _eventHandlerDictionary;

    public SceneManagementService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }
    
    public void OnInit()
    {
        this._eventHandlerDictionary[Events.SceneManagement.CreateSpeedZone] += new Action<string>(this.CreateSpeedZone);
        this._eventHandlerDictionary[Events.SceneManagement.DeleteSpeedZone] += new Action<string>(this.DeleteSpeedZone);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.SceneManagement.GetAllSpeedZones);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.SceneManagement.DeleteUI);
    }

    private void CreateSpeedZone(string serializedSpeedZone)
    {
        var dto = JsonConvert.DeserializeObject<CreateSpeedZoneDTO>(serializedSpeedZone);
        var blipId = Cfx.API.AddBlipForRadius(dto.SpeedZone.Position.X, dto.SpeedZone.Position.Y, dto.SpeedZone.Position.Z, dto.SpeedZone.Radius);
        Cfx.API.SetBlipColour(blipId, 17);
        Cfx.API.SetBlipAlpha(blipId, 150);
        var speedZoneId = Cfx.API.AddSpeedZoneForCoord(dto.SpeedZone.Position.X, dto.SpeedZone.Position.Y, dto.SpeedZone.Position.Z, dto.SpeedZone.Radius, dto.SpeedZone.Speed, false);
        this._speedZoneMarkers[dto.Id] = new SpeedZoneMarker(speedZoneId, blipId);
    }
    
    private void DeleteSpeedZone(string serializedGuid)
    {
        if (!Guid.TryParse(serializedGuid, out var id))
        {
            return;
        }

        if (!this._speedZoneMarkers.TryGetValue(id, out var marker))
        {
            return;
        }

        Cfx.API.RemoveSpeedZone(marker.SpeedZoneId);
        Cfx.API.RemoveBlip(ref marker.BlipId);

        this._speedZoneMarkers.Remove(id);
    }

    private record SpeedZoneMarker(int SpeedZoneId, int BlipId)
    {
        public readonly int SpeedZoneId = SpeedZoneId;

        public int BlipId = BlipId;
    }
}