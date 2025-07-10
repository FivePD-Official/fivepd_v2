using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

#pragma warning disable

public class AssistanceServiceConfigService : IAssistanceServiceConfigService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    public AssistanceServiceConfigService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.GetTypeConfig);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.GetTypeConfigPage);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.EditTypeConfig);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.GetStation);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.GetStationPage);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.CreateStation);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.DeleteStation);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.GetStationTypeConfigsPage);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.GetAvailableStationTypeConfigs);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.GetStationTypeConfig);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.AddStationTypeConfig);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.EditStationTypeConfig);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AssistanceService.DeleteStationTypeConfig);
    }
}