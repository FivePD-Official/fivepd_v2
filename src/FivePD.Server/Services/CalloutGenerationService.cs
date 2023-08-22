using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.Attributes;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.Requests;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

internal class CalloutGenerationService : ICalloutGenerationService, IServiceWithEvents
{
    private readonly IInternalAddonMetadataService _addonMetadataService;
    private readonly IInternalCalloutManagerService _calloutManagerService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly ILogger _logger;
    private readonly IInternalTickQueueService _tickQueueService;
    private readonly ICalloutDistributionService _calloutDistributionService;
    
    private readonly Stopwatch _nextCalloutTimer = new();
    private int _nextCalloutInMs;
    
    public CalloutGenerationService(
        IInternalAddonMetadataService addonMetadataService,
        IInternalCalloutManagerService calloutManagerService,
        IConfigService configService,
        IRandomService randomService,
        ILogger logger,
        IInternalTickQueueService tickQueueService,
        ICalloutDistributionService calloutDistributionService)
    {
        this._addonMetadataService = addonMetadataService;
        this._calloutManagerService = calloutManagerService;
        this._configService = configService;
        this._randomService = randomService;
        this._logger = logger;
        this._tickQueueService = tickQueueService;
        this._calloutDistributionService = calloutDistributionService;
    }
    
    /// <inheritdoc/>
    public void RegisterEvents()
    {
        this._nextCalloutInMs = this._randomService.Random.Next(
            this._configService.GeneralConfig.Callout.MinIntervalInMs,
            this._configService.GeneralConfig.Callout.MaxIntervalInMs);
        this._nextCalloutTimer.Start();
        this._tickQueueService.OnTick += this.OnTick;
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (this._configService.GeneralConfig.Callout.MaxActiveCallouts > 0 &&
            this._calloutManagerService.ActiveCallouts.Count >= this._configService.GeneralConfig.Callout.MaxActiveCallouts)
        {
            return;
        }

        if (this._nextCalloutTimer.ElapsedMilliseconds < this._nextCalloutInMs)
        {
            return;
        }

        this.GenerateCallout();

        this._nextCalloutInMs = this._randomService.Random.Next(
            this._configService.GeneralConfig.Callout.MinIntervalInMs,
            this._configService.GeneralConfig.Callout.MaxIntervalInMs);
        this._nextCalloutTimer.Restart();
    }

    private void GenerateCallout()
    {
        var calloutTypes = this._addonMetadataService.CalloutTypes;
        var calloutType = calloutTypes[this._randomService.Random.Next(0, calloutTypes.Count)];
        this.GenerateCalloutOfTypeAsync(calloutType.Type);
    }
    
    [UIEvent(Events.UI.Callout.Dispatch)]
    private void DispatchCalloutUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<DispatchCalloutRequest>();
        var calloutType = this._addonMetadataService.CalloutTypes.FirstOrDefault(x => x.Type.Name == request.TypeName);
        if (calloutType is null)
        {
            networkCallbackDelegate.Invoke(null);
            return;
        }

#pragma warning disable CS4014
        this.GenerateCalloutOfTypeAsync(calloutType.Type, false);
#pragma warning restore CS4014

        networkCallbackDelegate.Invoke(null);
    }
    
    private async Task GenerateCalloutOfTypeAsync(Type type, bool invokeCanBeDispatch = true)
    {
        var callout = this._addonMetadataService.InstantiateCallout(type);
        if (callout is null)
        {
            this._logger.Warning("A callout with the type of \"{Type}\" could not be instantiated.", type);
            return;
        }

        var properties = await callout.Setup();
        callout.Id = Guid.NewGuid();
        callout.Title = properties.Title;
        callout.Description = properties.Description;
        callout.ActivityRadius = properties.ActivityRadius;
        callout.GeneratedAt = DateTime.Now;
        if (properties.RandomPriority)
        {
#pragma warning disable
            // TEST DISPATCH
            // callout.Priority = this._calloutManagerService.ActiveCallouts.Count switch
            // {
            //     0 => FIncidentPriority.Code2,
            //     1 => FIncidentPriority.Code3,
            //     _ => FIncidentPriority.Code1
            // };
#pragma warning restore
            callout.Priority = (FIncidentPriority)this._randomService.Random.Next(0, 3);
        }
        else
        {
            callout.Priority = properties.Priority;
        }

        var activeCalloutsWithSameType = this._calloutManagerService.ActiveCallouts
            .Where(x => x.Type == type)
            .ToList();
        if (properties.Locations is not null)
        {
            var locations = properties.Locations
                .Where(l => activeCalloutsWithSameType.All(c => c.Callout.Location != l))
                .ToList();
            if (locations.Count > 0)
            {
                callout.Location = locations[this._randomService.Random.Next(0, locations.Count)];
            }
            else
            {
                return;
            }
        }
        else
        {
            if (activeCalloutsWithSameType.Any(c => c.Callout.Location == properties.Location))
            {
                return;
            }

            callout.Location = properties.Location;
        }

        if (invokeCanBeDispatch)
        {
            var canBeDispatched = await callout.CanBeDispatched();
            if (!canBeDispatched)
            {
                return;
            }
        }

        var calloutHolder = new CalloutHolder(callout, type, new Stopwatch());
        this._calloutManagerService.ActiveCallouts.Add(calloutHolder);
        calloutHolder.Timer.Start();

        ((IInternalCalloutDistributionService)this._calloutDistributionService).DistributeCallout(calloutHolder);
    }
}