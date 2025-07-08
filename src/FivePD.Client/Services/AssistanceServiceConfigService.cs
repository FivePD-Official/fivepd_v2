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
    }
}