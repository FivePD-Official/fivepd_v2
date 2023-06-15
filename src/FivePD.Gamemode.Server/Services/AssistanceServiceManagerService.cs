using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models.AssistanceServiceModels;
using Serilog;

namespace FivePD.Gamemode.Server.Services;

#pragma warning disable

public class AssistanceServiceManagerService : IAssistanceServiceManagerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITickQueueService _tickQueueService;
    private readonly ILogger _logger;

    private readonly ConcurrentDictionary<Guid, AssistanceServiceInvoker> _assistanceServices = new();

    public AssistanceServiceManagerService(IServiceProvider serviceProvider, ITickQueueService tickQueueService, ILogger logger)
    {
        this._serviceProvider = serviceProvider;
        this._tickQueueService = tickQueueService;
        this._logger = logger;
    }

    public Guid Call<T>(Vector3 location)
        where T : AssistanceService
    {
        try
        {
            var guid = Guid.NewGuid();

            var assistanceService = this.CreateInstance<T>();
            ((IInternalAssistanceService)assistanceService).Location = location;
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

            this._methodQueue.Add(service.SpawnEntitiesAsync);
            this._methodQueue.Add(service.GoToLocationAsync);
            this._methodQueue.Add(service.PerformActionAsync);
            this._methodQueue.Add(service.ActionCompletedOrCanceledAsync);
        }
    
        public async Task InvokeAssistanceServiceMethods()
        {
            foreach (var method in this._methodQueue)
            {
                if (this.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    this._service.ActionCompletedOrCanceledAsync();
                    return;
                }

                await method();
            }
        }
    }
}