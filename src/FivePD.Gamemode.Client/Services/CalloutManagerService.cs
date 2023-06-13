using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Common.Models;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services;

#pragma warning disable

internal class CalloutManagerService : ICalloutManagerService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickService _tickService;

    private ClientCallout _currentCallout;
    private Blip _calloutBlip;

    public CalloutManagerService(EventHandlerDictionary eventHandlerDictionary, ITickService tickService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickService = tickService;
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Callout.OnAccepted] += new Action<string>(this.OnAccepted);
        this._eventHandlerDictionary[Events.Callout.OnCompleted] += new Action(this.OnCompleted);
    }

    private void OnAccepted(string serializedCallout)
    {
        var callout = JsonConvert.DeserializeObject<ClientCallout>(serializedCallout);
        this._currentCallout = callout;
        this._tickService.OnTick += this.OnTick;

        this._calloutBlip = World.CreateBlip(callout.Location, callout.ActivityRadius);
        var blipColor = callout.Priority switch
        {
            FIncidentPriority.Code1 => (BlipColor)FBlipColor.Blue,
            FIncidentPriority.Code2 => (BlipColor)FBlipColor.Yellow,
            FIncidentPriority.Code3 => (BlipColor)FBlipColor.Red,
        };
        this._calloutBlip.Name = callout.Title;
        this._calloutBlip.Alpha = 100;
        this._calloutBlip.Sprite = BlipSprite.BigCircle;
        this._calloutBlip.ShowRoute = true;
        Cfx.API.SetBlipColour(this._calloutBlip.Handle, (int)blipColor);
    }

    private void OnCompleted()
    {
        this._currentCallout = null;
        var calloutBlipHandle = this._calloutBlip.Handle;
        Cfx.API.RemoveBlip(ref calloutBlipHandle);
    }

    private Task OnTick()
    {
        if (this._currentCallout is null)
        {
            return Task.FromResult(0);
        }

        if (World.GetDistance(this._currentCallout.Location, Game.PlayerPed.Position) <= this._currentCallout.ActivityRadius)
        {
            this._calloutBlip.ShowRoute = false;
            BaseScript.TriggerServerEvent(Events.Callout.OnPlayerEnteredInActivityZone, this._currentCallout.Id);
        }

        return Task.FromResult(0);
    }
}