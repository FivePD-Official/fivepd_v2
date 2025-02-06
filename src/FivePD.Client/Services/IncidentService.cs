using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Client.Models;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

internal class IncidentService : IIncidentService, IOnInit, IOnDestroy
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickService _tickService;
    private readonly ILocalPlayerService _localPlayerService;

    private ClientIncidentWithAddressDTO _currentIncident;
    private Blip _incidentBlip;
    private bool _triggeredOnPlayerEnteredInActivityZone;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutManagerService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="localPlayerService">The <see cref="ILocalPlayerService"/> to use.</param>
    public IncidentService(EventHandlerDictionary eventHandlerDictionary, ITickService tickService, ILocalPlayerService localPlayerService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickService = tickService;
        this._localPlayerService = localPlayerService;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._tickService.OnTick += this.OnTickAsync;

        this._eventHandlerDictionary[Events.Incident.OnCompleted] += new Action(this.OnCompleted);
        this._eventHandlerDictionary[Events.Incident.OnAssigned] += new Action<string>(this.OnAssigned);
        this._eventHandlerDictionary.AddUIEvent(
            Events.Incident.GetAssigned,
            new Action<ExpandoObject, CallbackDelegate>((_, cb) => { cb.Invoke(JsonConvert.SerializeObject(this._currentIncident)); }));
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Incident.SelfAssign);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Incident.Complete);
        this._eventHandlerDictionary.AddUIToServerEventOverrideData(
            Events.Incident.Get,
            serverResponse =>
            {
                var incident = JsonConvert.DeserializeObject<IncidentDTOs.IncidentToClientDTO>(serverResponse);
                var mappedIncident = new IncidentDTOs.IncidentToUIDTO
                {
                    Id = incident.Id,
                    Type = incident.Type,
                    Title = incident.Title,
                    Description = incident.Description,
                    Units = incident.Units,
                    Address = incident.Location.GetStreetName(),
                };

                return JsonConvert.SerializeObject(mappedIncident);
            });
        this._eventHandlerDictionary.AddUIToServerEventOverrideData(
            Events.Incident.GetAllActive,
            serverResponse =>
            {
                var page = JsonConvert.DeserializeObject<Page<IncidentDTOs.IncidentToClientDTO>>(serverResponse);
                var mappedIncidents = page.Items.Select(incident => new IncidentDTOs.IncidentToUIDTO
                    {
                        Id = incident.Id,
                        Type = incident.Type,
                        Priority = incident.Priority,
                        Title = incident.Title,
                        Description = incident.Description,
                        Units = incident.Units,
                        Address = incident.Location.GetStreetName(),
                    })
                    .ToList();
                var newPage = new Page<IncidentDTOs.IncidentToUIDTO>
                {
                    Items = mappedIncidents,
                    CurrentPage = 1,
                    ItemCount = mappedIncidents.Count,
                    PageCount = 1,
                    PageSize = mappedIncidents.Count,
                };

                return JsonConvert.SerializeObject(newPage);
            });
    }

    /// <inheritdoc/>
    public void OnDestroy()
    {
        this._tickService.OnTick -= this.OnTickAsync;
    }

    private Task OnTickAsync()
    {
        if (this._currentIncident is null || this._currentIncident.Incident.Type != FIncidentType.Callout || this._triggeredOnPlayerEnteredInActivityZone)
        {
            return Task.FromResult(0);
        }

        if (World.GetDistance(this._currentIncident.Incident.Location, Game.PlayerPed.Position) <= this._currentIncident.Incident.ActivityRadius)
        {
            this._triggeredOnPlayerEnteredInActivityZone = true;
            this._incidentBlip.ShowRoute = false;
            BaseScript.TriggerServerEvent(Events.Callout.OnPlayerEnteredInActivityZone, this._currentIncident.Incident.Id);
        }

        return Task.FromResult(0);
    }

    private void OnCompleted()
    {
        this._currentIncident = null;
        this._triggeredOnPlayerEnteredInActivityZone = false;
        this._localPlayerService.UpdateInformationCallout(null);

        if (this._incidentBlip is not null)
        {
            var incidentBlipHandle = this._incidentBlip.Handle;
            Cfx.API.RemoveBlip(ref incidentBlipHandle);
            this._incidentBlip = null;
        }
    }

    private void OnAssigned(string serializedRequest)
    {
        var incident = JsonConvert.DeserializeObject<ClientIncidentDTO>(serializedRequest);
        this._currentIncident = new ClientIncidentWithAddressDTO(incident, incident.Location.GetStreetName());
        this._localPlayerService.UpdateInformationCallout(this._currentIncident);

        if (this._incidentBlip is not null)
        {
            var incidentBlipHandle = this._incidentBlip.Handle;
            Cfx.API.RemoveBlip(ref incidentBlipHandle);
        }

        switch (incident.Type)
        {
            case FIncidentType.Callout:
                this._incidentBlip = World.CreateBlip(incident.Location, incident.ActivityRadius);
                this._incidentBlip.Alpha = 100;
                this._incidentBlip.Sprite = BlipSprite.BigCircle;
                break;
            case FIncidentType.TrafficStop:
                if (Cfx.API.NetworkDoesEntityExistWithNetworkId(incident.TrafficStopInvolvedVehicle) && Entity.FromNetworkId(incident.TrafficStopInvolvedVehicle) is Vehicle vehicle)
                {
                    this._incidentBlip = vehicle.CreateBlip(FBlipSprite.PersonalVehicleCar, FBlipColor.Blue, "Stopped vehicle");
                }
                break;
        }

        if (this._incidentBlip is not null)
        {
            this._incidentBlip.Name = incident.Title;
            this._incidentBlip.ShowRoute = true;
            var blipColor = incident.Priority switch
            {
                FPriority.Code1 => (BlipColor)FBlipColor.Blue,
                FPriority.Code2 => (BlipColor)FBlipColor.Yellow,
                FPriority.Code3 => (BlipColor)FBlipColor.Orange,
                FPriority.Code99 => (BlipColor)FBlipColor.Red,
                _ => (BlipColor)FBlipColor.White,
            };
            Cfx.API.SetBlipColour(this._incidentBlip.Handle, (int)blipColor);
        }
    }
}