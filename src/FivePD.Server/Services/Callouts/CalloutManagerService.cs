using System;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.DTO;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.QueryHelpers;

namespace FivePD.Server.Services.Callouts;

/// <inheritdoc cref="ICalloutManagerService" />
public class CalloutManagerService : ICalloutManagerService, IOnInit
{
    private const int AutoDispatchCalloutTimeoutInMs = 10000;

    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickQueueService _tickQueueService;
    private readonly IInternalDispatchService _dispatchService;
    private readonly IInternalIncidentService _incidentService;
    private readonly IAddonMetadataService _addonMetadataService;

    private readonly int _maxCalloutTimeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutManagerService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="dispatchService">The <see cref="IDispatchService"/> to use.</param>
    /// <param name="incidentService">The <see cref="IIncidentService"/> to use.</param>
    /// <param name="addonMetadataService">The <see cref="IAddonMetadataService"/> to use.</param>
    public CalloutManagerService(
        EventHandlerDictionary eventHandlerDictionary,
        ITickQueueService tickQueueService,
        IConfigService configService,
        IDispatchService dispatchService,
        IIncidentService incidentService,
        IAddonMetadataService addonMetadataService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickQueueService = tickQueueService;
        this._dispatchService = (IInternalDispatchService)dispatchService;
        this._incidentService = (IInternalIncidentService)incidentService;
        this._addonMetadataService = addonMetadataService;

        this._maxCalloutTimeout = configService.GeneralConfig.Callout.DispatchTypeAuto()
            ? AutoDispatchCalloutTimeoutInMs
            : configService.GeneralConfig.Callout.DispatchTimeoutInMs;
    }

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Callout.OnPlayerEnteredInActivityZone] += new Action<Player, string>(this.OnPlayerEnteredInActivityZone);
        this._tickQueueService.EnqueueInterval(this.CheckCallouts, 1000);
    }

    private void CheckCallouts()
    {
        var calloutsToRemove = this._incidentService.InternalCallouts
            .Where(x => x.Timer.ElapsedMilliseconds >= this._maxCalloutTimeout && x.Callout.Units.Count == 0)
            .ToList();

        foreach (var callout in calloutsToRemove)
        {
            this._dispatchService.RemoveCalloutFromDispatchers(callout.Callout);
            this._incidentService.InternalCallouts.Remove(callout);
        }

        foreach (var calloutHolder in this._incidentService.InternalCallouts)
        {
            if (calloutHolder.Callout.Units.Count > 0)
            {
                calloutHolder.Timer.Reset();
            }
        }
    }

    private void OnPlayerEnteredInActivityZone([FromSource] Player player, string calloutId)
    {
        var callout = this._incidentService.InternalCallouts.FirstOrDefault(x => x.Callout.Id.ToString() == calloutId);
        if (callout is null)
        {
            return;
        }

        callout.Callout.OnPlayerEnteredInActivityZone(FPlayer.FromPlayer(player));
    }

    [UIEvent(Events.UI.Callout.GetAllTypes)]
    private void GetAllCalloutTypesUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var types = ((IInternalAddonMetadataService)this._addonMetadataService).CalloutTypes.Select(x => new IncidentDTOs.CalloutTypeToUIDTO
        {
            TypeName = x.Type.Name,
            AssemblyName = x.AssemblyName,
        }).ToList();
        var page = new Page<IncidentDTOs.CalloutTypeToUIDTO>
        {
            Items = types,
            CurrentPage = 1,
            ItemCount = types.Count,
            PageCount = 1,
            PageSize = types.Count,
        };
        networkCallbackDelegate.InvokeWithJson(page);
    }
}