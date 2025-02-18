using System;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class DispatchService : IDispatchService, IInternalDispatchService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IInternalIncidentService _incidentService;
    private readonly ICalloutDistributionService _calloutDistributionService;
    private readonly IUnitService _unitService;

    public DispatchService(
        EventHandlerDictionary eventHandlerDictionary,
        IIncidentService incidentService,
        ICalloutDistributionService calloutDistributionService,
        IUnitService unitService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._incidentService = (IInternalIncidentService)incidentService;
        this._calloutDistributionService = calloutDistributionService;
        this._unitService = unitService;
    }

    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Dispatch.EnableForPlayer] += new Action<Player>(this.OnEnableForPlayer);
        this._eventHandlerDictionary[Events.Dispatch.DisableForPlayer] += new Action<Player>(this.OnDisableForPlayer);
    }

    [UIEvent(Events.UI.Dispatch.GetCallouts)]
    private void GetCalloutsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<GetCalloutsDispatcherRequest>();
        networkCallbackDelegate.InvokeWithJson(this._incidentService.InternalCallouts
            .Where(callout => request.HasUnitsAssigned ? callout.Callout.Units.Count > 0 : callout.Callout.Units.Count == 0)
            .Select(x => x.Callout.ToCalloutForDispatcherDTO()).ToList());
    }

    [UIEvent(Events.UI.Dispatch.GetUnits)]
    private void GetUnitsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var units = this._unitService.Units().Select(unit => new DispatcherUnitDTO
        {
            Callsign = unit.Callsign,
            Players = unit.Players.Select(player => new SimplePlayerDTO(player.NetId, player.GetUnderlyingPlayer().Name)),
            Availability = unit.Availability.Value,
            Status = unit.Status.Value,
            AssignedCallout = unit.Callout is null ? null : new SimpleCallout(unit.InternalCallout),
        }).ToList();
        networkCallbackDelegate.InvokeWithJson(units);
    }

    [UIEvent(Events.Dispatch.AssignCalloutToUnits)]
    private void AssignCalloutToUnitsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<AssignCalloutToUnitsRequest>();
        var internalCallout = this._incidentService.InternalCallouts.FirstOrDefault(x => x.Callout.Id == Guid.Parse(request.CalloutId));
        if (internalCallout is null)
        {
            return;
        }

        foreach (var unit in this._unitService.Units().Where(unit => request.UnitCallsigns.Contains(unit.Callsign)))
        {
            this._calloutDistributionService.AssignUnitToCallout(unit, internalCallout.Callout);
        }

        // foreach (var dispatcher in this.Dispatchers())
        // {
        //     dispatcher.GetUnderlyingPlayer().TriggerEvent(Events.Dispatch.OnCalloutAssignedToUnits, JsonConvert.SerializeObject(callout.Callout.ToCalloutForDispatcherDTO()));
        // }

        networkCallbackDelegate.Invoke(null);
    }

    private void OnEnableForPlayer([FromSource] Player player)
    {
        var fPlayer = FPlayer.FromPlayer(player);
        fPlayer.Dispatcher.Value = true;
    }

    private void OnDisableForPlayer([FromSource] Player player)
    {
        var fPlayer = FPlayer.FromPlayer(player);
        fPlayer.Dispatcher.Value = false;
    }
}