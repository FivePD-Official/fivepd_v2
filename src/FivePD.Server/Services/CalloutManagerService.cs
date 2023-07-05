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
using FivePD.Server.QueryHelpers;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.ICalloutManagerService" />
public class CalloutManagerService : ICalloutManagerService, IInternalCalloutManagerService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickQueueService _tickQueueService;

    private readonly List<CalloutType> _calloutTypes = new();
    private readonly List<CalloutHolder> _activeCallouts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutManagerService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService"/> to use.</param>
    public CalloutManagerService(
        EventHandlerDictionary eventHandlerDictionary,
        ITickQueueService tickQueueService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickQueueService = tickQueueService;
    }

    /// <inheritdoc/>
    public event EventHandler<Callout> OnCalloutCompleted;

    /// <inheritdoc/>
    List<CalloutHolder> IInternalCalloutManagerService.ActiveCallouts => this._activeCallouts;

    /// <inheritdoc/>
    List<Type> IInternalCalloutManagerService.CalloutTypes()
    {
        return this._calloutTypes.Select(x => x.Type).ToList();
    }

    /// <inheritdoc />
    public List<Callout> ActiveCallouts()
    {
        return this._activeCallouts.Select(x => x.Callout).ToList();
    }

    /// <inheritdoc />
    public Callout GetPlayerCallout(FPlayer player)
    {
        return this.ActiveCallouts().FirstOrDefault(x => x.Units.Any(unit => unit.Players.Any(p => p.NetId == player.NetId)));
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Callout.OnPlayerEnteredInActivityZone] += new Action<Player, string>(this.OnPlayerEnteredInActivityZone);
        this.RegisterCallouts();
        this._tickQueueService.EnqueueInterval(this.CheckCallouts, 1000);
    }

    /// <inheritdoc />
    public void CompleteCalloutOfPlayer(FPlayer player)
    {
        this.CompleteCallout(this.GetPlayerCallout(player));
    }

    private void RegisterCallouts()
    {
        var callouts = (IEnumerable<Callout>)ServiceLocator.GetServices(typeof(Callout));
        foreach (var callout in callouts)
        {
            var type = callout.GetType();
            this._calloutTypes.Add(new CalloutType(type, type.Assembly.GetName().Name));
        }
    }

    private void CheckCallouts()
    {
        this._activeCallouts.RemoveAll(x => x.Timer.ElapsedMilliseconds >= 10000 && x.Callout.Units.Count == 0);
        foreach (var calloutHolder in this._activeCallouts)
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
            foreach (var player in unit.Players)
            {
                player.Availability.Value = FPlayerAvailability.Receiving;
                player.GetUnderlyingPlayer().TriggerEvent(Events.Callout.OnCompleted);
            }
        }

        this._activeCallouts.RemoveAll(x => x.Callout.Id == callout.Id);
    }

    private void OnPlayerEnteredInActivityZone([FromSource] Player player, string calloutId)
    {
        var callout = this._activeCallouts.FirstOrDefault(x => x.Callout.Id.ToString() == calloutId);
        if (callout is null)
        {
            return;
        }

        callout.Callout.OnPlayerEnteredInActivityZone(FPlayer.FromPlayer(player));
    }

    [UIEvent(Events.Callout.GetAllActiveUI)]
    private void GetAllActiveCalloutsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var activeCallouts = this._activeCallouts
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
        var types = this._calloutTypes.Select(x => new CalloutDTO.CalloutTypeToUIDTO
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