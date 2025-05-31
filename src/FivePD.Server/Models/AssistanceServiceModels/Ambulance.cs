using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Utilities;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Models.AssistanceServiceModels;

#pragma warning disable

public class Ambulance : AssistanceService
{
    private readonly IEntityService _entityService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly ICfxService _cfxService;
    private readonly IVehicleConfigService _vehicleConfigService;
    private readonly IGeneralAssistanceServiceSyncService _syncService;

    private FPed _driver;
    private FVehicle _ambulance;
    private const float SlowingDistance = 125f;
    private const float MaximumSpeed = 30f;
    private const float MinimumSpeed = 5f;

    private Vector3 _target;
    private Entity _currentEntity;

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

    public override async Task SetupAsync(CancellationTokenSource cancellationTokenSource)
    {
        this._target = this.RequestedBy.GetUnderlyingPlayer().Character.Position;
    }

    public override Task<bool> CanRunAsync()
    {
        return Task.FromResult(true);
    }

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

        this._driver.Task.DriveTo(this._ambulance.GetUnderlyingVehicle, this._target, 0f, MaximumSpeed, (int)FDrivingStyle.Emergency);

        while (this._ambulance.GetUnderlyingVehicle.Position.DistanceToSquared(this._target) >= SlowingDistance)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            var distance = Vector3.Distance(this._ambulance.GetUnderlyingVehicle.Position, this._target);
            var distanceRatio = MathHelper.Clamp((distance - SlowingDistance) / (MaximumSpeed - SlowingDistance), 0f, 1f);
            var currentSpeed = MaximumSpeed - ((MaximumSpeed - MinimumSpeed) * distanceRatio);

            this._driver.Properties.SetDrivingSpeed(currentSpeed);

            await Task.Delay(500);
        }

        this._driver.Task.LeaveVehicle(FLeaveVehicleFlags.LeaveDoorOpen);

        await Task.Delay(2000);

        var nearbyInjuredPeds = this._entityService.GetPedsInRangeOfEntity(this._ambulance.GetUnderlyingVehicle, 200f)
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
                        Title = "FivePD",
                        Message = "No available dead peds in the area to assist.",
                    });
                break;
            }

            if (this._syncService.StartWorkOnEntity(this._currentEntity))
            {
                await this.DoWorkAsync(this._currentEntity, cancellationTokenSource);
                this._syncService.RemoveEntity(this._currentEntity);
            }
        }
    }

    private async Task DoWorkAsync(Entity ped, CancellationTokenSource cancellationToken)
    {
        this._driver.Task.RunTo(ped.Position);

        await Task.Delay(5000); // TODO: wait until driver is close to ped

        var couldRevive = this._randomService.GetRandomChance <= this._configService.GeneralConfig.AssistanceServices.Ambulance.ChanceToRevive;
        if (couldRevive)
        {
            this._cfxService.ResurrectPed(ped.NetworkId);
            this._gameNotificationService.Notify(
                this.RequestedBy,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "Patient revived!",
                });
        }
        else
        {
            this._gameNotificationService.Notify(
                this.RequestedBy,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "Patient could not be revived.",
                });
        }

        await Task.Delay(1000);
    }

    public override async Task OnCancellationRequestedAsync()
    {
        if (this._currentEntity is not null)
        {
            this._syncService.RemoveEntity(this._currentEntity);
        }

        this.ClearEntities(new Dictionary<FEntity, FEntity>()
        {
            { this._driver, this._ambulance },
            { this._ambulance, null }
        });
        await Task.CompletedTask;
    }

    public override async Task OnCompletedAsync()
    {
        if (this._currentEntity is not null)
        {
            this._syncService.RemoveEntity(this._currentEntity);
        }

        this.ClearEntities(new Dictionary<FEntity, FEntity>()
        {
            { this._driver, this._ambulance },
            { this._ambulance, null }
        });

        await Task.CompletedTask;
    }
}
