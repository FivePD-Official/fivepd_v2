using System.Linq;
using FivePD.Client.DTO;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Dispatcher;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using Newtonsoft.Json;

namespace FivePD.Server.Services.Dispatcher;

#pragma warning disable

[Obfuscate]
public class DispatcherManagerService : IDispatcherManagerService, IInternalDispatcherManagerService, IOnInit
{
    private readonly IDispatcherStorageService _dispatcherStorageService;
    private readonly IUnitManagerService _unitManagerService;

    public DispatcherManagerService(
        IDispatcherStorageService dispatcherStorageService,
        IUnitManagerService unitManagerService)
    {
        this._dispatcherStorageService = dispatcherStorageService;
        this._unitManagerService = unitManagerService;
    }

    void IOnInit.OnInit()
    {
        this._unitManagerService.OnUnitCreated += this.OnUnitCreated;
        this._unitManagerService.OnUnitRemoved += this.OnUnitRemoved;
    }

    void IInternalDispatcherManagerService.SendIncidentToDispatchers(Callout incident)
    {
        foreach (var dispatcher in this._dispatcherStorageService.Dispatchers())
        {
            dispatcher.GetUnderlyingPlayer().TriggerEvent(
                Events.Dispatch.SendCalloutToDispatcher,
                JsonConvert.SerializeObject(incident.ToCalloutForDispatcherDTO()));
        }
    }

    void IInternalDispatcherManagerService.RemoveIncidentFromDispatchers(IIncident incident)
    {
        foreach (var dispatcher in this._dispatcherStorageService.Dispatchers())
        {
            dispatcher.GetUnderlyingPlayer().TriggerEvent(
                Events.Dispatch.RemoveCalloutFromDispatcher,
                JsonConvert.SerializeObject(new DispatcherModeDTOs.RemoveCalloutClient(incident.Id.ToString())));
        }
    }

    void IInternalDispatcherManagerService.SendUpdateRequestToDispatchers()
    {
        foreach (var dispatcher in this._dispatcherStorageService.Dispatchers())
        {
            dispatcher.GetUnderlyingPlayer().TriggerEvent(Events.Dispatch.UpdateRequest, JsonConvert.SerializeObject(new DispatcherModeDTOs.UpdateRequest()));
        }
    }

    private void OnUnitCreated(object _, Unit unit)
    {
        foreach (var dispatcher in this._dispatcherStorageService.Dispatchers())
        {
            dispatcher.GetUnderlyingPlayer().TriggerEvent(Events.Dispatch.OnUnitCreated, JsonConvert.SerializeObject(new DispatcherModeDTOs.Unit
            {
                Callsign = unit.Callsign,
                Players = unit.Players.Select(player => new SimplePlayerDTO(player.NetId, player.GetUnderlyingPlayer().Name)),
                Availability = unit.Availability.Value,
                Status = unit.Status.Value,
            }));
        }
    }

    private void OnUnitRemoved(object _, Unit unit)
    {
        foreach (var dispatcher in this._dispatcherStorageService.Dispatchers())
        {
            dispatcher.GetUnderlyingPlayer().TriggerEvent(Events.Dispatch.OnUnitRemoved, new DispatcherModeDTOs.RemoveUnitClient(unit.Callsign));
        }
    }
}