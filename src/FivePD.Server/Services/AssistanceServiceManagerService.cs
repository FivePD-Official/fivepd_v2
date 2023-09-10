using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.AssistanceServiceModels;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

public class AssistanceServiceManagerService : IAssistanceServiceManagerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITickQueueService _tickQueueService;
    private readonly ILogger _logger;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;

    private readonly ConcurrentDictionary<Guid, AssistanceServiceInvoker> _assistanceServices = new();

    public AssistanceServiceManagerService(
        IServiceProvider serviceProvider,
        ITickQueueService tickQueueService,
        ILogger logger,
        IConfigService configService,
        IRandomService randomService)
    {
        this._serviceProvider = serviceProvider;
        this._tickQueueService = tickQueueService;
        this._logger = logger;
        this._configService = configService;
        this._randomService = randomService;
    }

    public Guid Call<T>(FPlayer requestedBy)
        where T : AssistanceService
    {
        try
        {
            var guid = Guid.NewGuid();

            var assistanceService = this.CreateInstance<T>();
            ((IInternalAssistanceService)assistanceService).RequestedBy = requestedBy;

            if (this._configService.GeneralConfig.AssistanceServices.SpawnTypeAuto())
            {
                ((IInternalAssistanceService)assistanceService).SpawnLocation = new Vector3(704.09f, -117.47f, 52.83f); // TODO: get random location
            }
            else
            {
                List<Vector3> locations = new();
                var type = typeof(T);
                if (type == typeof(Ambulance))
                {
                    locations = this._configService.GeneralConfig.AssistanceServices.SpawnLocations.Ambulance;
                }
                else if (type == typeof(TowTruck))
                {
                    locations = this._configService.GeneralConfig.AssistanceServices.SpawnLocations.TowTruck;
                }

                if (locations.Count == 0)
                {
                    this._logger.Error("Failed to call assistance service with type \"{Type}\". No locations found.", typeof(T));
                    return Guid.Empty;
                }

                ((IInternalAssistanceService)assistanceService).SpawnLocation = locations[this._randomService.Random.Next(0, locations.Count)];
            }
            
            var assistanceServiceInvoker = new AssistanceServiceInvoker(assistanceService, new CancellationTokenSource());
            this._assistanceServices.TryAdd(guid, assistanceServiceInvoker);
            this._tickQueueService.EnqueueAsync(assistanceServiceInvoker.InvokeAssistanceServiceMethods);

            return guid;
        }
        catch (Exception e)
        {
            this._logger.Error("Failed to call assistance service with type \"{Type}\". Exception: {Exception}", typeof(T), e.Message);
        }

        return Guid.Empty;
    }

    public void Cancel(Guid guid)
    {
        try
        {
            this._assistanceServices.TryRemove(guid, out var service);
            service?.CancellationTokenSource.Cancel();
        }
        catch (Exception e)
        {
            this._logger.Error("Failed to cancel assistance service with guid \"{Guid}\". Exception: {Exception}", guid, e.Message);
        }
    }

    private T CreateInstance<T>(Type type = null)
        where T : AssistanceService
    {
        type ??= typeof(T);
        var parameters = type.GetConstructors()[0].GetParameters();
        return (T)Activator.CreateInstance(
            type,
            parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray());
    }

    private sealed record AssistanceServiceInvoker
    {
        private readonly AssistanceService _service;

        private readonly List<Func<Task>> _methodQueue = new();

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public AssistanceServiceInvoker(AssistanceService service, CancellationTokenSource cancellationTokenSource)
        {
            this._service = service;
            this.CancellationTokenSource = cancellationTokenSource;

            this._methodQueue.Add(service.Setup);
            this._methodQueue.Add(() => service.OnCalledAsync(cancellationTokenSource));
            this._methodQueue.Add(service.OnCompletedAsync);
        }

        public async Task InvokeAssistanceServiceMethods()
        {
            foreach (var method in this._methodQueue)
            {
                if (this.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    this._service.OnCancellationRequestedAsync();
                    return;
                }

                await method();
            }
        }
    }
}