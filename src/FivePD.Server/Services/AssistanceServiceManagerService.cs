using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.AssistanceServiceModels;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class AssistanceServiceManagerService : IAssistanceServiceManagerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITickQueueService _tickQueueService;
    private readonly ILogger _logger;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;

    private readonly ConcurrentDictionary<Guid, AssistanceServiceInvoker> _assistanceServices = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AssistanceServiceManagerService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="randomService">The <see cref="IRandomService"/> to use.</param>
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

    /// <inheritdoc />
    public Guid Call<T>(FPlayer requestedBy, Action onCanceled = null)
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
                else if (type == typeof(Towing))
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

            var assistanceServiceInvoker = new AssistanceServiceInvoker(assistanceService, onCanceled, new CancellationTokenSource(), this._logger);
            this._assistanceServices.TryAdd(guid, assistanceServiceInvoker);
            this._tickQueueService.EnqueueAsync(assistanceServiceInvoker.InvokeAssistanceServiceMethodsAsync);

            return guid;
        }
        catch (Exception e)
        {
            this._logger.Error("Failed to call assistance service with type \"{Type}\". Exception: {Exception}", typeof(T), e.Message);
        }

        return Guid.Empty;
    }

    /// <inheritdoc />
    public void Cancel(ref Guid guid)
    {
        try
        {
            this._assistanceServices.TryRemove(guid, out var service);
            service?.CancellationTokenSource.Cancel();
            guid = Guid.Empty;
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
        private readonly ILogger _logger;
        private readonly Action _onCanceled;

        private readonly List<Func<Task>> _methodQueue = new();

        public AssistanceServiceInvoker(
            AssistanceService service,
            Action onCanceled,
            CancellationTokenSource cancellationTokenSource,
            ILogger logger)
        {
            this._service = service;
            this.CancellationTokenSource = cancellationTokenSource;
            this._logger = logger;
            this._onCanceled = onCanceled;

            this._methodQueue.Add(() => service.OnCalledAsync(cancellationTokenSource));
            this._methodQueue.Add(service.OnCompletedAsync);
        }

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        /// Invokes the methods of the assistance service.
        /// </summary>
        /// <exception cref="TaskCanceledException">Thrown when the <see cref="CancellationTokenSource"/> is canceled.</exception>
        public async Task InvokeAssistanceServiceMethodsAsync()
        {
            Func<Task> currentMethod = null;
            var canRunResult = false;
            try
            {
                if (this.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                await this._service.SetupAsync();

                if (this.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                canRunResult = await this._service.CanRunAsync();
                if (!canRunResult)
                {
                    throw new TaskCanceledException();
                }

                foreach (var method in this._methodQueue)
                {
                    currentMethod = method;
                    if (this.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }

                    await method();
                }

                throw new TaskCanceledException();
            }
            catch (TaskCanceledException)
            {
                try
                {
                    if (canRunResult)
                    {
#pragma warning disable CS4014
                        this._service.OnCancellationRequestedAsync();
#pragma warning restore CS4014
                    }

                    this._onCanceled?.Invoke();
                }
                catch (Exception e)
                {
                    this._logger.Error(
                        "Failed to invoke method \"{Method}\" of assistance service \"{Service}\". Exception: {Exception}",
                        nameof(this._service.OnCancellationRequestedAsync),
                        this._service.GetType(),
                        e);
                }
            }
            catch (Exception e)
            {
                this._logger.Error(
                    "Failed to invoke method \"{Method}\" of assistance service \"{Service}\". Cancelling service. Exception: {Exception}",
                    currentMethod?.Method.Name,
                    this._service.GetType(),
                    e);
            }
        }
    }
}