using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.AssistanceServiceModels;
using FivePD.Server.Repositories;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IAssistanceServiceManagerService" />
[Obfuscate]
public class AssistanceServiceManagerService : IAssistanceServiceManagerService, IOnInit
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITickQueueService _tickQueueService;
    private readonly ILogger _logger;
    private readonly IRandomService _randomService;
    private readonly IVector3Service _vector3Service;
    private readonly IAssistanceServiceConfigService _assistanceServiceConfigService;
    private readonly AssistanceServiceStationRepository _assistanceServiceStationRepository;
    private readonly AssistanceServiceStationTypeConfigRepository _assistanceServiceStationTypeConfigRepository;
    private readonly AssistanceServiceTypeConfigRepository _assistanceServiceTypeConfigRepository;

    private readonly ConcurrentDictionary<Guid, AssistanceServiceInvoker> _activeServices = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AssistanceServiceManagerService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="randomService">The <see cref="IRandomService"/> to use.</param>
    /// <param name="vector3Service">The <see cref="IVector3Service"/> to use.</param>
    /// <param name="assistanceServiceConfigService">The <see cref="IAssistanceServiceConfigService"/> to use.</param>
    /// <param name="assistanceServiceStationRepository">The <see cref="AssistanceServiceStationRepository"/> to use.</param>
    /// <param name="assistanceServiceStationTypeConfigRepository">The <see cref="AssistanceServiceStationTypeConfigRepository"/> to use.</param>
    /// <param name="assistanceServiceTypeConfigRepository">The <see cref="AssistanceServiceTypeConfigRepository"/> to use.</param>
    public AssistanceServiceManagerService(
        IServiceProvider serviceProvider,
        ITickQueueService tickQueueService,
        ILogger logger,
        IRandomService randomService,
        IVector3Service vector3Service,
        IAssistanceServiceConfigService assistanceServiceConfigService,
        AssistanceServiceStationRepository assistanceServiceStationRepository,
        AssistanceServiceStationTypeConfigRepository assistanceServiceStationTypeConfigRepository,
        AssistanceServiceTypeConfigRepository assistanceServiceTypeConfigRepository)
    {
        this._serviceProvider = serviceProvider;
        this._tickQueueService = tickQueueService;
        this._logger = logger;
        this._randomService = randomService;
        this._vector3Service = vector3Service;
        this._assistanceServiceConfigService = assistanceServiceConfigService;
        this._assistanceServiceStationRepository = assistanceServiceStationRepository;
        this._assistanceServiceStationTypeConfigRepository = assistanceServiceStationTypeConfigRepository;
        this._assistanceServiceTypeConfigRepository = assistanceServiceTypeConfigRepository;
    }

    /// <inheritdoc />
    public IReadOnlyList<AvailableAssistanceService> AvailableServices =>
        this._assistanceServiceConfigService.AvailableServices;

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        this._assistanceServiceConfigService.Register<Ambulance>(new AssistanceServiceOptions
        {
            RequestLabel = "LABEL.ASSISTANCE_SERVICE.AMBULANCE.REQUEST",
            CancelLabel = "LABEL.ASSISTANCE_SERVICE.AMBULANCE.CANCEL",
        });

        this._assistanceServiceConfigService.Register<Coroner>(new AssistanceServiceOptions
        {
            RequestLabel = "LABEL.ASSISTANCE_SERVICE.CORONER.REQUEST",
            CancelLabel = "LABEL.ASSISTANCE_SERVICE.CORONER.CANCEL",
        });

        this._assistanceServiceConfigService.Register<Towing>(new AssistanceServiceOptions
        {
            RequestLabel = "LABEL.ASSISTANCE_SERVICE.TOWING.REQUEST",
            CancelLabel = "LABEL.ASSISTANCE_SERVICE.TOWING.CANCEL",
        });

        this._assistanceServiceConfigService.Register<RideShare>(new AssistanceServiceOptions
        {
            RequestLabel = "LABEL.ASSISTANCE_SERVICE.RIDE_SHARE.REQUEST",
            CancelLabel = "LABEL.ASSISTANCE_SERVICE.RIDE_SHARE.CANCEL",
        });
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

        var availableService = this._assistanceServiceConfigService.AvailableServices.FirstOrDefault(x => x.Type == type);
        if (availableService is null)
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

            var typeName = type.Name;
            var assemblyName = type.Assembly.GetName().Name;

            var assistanceServiceVehicleConfig = this._assistanceServiceTypeConfigRepository.GetByTypeNameAndAssembly(typeName, assemblyName);
            if (assistanceServiceVehicleConfig is null)
            {
                this._logger.Error("Could not find database entry for assistance service with type \"{Type}\".", type);
                return Guid.Empty;
            }

            AssistanceServiceStation selectedStation = null;
            AssistanceServiceStationTypeConfig selectedStationTypeConfig = null;

            if (assistanceServiceVehicleConfig.SpawnTypeManual())
            {
                var locations = assistanceServiceVehicleConfig.ManualSpawnLocations ?? new List<Vector3>();
                if (locations.Count == 0)
                {
                    await SetRandomLocation();
                }
                else
                {
                    ((IInternalAssistanceService)assistanceService).SpawnLocation = locations[this._randomService.Random.Next(0, locations.Count)];
                    ((IInternalAssistanceService)assistanceService).SpawnHeading = 0f; // TODO
                }
            }
            else if (assistanceServiceVehicleConfig.SpawnTypeAuto())
            {
                await SetRandomLocation();
            }
            else if (assistanceServiceVehicleConfig.SpawnTypeStation())
            {
                var stations = this._assistanceServiceStationRepository.GetAllWithDynamicRelated(
                    new AssistanceServiceStationRepository.AssistanceServiceStationLoadOptions { IncludeVehicleTypeConfigs = true });
                var orderedStations = stations.OrderBy(s => s.Position.DistanceToSquared(requestedBy.GetUnderlyingPlayer().Character.Position));

                foreach (var station in orderedStations)
                {
                    var serviceConfig = station.TypeConfigs.FirstOrDefault(c =>
                        c.TypeConfig.TypeName == typeName &&
                        c.TypeConfig.AssemblyName == assemblyName &&
                        c.TypeConfig.Active &&
                        c.TypeConfig.Enabled &&
                        c.ActiveCapacity < c.Capacity);

                    if (serviceConfig is not null)
                    {
                        selectedStation = station;
                        selectedStationTypeConfig = serviceConfig;
                        break;
                    }
                }

                if (selectedStation is null)
                {
                    await SetRandomLocation();
                }
                else
                {
                    selectedStationTypeConfig.ActiveCapacity++;
                    this._assistanceServiceStationTypeConfigRepository.Update(
                        selectedStationTypeConfig,
                        x => { x.ActiveCapacity = selectedStationTypeConfig.ActiveCapacity; });

                    ((IInternalAssistanceService)assistanceService).SpawnLocation = selectedStation.Position;
                    ((IInternalAssistanceService)assistanceService).SpawnHeading = 0f; // TODO
                }
            }

            void LocalOnCanceled()
            {
                if (onCanceled is not null)
                {
                    onCanceled();
                }

                this.OnStationVehicleCanceled(guid);
            }

            var assistanceServiceInvoker = new AssistanceServiceInvoker(
                assistanceService,
                LocalOnCanceled,
                new CancellationTokenSource(),
                this._logger,
                selectedStationTypeConfig);
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
            var service = this.OnStationVehicleCanceled(guid);
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

    private AssistanceServiceInvoker OnStationVehicleCanceled(Guid guid)
    {
        if (!this._activeServices.TryGetValue(guid, out var activeService))
        {
            return null;
        }

        if (!this._activeServices.TryRemove(guid, out _) || activeService?.StationTypeConfig is null)
        {
            return activeService;
        }

        var assistanceServiceVehicleConfig = this._assistanceServiceStationTypeConfigRepository.FindById(activeService.StationTypeConfig.Id);
        if (assistanceServiceVehicleConfig is null)
        {
            return activeService;
        }

        assistanceServiceVehicleConfig.ActiveCapacity--;

        if (assistanceServiceVehicleConfig.ActiveCapacity < 0)
        {
            assistanceServiceVehicleConfig.ActiveCapacity = 0;
        }

        this._assistanceServiceStationTypeConfigRepository.Update(
            assistanceServiceVehicleConfig,
            x => { x.ActiveCapacity = assistanceServiceVehicleConfig.ActiveCapacity; });

        return activeService;
    }

    private sealed record AssistanceServiceInvoker
    {
        private readonly ILogger _logger;
        private readonly Action _onCanceled;
        private readonly AssistanceService _service;

        private readonly List<Func<Task>> _methodQueue = new();

        public AssistanceServiceInvoker(
            AssistanceService service,
            Action onCanceled,
            CancellationTokenSource cancellationTokenSource,
            ILogger logger,
            AssistanceServiceStationTypeConfig stationTypeConfig = null)
        {
            this._service = service;
            this.CancellationTokenSource = cancellationTokenSource;
            this._logger = logger;
            this._onCanceled = onCanceled;
            this.StationTypeConfig = stationTypeConfig;

            this._methodQueue.Add(() => service.OnCalledAsync(cancellationTokenSource));
            this._methodQueue.Add(service.OnCompletedAsync);
        }

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public AssistanceServiceStationTypeConfig StationTypeConfig { get; private set; }

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

                await this._service.SetupAsync(this.CancellationTokenSource);

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