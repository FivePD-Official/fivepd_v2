using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.DTO;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;
using FivePD.Server.QueryHelpers;

namespace FivePD.Server.Services.Callouts;

/// <inheritdoc cref="ICalloutManagerService" />
public class CalloutManagerService : ICalloutManagerService, IInternalCalloutManagerService, IServiceWithEvents
{
    private const int AutoDispatchCalloutTimeoutInMs = 10000;

    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickQueueService _tickQueueService;
    private readonly IProgressionSystemService _progressionSystemService;
    private readonly IConfigService _configService;
    private readonly IInternalDispatchService _dispatchService;
    private readonly IInternalCalloutStoreService _calloutStoreService;
    private readonly IAddonMetadataService _addonMetadataService;

    private readonly int _maxCalloutTimeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutManagerService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService"/> to use.</param>
    /// <param name="progressionSystemService">The <see cref="IProgressionSystemService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="dispatchService">The <see cref="IDispatchService"/> to use.</param>
    /// <param name="calloutStoreService">The <see cref="ICalloutStoreService"/> to use.</param>
    /// <param name="addonMetadataService">The <see cref="IAddonMetadataService"/> to use.</param>
    public CalloutManagerService(
        EventHandlerDictionary eventHandlerDictionary,
        ITickQueueService tickQueueService,
        IProgressionSystemService progressionSystemService,
        IConfigService configService,
        IDispatchService dispatchService,
        ICalloutStoreService calloutStoreService,
        IAddonMetadataService addonMetadataService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickQueueService = tickQueueService;
        this._progressionSystemService = progressionSystemService;
        this._configService = configService;
        this._dispatchService = (IInternalDispatchService)dispatchService;
        this._calloutStoreService = (IInternalCalloutStoreService)calloutStoreService;
        this._addonMetadataService = addonMetadataService;

        this._maxCalloutTimeout = configService.GeneralConfig.Callout.DispatchTypeAuto()
            ? AutoDispatchCalloutTimeoutInMs
            : this._configService.GeneralConfig.Callout.DispatchTimeoutInMs;
    }

    /// <inheritdoc/>
    public event EventHandler<Callout> OnCalloutCompleted;

    /// <inheritdoc/>
    List<CalloutHolder> IInternalCalloutManagerService.ActiveCallouts => this._calloutStoreService.ActiveCallouts;

    /// <inheritdoc />
    public List<Callout> ActiveCallouts()
    {
        return this._calloutStoreService.ActiveCallouts.Select(x => x.Callout).ToList();
    }

    /// <inheritdoc />
    public Callout GetPlayerCallout(FPlayer player)
    {
        return this.ActiveCallouts().FirstOrDefault(x => x.Units.Any(unit => unit.Players.Any(p => p.NetId == player.NetId)));
    }

    /// <inheritdoc />
    void IServiceWithEvents.RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Callout.OnPlayerEnteredInActivityZone] += new Action<Player, string>(this.OnPlayerEnteredInActivityZone);
        this._tickQueueService.EnqueueInterval(this.CheckCallouts, 1000);
    }

    /// <inheritdoc />
    public void CompleteCalloutOfPlayer(FPlayer player)
    {
        this.CompleteCallout(this.GetPlayerCallout(player));
    }

    private void CheckCallouts()
    {
        var calloutsToRemove = this._calloutStoreService.ActiveCallouts
            .Where(x => x.Timer.ElapsedMilliseconds >= this._maxCalloutTimeout && x.Callout.Units.Count == 0)
            .ToList();

        foreach (var callout in calloutsToRemove)
        {
            this._dispatchService.RemoveCalloutFromDispatchers(callout.Callout);
            this._calloutStoreService.ActiveCallouts.Remove(callout);
        }

        foreach (var calloutHolder in this._calloutStoreService.ActiveCallouts)
        {
            if (calloutHolder.Callout.Units.Count > 0)
            {
                calloutHolder.Timer.Reset();
            }
        }
    }

    private void CompleteCallout(Callout callout)
    {
        if (callout is null)
        {
            return;
        }

        callout.OnCompleted();
        this.OnCalloutCompleted?.Invoke(this, callout);
        foreach (var unit in callout.Units)
        {
            unit.Availability.Value = FUnitAvailability.Receiving;
            unit.Callout = null;
            ((IInternalUnit)unit).InternalCallout = null;
            this._progressionSystemService.AddExperiencePoints(unit, this._configService.GeneralConfig.ProgressionSystem.ExperiencePoints.CalloutCompleted);
            foreach (var player in unit.Players)
            {
                player.GetUnderlyingPlayer().TriggerEvent(Events.Callout.OnCompleted);
            }
        }

        this._calloutStoreService.ActiveCallouts.RemoveAll(x => x.Callout.Id == callout.Id);
    }

    private void OnPlayerEnteredInActivityZone([FromSource] Player player, string calloutId)
    {
        var callout = this._calloutStoreService.ActiveCallouts.FirstOrDefault(x => x.Callout.Id.ToString() == calloutId);
        if (callout is null)
        {
            return;
        }

        callout.Callout.OnPlayerEnteredInActivityZone(FPlayer.FromPlayer(player));
    }

    [UIEvent(Events.Callout.GetAllActiveUI)]
    private void GetAllActiveCalloutsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var activeCallouts = this._calloutStoreService.ActiveCallouts
            .Where(x => x.Callout.Units.Count > 0)
            .Select(x => new CalloutDTO.CalloutToClientDTO
            {
                Title = x.Callout.Title,
                Description = x.Callout.Description,
                Location = x.Callout.Location,
                Units = x.Callout.Units.Select(p => p.Callsign).ToList(),
            }).ToList();
        var page = new Page<CalloutDTO.CalloutToClientDTO>
        {
            Items = activeCallouts,
            CurrentPage = 1,
            ItemCount = activeCallouts.Count,
            PageCount = 1,
            PageSize = activeCallouts.Count,
        };
        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.Callout.GetAllTypes)]
    private void GetAllCalloutTypesUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var types = ((IInternalAddonMetadataService)this._addonMetadataService).CalloutTypes.Select(x => new CalloutDTO.CalloutTypeToUIDTO
        {
            TypeName = x.Type.Name,
            AssemblyName = x.AssemblyName,
        }).ToList();
        var page = new Page<CalloutDTO.CalloutTypeToUIDTO>
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