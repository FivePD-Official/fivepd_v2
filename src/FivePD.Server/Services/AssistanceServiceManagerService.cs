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
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models.AssistanceServiceModels;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IAssistanceServiceManagerService" />
[Obfuscate]
public class AssistanceServiceManagerService : IAssistanceServiceManagerService, IOnInit
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITickQueueService _tickQueueService;
    private readonly ILogger _logger;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly IVector3Service _vector3Service;

    private readonly ConcurrentDictionary<Type, AssistanceServiceOptions> _availableServices = new();
    private readonly ConcurrentDictionary<Guid, AssistanceServiceInvoker> _activeServices = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AssistanceServiceManagerService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="randomService">The <see cref="IRandomService"/> to use.</param>
    /// <param name="vector3Service">The <see cref="IVector3Service"/> to use.</param>
    public AssistanceServiceManagerService(
        IServiceProvider serviceProvider,
        ITickQueueService tickQueueService,
        ILogger logger,
        IConfigService configService,
        IRandomService randomService,
        IVector3Service vector3Service)
    {
        this._serviceProvider = serviceProvider;
        this._tickQueueService = tickQueueService;
        this._logger = logger;
        this._configService = configService;
        this._randomService = randomService;
        this._vector3Service = vector3Service;
    }

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        var config = this._configService.GeneralConfig.AssistanceServices;
        if (config.Ambulance.Enabled)
        {
            this.Register<Ambulance>(new AssistanceServiceOptions
            {
                RequestLabel = "Request ambulance",
                CancelLabel = "Cancel ambulance",
                SpawnLocations = config.Ambulance.SpawnLocations,
            });
        }

        if (config.TowTruck.Enabled)
        {
            this.Register<Towing>(new AssistanceServiceOptions
            {
                RequestLabel = "Request tow",
                CancelLabel = "Cancel tow",
                SpawnLocations = config.TowTruck.SpawnLocations,
            });
        }
    }

    /// <inheritdoc />
    public void Register<T>(AssistanceServiceOptions options)
        where T : AssistanceService
    {
        var type = typeof(T);
        if (this._availableServices.ContainsKey(type))
        {
            this._logger.Error(
                "Can not register assistance service with type \"{Type}\". An assistance service with the same type has been already registered.", type);
            return;
        }

        if (string.IsNullOrEmpty(options.RequestLabel))
        {
            this._logger.Error("Can not register assistance service with type \"{Type}\". The RequestLabel is null or empty.", type);
            return;
        }

        if (string.IsNullOrEmpty(options.CancelLabel))
        {
            this._logger.Error("Can not register assistance service with type \"{Type}\". The CancelLabel is null or empty.", type);
            return;
        }

        this._logger.Information("Registering an assistance service of type \"{Type}\"", type);

        this._availableServices[type] = options;
    }

    /// <inheritdoc />
    public List<(Type Type, AssistanceServiceOptions Options)> AvailableServices()
    {
        return this._availableServices.Select(x => (x.Key, x.Value)).ToList();
    }

    /// <inheritdoc />
    public Task<Guid> CallAsync<T>(FPlayer requestedBy, Action onCanceled = null)
        where T : AssistanceService
    {
        return this.CallAsync(typeof(T), requestedBy, onCanceled);
    }

    /// <inheritdoc />
    public async Task<Guid> CallAsync(Type type, FPlayer requestedBy, Action onCanceled = null)
    {
        if (!type.IsClass || type.IsAbstract || !type.IsSubclassOf(typeof(AssistanceService)))
        {
            this._logger.Error(
                "Failed to call assistance service with type \"{Type}\". This type is not a subclass of \"{Type2}\".",
                type,
                typeof(AssistanceService));
            return Guid.Empty;
        }

        if (!this._availableServices.TryGetValue(type, out var options))
        {
            this._logger.Error("Failed to call assistance service with type \"{Type}\". This type has not been registered.", type);
            return Guid.Empty;
        }

        try
        {
            var guid = Guid.NewGuid();

            var assistanceService = this.CreateInstance(type);
            ((IInternalAssistanceService)assistanceService).RequestedBy = requestedBy;

            async Task SetRandomLocation()
            {
                var pointOnRoadSide = await this._vector3Service.GetPointOnRoadSideAsync(requestedBy.GetUnderlyingPlayer().Character, 800, 1600);
                ((IInternalAssistanceService)assistanceService).SpawnLocation = pointOnRoadSide.Location;
                ((IInternalAssistanceService)assistanceService).SpawnHeading = pointOnRoadSide.Heading;
            }

            if (this._configService.GeneralConfig.AssistanceServices.SpawnTypeAuto())
            {
                await SetRandomLocation();
            }
            else
            {
                var locations = options.SpawnLocations ?? new List<Vector3>();
                if (locations.Count == 0)
                {
                    await SetRandomLocation();
                }
                else
                {
                    ((IInternalAssistanceService)assistanceService).SpawnLocation = locations[this._randomService.Random.Next(0, locations.Count)];
                    ((IInternalAssistanceService)assistanceService).SpawnHeading = options.SpawnHeading;
                }
            }

            var assistanceServiceInvoker = new AssistanceServiceInvoker(assistanceService, onCanceled, new CancellationTokenSource(), this._logger);
            this._activeServices.TryAdd(guid, assistanceServiceInvoker);
#pragma warning disable CS4014
            this._tickQueueService.EnqueueAsync(assistanceServiceInvoker.InvokeAssistanceServiceMethodsAsync);
#pragma warning restore CS4014

            return guid;
        }
        catch (Exception e)
        {
            this._logger.Error("Failed to call assistance service with type \"{Type}\". Exception: {Exception}", type, e.Message);
        }

        return Guid.Empty;
    }

    /// <inheritdoc />
    public void Cancel(ref Guid guid)
    {
        try
        {
            this._activeServices.TryRemove(guid, out var service);
            service?.CancellationTokenSource.Cancel();
            guid = Guid.Empty;
        }
        catch (Exception e)
        {
            this._logger.Error("Failed to cancel assistance service with guid \"{Guid}\". Exception: {Exception}", guid, e.Message);
        }
    }

    private AssistanceService CreateInstance(Type type)
    {
        if (type is null)
        {
            return null;
        }

        var parameters = type.GetConstructors()[0].GetParameters();
        return Activator.CreateInstance(
            type,
            parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray()) as AssistanceService;
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