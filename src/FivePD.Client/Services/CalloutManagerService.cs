using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.DTO;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Models;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.ICalloutManagerService" />
internal class CalloutManagerService : ICalloutManagerService, IServiceWithEvents, IDisposableService
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickService _tickService;
    private readonly ILocalPlayerService _localPlayerService;

    private ClientCalloutWrapper _currentCallout;
    private Blip _calloutBlip;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutManagerService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="localPlayerService">The <see cref="ILocalPlayerService"/> to use.</param>
    public CalloutManagerService(EventHandlerDictionary eventHandlerDictionary, ITickService tickService, ILocalPlayerService localPlayerService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickService = tickService;
        this._localPlayerService = localPlayerService;
    }

    /// <inheritdoc/>
    public void RegisterEvents()
    {
        this._tickService.OnTick += this.OnTickAsync;

        this._eventHandlerDictionary[Events.Callout.OnAccepted] += new Action<string>(this.OnAccepted);
        this._eventHandlerDictionary[Events.Callout.OnCompleted] += new Action(this.OnCompleted);

        this._eventHandlerDictionary.AddUIEvent(
            Events.UI.Callout.GetActive,
            new Action<ExpandoObject, CallbackDelegate>((_, cb) => { cb.Invoke(JsonConvert.SerializeObject(this._currentCallout)); }));

        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Callout.GetAllTypes);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Callout.Dispatch);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Callout.SelfAssign);
        this._eventHandlerDictionary.AddUIToServerEventOverrideData(
            Events.Callout.GetAllActiveUI,
            serverResponse =>
            {
                var callouts = JsonConvert.DeserializeObject<Page<CalloutDTO.CalloutToClientDTO>>(serverResponse);
                var newCallouts = callouts.Items.Select(callout => new CalloutDTO.CalloutToUIDTO
                    {
                        Id = callout.Id,
                        Title = callout.Title,
                        Description = callout.Description,
                        Units = callout.Units,
                        Address = callout.Location.GetStreetName(),
                    })
                    .ToList();
                var newPage = new Page<CalloutDTO.CalloutToUIDTO>
                {
                    Items = newCallouts,
                    CurrentPage = 1,
                    ItemCount = newCallouts.Count,
                    PageCount = 1,
                    PageSize = newCallouts.Count,
                };

                return JsonConvert.SerializeObject(newPage);
            });
    }

    /// <inheritdoc/>
    public void InternalDispose()
    {
        this._tickService.OnTick -= this.OnTickAsync;
    }

    private void OnAccepted(string serializedCallout)
    {
        var callout = JsonConvert.DeserializeObject<ClientCallout>(serializedCallout);
        this._currentCallout = new ClientCalloutWrapper(callout, callout.Location.GetStreetName());
        this._localPlayerService.UpdateInformationCallout(this._currentCallout);

        this._calloutBlip = World.CreateBlip(callout.Location, callout.ActivityRadius);
        var blipColor = callout.Priority switch
        {
            FIncidentPriority.Code1 => (BlipColor)FBlipColor.Blue,
            FIncidentPriority.Code2 => (BlipColor)FBlipColor.Yellow,
            FIncidentPriority.Code3 => (BlipColor)FBlipColor.Red,
            _ => (BlipColor)FBlipColor.White,
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
        this._localPlayerService.UpdateInformationCallout(null);
        var calloutBlipHandle = this._calloutBlip.Handle;
        Cfx.API.RemoveBlip(ref calloutBlipHandle);
    }

    private Task OnTickAsync()
    {
        if (this._currentCallout is null)
        {
            return Task.FromResult(0);
        }

        if (World.GetDistance(this._currentCallout.Callout.Location, Game.PlayerPed.Position) <= this._currentCallout.Callout.ActivityRadius)
        {
            this._calloutBlip.ShowRoute = false;
            BaseScript.TriggerServerEvent(Events.Callout.OnPlayerEnteredInActivityZone, this._currentCallout.Callout.Id);
        }

        return Task.FromResult(0);
    }
}