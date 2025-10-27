using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Utilities;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Models.AssistanceServiceModels;

#pragma warning disable

/// <summary>
/// Represents a coroner assistance service.
/// </summary>
public class Coroner : AssistanceService
{
    private const float SlowingDistance = 125f;
    private const float MaximumSpeed = 30f;
    private const float MinimumSpeed = 5f;

    private readonly IEntityService _entityService;
    private readonly IRandomService _randomService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IVehicleConfigService _vehicleConfigService;
    private readonly IGeneralAssistanceServiceSyncService _syncService;

    private FPed _coroner1;
    private FVehicle _van;

    private Vector3 _target;
    private Entity _currentEntity;

    /// <summary>
    /// Initializes a new instance of the <see cref="Coroner"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="randomService">The random service.</param>
    /// <param name="gameNotificationService">The game notification service.</param>
    /// <param name="vehicleConfigService">The vehicle configuration service.</param>
    /// <param name="syncService">The general assistance service synchronization service.</param>
    public Coroner(
        IEntityService entityService,
        IRandomService randomService,
        IGameNotificationService gameNotificationService,
        IVehicleConfigService vehicleConfigService,
        IGeneralAssistanceServiceSyncService syncService)
    {
        this._entityService = entityService;
        this._randomService = randomService;
        this._gameNotificationService = gameNotificationService;
        this._vehicleConfigService = vehicleConfigService;
        this._syncService = syncService;
    }

    /// <inheritdoc />
    public override async Task SetupAsync(CancellationTokenSource cancellationTokenSource)
    {
        var a = this._entityService.GetClosestFPedToEntity(this.RequestedBy.GetUnderlyingPlayer().Character);
        Debug.WriteLine($"closest: {a?.GetUnderlyingPed.Handle} - {Cfx.API.GetEntityHealth(a.GetUnderlyingPed.Handle)}");
        this._target = this.RequestedBy.GetUnderlyingPlayer().Character.Position;
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task<bool> CanRunAsync()
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public override async Task OnCalledAsync(CancellationTokenSource cancellationTokenSource)
    {
        var coroners = this._vehicleConfigService.GetAll(VehicleConfigType.Coroner);
        if (coroners.Count == 0)
        {
            coroners.Add(DefaultVehicleConfigs.Coroner);
        }

        var serviceVehicle = coroners[this._randomService.Random.Next(0, coroners.Count)];

        this._coroner1 = await this._entityService.SpawnPedAsync((uint)FPedHash.Autopsy01SMY, this.SpawnLocation, 0f, true);
        this._van = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(serviceVehicle.Model), this.SpawnLocation, this.SpawnHeading);

        this._van.Properties.CreateBlip(FBlipSprite.Deathmatch, FBlipColor.White, "Coroner");
        this._van.Properties.IsSirenActive(true);

        this._coroner1.Properties.SetAlwaysKeepTask(true);
        this._coroner1.Properties.SetIntoVehicle(this._van.GetUnderlyingVehicle, FVehicleSeat.Driver);
        this._coroner1.Properties.SetBlockPermanentEvents(true);

        this._coroner1.Task.DriveTo(this._van.GetUnderlyingVehicle, this._target, 0f, MaximumSpeed, (int)FDrivingStyle.Emergency);

        while (this._van.GetUnderlyingVehicle.Position.DistanceToSquared(this._target) >= SlowingDistance)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            var distance = Vector3.Distance(this._van.GetUnderlyingVehicle.Position, this._target);
            var distanceRatio = MathHelper.Clamp((distance - SlowingDistance) / (MaximumSpeed - SlowingDistance), 0f, 1f);
            var currentSpeed = MaximumSpeed - ((MaximumSpeed - MinimumSpeed) * distanceRatio);

            this._coroner1.Properties.SetDrivingSpeed(currentSpeed);

            await Task.Delay(500);
        }

        this._coroner1.Task.LeaveVehicle(FLeaveVehicleFlags.LeaveDoorOpen);

        await Task.Delay(2000);

        var nearbyDeadPeds = this._entityService.GetPedsInRangeOfEntity(this._van.GetUnderlyingVehicle, 100f)
            .Where(x => Cfx.API.GetEntityHealth(x.Handle) <= 0)
            .ToList();
        this._syncService.AddEntities(typeof(Coroner), nearbyDeadPeds, this._target);
        
        while (true)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                break;
            }

            this._currentEntity = this._syncService.GetFirstEntityOnWhichWorkHasNotStartedYet(
                typeof(Coroner),
                this._target);
            
            if (this._currentEntity is null)
            {
                if (nearbyDeadPeds.Count == 0)
                {
                    this._gameNotificationService.Notify(
                        this.RequestedBy,
                        new GameNotificationOptions
                        {
                            Title = "LABEL.ASSISTANCE_SERVICE.OFFICIAL.CORONER.TITLE",
                            Message = "LABEL.ASSISTANCE_SERVICE.OFFICIAL.CORONER.NO_DECEASED_FOUND",
                        });
                }
                else
                {
                    this._gameNotificationService.Notify(
                        this.RequestedBy,
                        new GameNotificationOptions
                        {
                            Title = "LABEL.ASSISTANCE_SERVICE.OFFICIAL.CORONER.TITLE",
                            Message = "LABEL.ASSISTANCE_SERVICE.OFFICIAL.CORONER.ALL_TRANSPORTED",
                        });
                }

                break;
            }

            if (this._syncService.StartWorkOnEntity(this._currentEntity))
            {
                var entityNetworkId = this._currentEntity.NetworkId;
                await this.DoWorkAsync(this._currentEntity, cancellationTokenSource);
                this._syncService.RemoveEntity(entityNetworkId);
            }
        }
    }

    /// <inheritdoc />
    public override Task OnCancellationRequestedAsync()
    {
        return this.OnCompletedOrCanceledAsync();
    }

    /// <inheritdoc />
    public override Task OnCompletedAsync()
    {
        return this.OnCompletedOrCanceledAsync();
    }

    /// <summary>
    /// Performs the work of collecting a dead ped.
    /// </summary>
    /// <param name="ped">The ped to collect.</param>
    /// <param name="cancellationTokenSource">The cancellation token.</param>
    private async Task DoWorkAsync(Entity ped, CancellationTokenSource cancellationTokenSource)
    {
        this._coroner1.Task.RunTo(ped.Position);

        while (this._coroner1.GetUnderlyingPed.Position.DistanceToSquared(ped.Position) > 4.0f)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            await Task.Delay(100);
        }

        await Task.Delay(2000);

        Cfx.API.DeleteEntity(ped.Handle);

        await Task.Delay(500);
    }

    private Task OnCompletedOrCanceledAsync()
    {
        if (this._currentEntity is not null)
        {
            this._syncService.RemoveEntity(this._currentEntity.NetworkId);
        }
        
        this.ClearEntities(new Dictionary<FEntity, FEntity>
        {
            { this._coroner1, this._van },
            { this._van, null },
        });

        return Task.CompletedTask;
    }
}