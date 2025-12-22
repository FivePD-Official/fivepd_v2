using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Utilities;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Models.AssistanceServiceModels;

/// <summary>
/// Represents an ambulance assistance service.
/// </summary>
public class Ambulance : AssistanceService
{
    private const float MaximumSpeed = 30f;
    private const float MinimumSpeed = 5f;

    private readonly IEntityService _entityService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly ICfxService _cfxService;
    private readonly IVehicleConfigService _vehicleConfigService;
    private readonly IGeneralAssistanceServiceSyncService _syncService;

    private FPed _driver;
    private FVehicle _ambulance;

    private Vector3 _target;
    private Entity _currentEntity;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ambulance"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="configService">The configuration service.</param>
    /// <param name="randomService">The random service.</param>
    /// <param name="gameNotificationService">The game notification service.</param>
    /// <param name="cfxService">The CFX service.</param>
    /// <param name="vehicleConfigService">The vehicle configuration service.</param>
    /// <param name="syncService">The general assistance service synchronization service.</param>
    public Ambulance(
        IEntityService entityService,
        IConfigService configService,
        IRandomService randomService,
        IGameNotificationService gameNotificationService,
        ICfxService cfxService,
        IVehicleConfigService vehicleConfigService,
        IGeneralAssistanceServiceSyncService syncService)
    {
        this._entityService = entityService;
        this._configService = configService;
        this._randomService = randomService;
        this._gameNotificationService = gameNotificationService;
        this._cfxService = cfxService;
        this._vehicleConfigService = vehicleConfigService;
        this._syncService = syncService;
    }

    /// <inheritdoc />
    public override async Task SetupAsync(CancellationTokenSource cancellationTokenSource)
    {
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
        var ambulances = this._vehicleConfigService.GetAll(VehicleConfigType.Ambulance);
        if (ambulances.Count == 0)
        {
            ambulances.Add(DefaultVehicleConfigs.Ambulance);
        }

        var serviceVehicle = ambulances[this._randomService.Random.Next(0, ambulances.Count)];

        this._driver = await this._entityService.SpawnPedAsync((uint)FPedHash.Paramedic01SMM, this.SpawnLocation, 0f, true);
        this._ambulance = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(serviceVehicle.Model), this.SpawnLocation, this.SpawnHeading);

        this._ambulance.Properties.CreateBlip(FBlipSprite.Health, FBlipColor.Red, "Ambulance");
        this._ambulance.Properties.IsSirenActive(true);

        this._driver.Properties.SetAlwaysKeepTask(true);
        this._driver.Properties.SetIntoVehicle(this._ambulance.GetUnderlyingVehicle, FVehicleSeat.Driver);
        this._driver.Properties.SetBlockPermanentEvents(true);

        await this.DriveToTargetWithSlowingAsync(this._driver, this._ambulance, this._target, MinimumSpeed, MaximumSpeed, cancellationTokenSource);

        this._driver.Task.LeaveVehicle(FLeaveVehicleFlags.LeaveDoorOpen);

        await Task.Delay(2000);

        var nearbyInjuredPeds = this._entityService.GetPedsInRangeOfEntity(this._ambulance.GetUnderlyingVehicle, SquaredDistances._200)
            .Where(x => Cfx.API.GetEntityHealth(x.Handle) <= 0)
            .ToList();

        this._syncService.AddEntities(typeof(Ambulance), nearbyInjuredPeds, this._target);

        while (true)
        {
            this._currentEntity = this._syncService.GetFirstEntityOnWhichWorkHasNotStartedYet(
                typeof(Ambulance),
                this._target);

            if (this._currentEntity is null)
            {
                this._gameNotificationService.Notify(
                    this.RequestedBy,
                    new GameNotificationOptions
                    {
                        Title = "LABEL.ASSISTANCE_SERVICE.AMBULANCE.TITLE",
                        Message = "LABEL.ASSISTANCE_SERVICE.AMBULANCE.NO_DEAD_PEDS",
                    });
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
    /// Performs the work of reviving a ped.
    /// </summary>
    /// <param name="ped">The ped to revive.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task DoWorkAsync(Entity ped, CancellationTokenSource cancellationToken)
    {
        this._driver.Task.RunTo(ped.Position);

        await Task.Delay(5000); // TODO: wait until driver is close to ped

#pragma warning disable S125

        // ?? var couldRevive = this._randomService.GetRandomChance <= this._configService.GeneralConfig.AssistanceServices.Ambulance.ChanceToRevive;
#pragma warning restore S125

        var couldRevive = this._randomService.GetRandomChance <= 50; // TODO: use VehicleConfig.Options to store ChanceToRevive
        if (couldRevive)
        {
            this._cfxService.ResurrectPed(ped.NetworkId);
            this._gameNotificationService.Notify(
                this.RequestedBy,
                new GameNotificationOptions
                {
                    Title = "LABEL.ASSISTANCE_SERVICE.AMBULANCE.TITLE",
                    Message = "LABEL.ASSISTANCE_SERVICE.AMBULANCE.PATIENT_REVIVED",
                });
        }
        else
        {
            this._gameNotificationService.Notify(
                this.RequestedBy,
                new GameNotificationOptions
                {
                    Title = "LABEL.ASSISTANCE_SERVICE.AMBULANCE.TITLE",
                    Message = "LABEL.ASSISTANCE_SERVICE.AMBULANCE.PATIENT_NOT_REVIVED",
                });
        }

        await Task.Delay(1000);
    }

    private Task OnCompletedOrCanceledAsync()
    {
        if (this._currentEntity is not null)
        {
            this._syncService.RemoveEntity(this._currentEntity.NetworkId);
        }

        this.ClearEntities(new Dictionary<FEntity, FEntity>()
        {
            { this._driver, this._ambulance },
            { this._ambulance, null },
        });

        return Task.CompletedTask;
    }
}