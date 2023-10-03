using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Client.Models;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable // TODO remove

/// <inheritdoc cref="FivePD.Client.Interfaces.IPedService" />
internal class PedService : IPedService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IQueueService _queueService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PedService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="queueService">The <see cref="IQueueService"/> to use.</param>
    public PedService(EventHandlerDictionary eventHandlerDictionary, IQueueService queueService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._queueService = queueService;
    }

    /// <inheritdoc />
    public void OnInit()
    {
        // TODO: move to PedHistory service
        this._eventHandlerDictionary.AddUIToServerEvent(Events.PedHistory.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.PedHistory.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.PedHistory.GetAllSimple);

        this._eventHandlerDictionary[Events.Ped.GetProperty] += new Action<string, int, string>(this.GetPedPropertyAsync);
        this._eventHandlerDictionary[Events.Ped.SetProperty] += new Action<int, string, string>(this.SetPedProperty);
        this._eventHandlerDictionary[Events.Ped.SetTask] += new Action<string>(this.SetTask);
        this._eventHandlerDictionary[Events.Ped.InvokeMethod] += new Action<string>(this.InvokeMethod);
        this._eventHandlerDictionary[Events.Ped.WeaponCollection] += new Action<string>(this.WeaponCollection);
        this._eventHandlerDictionary[Events.Ped.WeaponCollectionWithReturn] += new Action<string, string>(this.WeaponCollectionWithReturn);
    }

    /// <inheritdoc />
    public void PedLeaveVehicleWithHandsUp(Ped ped)
    {
        ped.BlockPermanentEvents = true;

        var sequence = new TaskSequence();
        sequence.AddTask.LeaveVehicle(ped.CurrentVehicle, false);
        sequence.AddTask.HandsUp(-1);
        sequence.Close();
        ped.Task.PerformSequence(sequence);

        ped.PlayAmbientSpeech("GENERIC_SHOCKED_MED");
    }

    /// <inheritdoc />
    public FPed GeneratePed(Ped ped)
    {
        if (ped is null)
        {
            return null;
        }

        var fPed = new FPed(ped);
        if (!fPed.HasGeneratedData.Value)
        {
            var fPedBuilder = new CommonFPedBuilder(ped.NetworkId)
                .WithGender((FGender)ped.Gender);
            BaseScript.TriggerServerEvent(Events.Ped.Generate, JsonConvert.SerializeObject(fPedBuilder));
        }

        return fPed;
    }

    private async void GetPedPropertyAsync(string eventCallbackId, int networkId, string property)
    {
        var ped = await this.FindPed(networkId);
        if (ped is null)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, string.Empty);
            return;
        }

        object value = property switch
        {
            FPedPropertiesDTO.Events.Accuracy => ped.Accuracy,
            FPedPropertiesDTO.Events.Armor => ped.Armor,
            FPedPropertiesDTO.Events.ArmorFloat => ped.ArmorFloat,
            FPedPropertiesDTO.Events.CanFlyThroughWindscreen => ped.CanFlyThroughWindscreen,
            FPedPropertiesDTO.Events.CanRagdoll => ped.CanRagdoll,
            FPedPropertiesDTO.Events.CanSufferCriticalHits => ped.CanSufferCriticalHits,
            FPedPropertiesDTO.Events.CanWrithe => ped.CanWrithe,
            FPedPropertiesDTO.Events.CurrentVehicle => ped.CurrentVehicle.NetworkId,
            FPedPropertiesDTO.Events.DropsWeaponsOnDeath => ped.DropsWeaponsOnDeath,
            FPedPropertiesDTO.Events.FatalInjuryHealthThreshold => ped.FatalInjuryHealthThreshold,
            FPedPropertiesDTO.Events.Gender => (int)ped.Gender,
            FPedPropertiesDTO.Events.InjuryHealthThreshold => ped.InjuryHealthThreshold,
            FPedPropertiesDTO.Events.IsAiming => ped.IsAiming,
            FPedPropertiesDTO.Events.IsAimingFromCover => ped.IsAimingFromCover,
            FPedPropertiesDTO.Events.IsAmbientSpeechEnabled => ped.IsAmbientSpeechEnabled,
            FPedPropertiesDTO.Events.IsAmbientSpeechplaying => ped.IsAmbientSpeechplaying,
            FPedPropertiesDTO.Events.IsBeingJacked => ped.IsBeingJacked,
            FPedPropertiesDTO.Events.IsBeingStealthKilled => ped.IsBeingStealthKilled,
            FPedPropertiesDTO.Events.IsBeingStunned => ped.IsBeingStunned,
            FPedPropertiesDTO.Events.IsClimbing => ped.IsClimbing,
            FPedPropertiesDTO.Events.IsCuffed => ped.IsCuffed,
            FPedPropertiesDTO.Events.IsDiving => ped.IsDiving,
            FPedPropertiesDTO.Events.IsDoingDriveBy => ped.IsDoingDriveBy,
            FPedPropertiesDTO.Events.IsDucking => ped.IsDucking,
            FPedPropertiesDTO.Events.IsFalling => ped.IsFalling,
            FPedPropertiesDTO.Events.IsFleeing => ped.IsFleeing,
            FPedPropertiesDTO.Events.IsGettingIntoAVehicle => ped.IsGettingIntoAVehicle,
            FPedPropertiesDTO.Events.IsGettingUp => ped.IsGettingUp,
            FPedPropertiesDTO.Events.IsGoingIntoCover => ped.IsGoingIntoCover,
            FPedPropertiesDTO.Events.IsHuman => ped.IsHuman,
            FPedPropertiesDTO.Events.IsIdle => ped.IsIdle,
            FPedPropertiesDTO.Events.IsInBoat => ped.IsInBoat,
            FPedPropertiesDTO.Events.IsInCombat => ped.IsInCombat,
            FPedPropertiesDTO.Events.IsInCoverFacingLeft => ped.IsInCoverFacingLeft,
            FPedPropertiesDTO.Events.IsInFlyingVehicle => ped.IsInFlyingVehicle,
            FPedPropertiesDTO.Events.IsInGroup => ped.IsInGroup,
            FPedPropertiesDTO.Events.IsInHeli => ped.IsInHeli,
            FPedPropertiesDTO.Events.IsInjured => ped.IsInjured,
            FPedPropertiesDTO.Events.IsInMeleeCombat => ped.IsInMeleeCombat,
            FPedPropertiesDTO.Events.IsInParachuteFreeFall => ped.IsInParachuteFreeFall,
            FPedPropertiesDTO.Events.IsInPlane => ped.IsInPlane,
            FPedPropertiesDTO.Events.IsInPoliceVehicle => ped.IsInPoliceVehicle,
            FPedPropertiesDTO.Events.IsInStealthMode => ped.IsInStealthMode,
            FPedPropertiesDTO.Events.IsInSub => ped.IsInSub,
            FPedPropertiesDTO.Events.IsInTaxi => ped.IsInTaxi,
            FPedPropertiesDTO.Events.IsInTrain => ped.IsInTrain,
            FPedPropertiesDTO.Events.IsJacking => ped.IsJacking,
            FPedPropertiesDTO.Events.IsJumping => ped.IsJumping,
            FPedPropertiesDTO.Events.IsJumpingOutOfVehicle => ped.IsJumpingOutOfVehicle,
            FPedPropertiesDTO.Events.IsOnBike => ped.IsOnBike,
            FPedPropertiesDTO.Events.IsOnFoot => ped.IsOnFoot,
            FPedPropertiesDTO.Events.IsPerformingStealthKill => ped.IsPerformingStealthKill,
            FPedPropertiesDTO.Events.IsPlantingBomb => ped.IsPlantingBomb,
            FPedPropertiesDTO.Events.IsPlayer => ped.IsPlayer,
            FPedPropertiesDTO.Events.IsProne => ped.IsProne,
            FPedPropertiesDTO.Events.IsReloading => ped.IsReloading,
            FPedPropertiesDTO.Events.IsRunning => ped.IsRunning,
            FPedPropertiesDTO.Events.IsScriptedSpeechplaying => ped.IsScriptedSpeechplaying,
            FPedPropertiesDTO.Events.IsShooting => ped.IsShooting,
            FPedPropertiesDTO.Events.IsSprinting => ped.IsSprinting,
            FPedPropertiesDTO.Events.IsStopped => ped.IsStopped,
            FPedPropertiesDTO.Events.IsSwimming => ped.IsSwimming,
            FPedPropertiesDTO.Events.IsSwimmingUnderWater => ped.IsSwimmingUnderWater,
            FPedPropertiesDTO.Events.IsTryingToEnterALockedVehicle => ped.IsTryingToEnterALockedVehicle,
            FPedPropertiesDTO.Events.IsVaulting => ped.IsVaulting,
            FPedPropertiesDTO.Events.IsWalking => ped.IsWalking,
            FPedPropertiesDTO.Events.IsWearingHelmet => ped.IsWearingHelmet,
            FPedPropertiesDTO.Events.LastVehicle => ped.LastVehicle.NetworkId,
            FPedPropertiesDTO.Events.Money => ped.Money,
            FPedPropertiesDTO.Events.SeatIndex => (int)ped.SeatIndex,
            FPedPropertiesDTO.Events.Sweat => ped.Sweat,
            FPedPropertiesDTO.Events.VehicleTryingToEnter => ped.VehicleTryingToEnter.NetworkId,
            FPedPropertiesDTO.Events.VehicleWeapon => (int)ped.VehicleWeapon,
            FPedPropertiesDTO.Events.WasKilledByStealth => ped.WasKilledByStealth,
            FPedPropertiesDTO.Events.WasKilledByTakedown => ped.WasKilledByTakedown,
            FPedPropertiesDTO.Events.IsDead => ped.IsDead,
            FPedPropertiesDTO.Events.IsInVehicle => ped.IsInVehicle(),
            _ => string.Empty,
        };

        BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, value.ToString());
    }

    private async void SetPedProperty(int networkId, string property, string value)
    {
        var ped = await this.FindPed(networkId);
        if (ped is null)
        {
            return;
        }

        switch (property)
        {
            case FPedPropertiesDTO.Events.Accuracy:
            {
                this._queueService.Queue(() => ped.Accuracy = int.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.AlwaysDiesOnLowHealth:
            {
                this._queueService.Queue(() => ped.AlwaysDiesOnLowHealth = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.AlwaysKeepTask:
            {
                this._queueService.Queue(() => ped.AlwaysKeepTask = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.Armor:
            {
                this._queueService.Queue(() => ped.Armor = int.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.ArmorFloat:
            {
                this._queueService.Queue(() => ped.ArmorFloat = float.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.BlockPermanentEvents:
            {
                this._queueService.Queue(() => ped.BlockPermanentEvents = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanBeDraggedOutOfVehicle:
            {
                this._queueService.Queue(() => ped.CanBeDraggedOutOfVehicle = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanBeKnockedOffBike:
            {
                this._queueService.Queue(() => ped.CanBeKnockedOffBike = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanBeShotInVehicle:
            {
                this._queueService.Queue(() => ped.CanBeShotInVehicle = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanBeTargetted:
            {
                this._queueService.Queue(() => ped.CanBeTargetted = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanFlyThroughWindscreen:
            {
                this._queueService.Queue(() => ped.CanFlyThroughWindscreen = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanPlayGestures:
            {
                this._queueService.Queue(() => ped.CanPlayGestures = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanRagdoll:
            {
                this._queueService.Queue(() => ped.CanRagdoll = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanSufferCriticalHits:
            {
                this._queueService.Queue(() => ped.CanSufferCriticalHits = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanSwitchWeapons:
            {
                this._queueService.Queue(() => ped.CanSwitchWeapons = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanWearHelmet:
            {
                this._queueService.Queue(() => ped.CanWearHelmet = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CanWrithe:
            {
                this._queueService.Queue(() => ped.CanWrithe = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.DiesInstantlyInWater:
            {
                this._queueService.Queue(() => ped.DiesInstantlyInWater = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.DrivingSpeed:
            {
                this._queueService.Queue(() => ped.DrivingSpeed = float.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.DrivingStyle:
            {
                this._queueService.Queue(() => ped.DrivingStyle = (DrivingStyle)int.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.DropsWeaponsOnDeath:
            {
                this._queueService.Queue(() => ped.DropsWeaponsOnDeath = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.DrownsInSinkingVehicle:
            {
                this._queueService.Queue(() => ped.DrownsInSinkingVehicle = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.DrownsInWater:
            {
                this._queueService.Queue(() => ped.DrownsInWater = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.FatalInjuryHealthThreshold:
            {
                this._queueService.Queue(() => ped.FatalInjuryHealthThreshold = float.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.InjuryHealthThreshold:
            {
                this._queueService.Queue(() => ped.InjuryHealthThreshold = float.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.IsDucking:
            {
                this._queueService.Queue(() => ped.IsDucking = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.IsEnemy:
            {
                this._queueService.Queue(() => ped.IsEnemy = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.IsPainAudioEnabled:
            {
                this._queueService.Queue(() => ped.IsPainAudioEnabled = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.IsPriorityTargetForEnemies:
            {
                this._queueService.Queue(() => ped.IsPriorityTargetForEnemies = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.MaxDrivingSpeed:
            {
                this._queueService.Queue(() => ped.MaxDrivingSpeed = float.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.Money:
            {
                this._queueService.Queue(() => ped.Money = int.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.MovementAnimationSet:
            {
                this._queueService.Queue(() => ped.MovementAnimationSet = value);
                break;
            }

            case FPedPropertiesDTO.Events.NeverLeavesGroup:
            {
                this._queueService.Queue(() => ped.NeverLeavesGroup = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.ShootRate:
            {
                this._queueService.Queue(() => ped.ShootRate = int.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.StaysInVehicleWhenJacked:
            {
                this._queueService.Queue(() => ped.StaysInVehicleWhenJacked = bool.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.Sweat:
            {
                this._queueService.Queue(() => ped.Sweat = float.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.VehicleDrivingFlags:
            {
                this._queueService.Queue(() => ped.VehicleDrivingFlags = (VehicleDrivingFlags)int.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.Voice:
            {
                this._queueService.Queue(() => ped.Voice = value);
                break;
            }

            case FPedPropertiesDTO.Events.WetnessHeight:
            {
                this._queueService.Queue(() => ped.WetnessHeight = float.Parse(value));
                break;
            }

            case FPedPropertiesDTO.Events.CreateBlip:
            {
                var request = JsonConvert.DeserializeObject<FPedPropertiesDTO.CreateBlipRequest>(value);
                this._queueService.Queue(() => ped.CreateBlip(request.Sprite, request.Color, request.Text));
                break;
            }

            case FPedPropertiesDTO.Events.RemoveBlips:
            {
                this._queueService.Queue(() => ped.RemoveBlips());
                break;
            }
        }
    }

    private async void InvokeMethod(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<FPedPropertiesDTO.InvokeMethodRequest>(serializedRequest);
        var ped = await this.FindPed(request.NetworkId);
        if (ped is null)
        {
            return;
        }

        switch (request.Method)
        {
            case FPedPropertiesDTO.Events.SetIntoVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedPropertiesDTO.SetIntoVehicle>(request.Args);
                ped.SetIntoVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, (VehicleSeat)args.VehicleSeat);
                break;
            }

            case FPedPropertiesDTO.Events.MarkAsNoLongerNeeded:
            {
                ped.MarkAsNoLongerNeeded();
                break;
            }
        }
    }

    private async void SetTask(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<FPedTasksDTO.SetPedTaskRequest>(serializedRequest);
        var ped = await this.FindPed(request.NetworkId);
        if (ped is null)
        {
            return;
        }

        switch (request.Task)
        {
            case FPedTasksDTO.Events.AchieveHeading:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.AchieveHeading>(request.Args);
                this._queueService.Queue(() => ped.Task.AchieveHeading(args.Heading, args.Timeout));
                break;
            }

            case FPedTasksDTO.Events.AimAt:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.AimAt>(request.Args);
                this._queueService.Queue(() => ped.Task.AimAt(args.Target, args.Duration));
                break;
            }

            case FPedTasksDTO.Events.Arrest:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.Arrest>(request.Args);
                this._queueService.Queue(() => ped.Task.Arrest(Entity.FromNetworkId(args.NetworkId) as Ped));
                break;
            }

            case FPedTasksDTO.Events.ChaseWithGroundVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ChaseWithGroundVehicle>(request.Args);
                this._queueService.Queue(() => ped.Task.ChaseWithGroundVehicle(Entity.FromNetworkId(args.TargetNetworkId)));
                break;
            }

            case FPedTasksDTO.Events.ChaseWithHelicopter:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ChaseWithHelicopter>(request.Args);
                this._queueService.Queue(() => ped.Task.ChaseWithHelicopter(Entity.FromNetworkId(args.TargetNetworkId), args.Offset));
                break;
            }

            case FPedTasksDTO.Events.ChaseWithPlane:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ChaseWithPlane>(request.Args);
                this._queueService.Queue(() => ped.Task.ChaseWithPlane(Entity.FromNetworkId(args.TargetNetworkId), args.Offset));
                break;
            }

            case FPedTasksDTO.Events.ChatTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ChatTo>(request.Args);
                this._queueService.Queue(() => ped.Task.ChatTo(Entity.FromNetworkId(args.NetworkId) as Ped));
                break;
            }

            case FPedTasksDTO.Events.ClearAll:
            {
                this._queueService.Queue(() => ped.Task.ClearAll());
                break;
            }

            case FPedTasksDTO.Events.ClearAllImmediately:
            {
                this._queueService.Queue(() => ped.Task.ClearAllImmediately());
                break;
            }

            case FPedTasksDTO.Events.ClearAnimation:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ClearAnimation>(request.Args);
                this._queueService.Queue(() => ped.Task.ClearAnimation(args.AnimSet, args.AnimName));
                break;
            }

            case FPedTasksDTO.Events.ClearLookAt:
            {
                this._queueService.Queue(() => ped.Task.ClearLookAt());
                break;
            }

            case FPedTasksDTO.Events.ClearSecondary:
            {
                this._queueService.Queue(() => ped.Task.ClearSecondary());
                break;
            }

            case FPedTasksDTO.Events.Climb:
            {
                this._queueService.Queue(() => ped.Task.Climb());
                break;
            }

            case FPedTasksDTO.Events.ClimbLadder:
            {
                this._queueService.Queue(() => ped.Task.ClimbLadder());
                break;
            }

            case FPedTasksDTO.Events.Cower:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.Cower>(request.Args);
                this._queueService.Queue(() => ped.Task.Cower(args.Duration));
                break;
            }

            case FPedTasksDTO.Events.CruiseWithVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.CruiseWithVehicle>(request.Args);
                this._queueService.Queue(
                    () => ped.Task.CruiseWithVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, args.Speed, args.DrivingStyle));
                break;
            }

            case FPedTasksDTO.Events.DriveTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.DriveTo>(request.Args);
                this._queueService.Queue(() =>
                    ped.Task.DriveTo(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, args.Target, args.Radius, args.Speed, args.DrivingStyle));
                break;
            }

            case FPedTasksDTO.Events.EnterAnyVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.EnterAnyVehicle>(request.Args);
                this._queueService.Queue(() => ped.Task.EnterAnyVehicle((VehicleSeat)args.Seat, args.Timeout, args.Speed, args.Flag));
                break;
            }

            case FPedTasksDTO.Events.EnterVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.EnterVehicle>(request.Args);
                this._queueService.Queue(() =>
                    ped.Task.EnterVehicle(Entity.FromHandle(args.VehicleNetworkId) as Vehicle, (VehicleSeat)args.Seat, args.Timeout, args.Speed, args.Flag));
                break;
            }

            case FPedTasksDTO.Events.FightAgainst:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FightAgainst>(request.Args);
                this._queueService.Queue(() => ped.Task.FightAgainst(Entity.FromHandle(args.PedNetworkId) as Ped, args.Duration));
                break;
            }

            case FPedTasksDTO.Events.FightAgainstHatedTargets:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FightAgainstHatedTargets>(request.Args);
                this._queueService.Queue(() => ped.Task.FightAgainstHatedTargets(args.Radius, args.Duration));
                break;
            }

            case FPedTasksDTO.Events.FleeFrom:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FleeFrom>(request.Args);
                this._queueService.Queue(() => ped.Task.FleeFrom(args.Position, args.Duration));
                break;
            }

            case FPedTasksDTO.Events.FollowPointRoute:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FollowPointRoute>(request.Args);
                this._queueService.Queue(() => ped.Task.FollowPointRoute(args.Points.ToArray()));
                break;
            }

            case FPedTasksDTO.Events.FollowToOffsetFromEntity:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FollowToOffsetFromEntity>(request.Args);
                this._queueService.Queue(() => ped.Task.FollowToOffsetFromEntity(Entity.FromNetworkId(args.TargetNetworkId), args.Offset, args.MovementSpeed,
                    args.Timeout, args.StoppingRange,
                    args.PersistFollowing));
                break;
            }

            case FPedTasksDTO.Events.GoToTarget:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.GoToTarget>(request.Args);
                this._queueService.Queue(() => ped.Task.GoTo(Entity.FromNetworkId(args.TargetNetworkId)));
                break;
            }

            case FPedTasksDTO.Events.GoToTargetWithOffset:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.GoToTargetWithOffset>(request.Args);
                this._queueService.Queue(() => ped.Task.GoTo(Entity.FromNetworkId(args.TargetNetworkId), args.Offset, args.Timeout));
                break;
            }

            case FPedTasksDTO.Events.GoToPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.GoToPosition>(request.Args);
                this._queueService.Queue(() => ped.Task.GoTo(args.Position, args.IgnorePaths, args.Timeout));
                break;
            }

            case FPedTasksDTO.Events.GuardCurrentPosition:
            {
                this._queueService.Queue(() => ped.Task.GuardCurrentPosition());
                break;
            }

            case FPedTasksDTO.Events.HandsUp:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.HandsUp>(request.Args);
                this._queueService.Queue(() => ped.Task.HandsUp(args.Duration));
                break;
            }

            case FPedTasksDTO.Events.Jump:
            {
                this._queueService.Queue(() => ped.Task.Jump());
                break;
            }

            case FPedTasksDTO.Events.LandPlane:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LandPlane>(request.Args);
                this._queueService.Queue(() =>
                    ped.Task.LandPlane(args.StartPosition, args.TouchdownPosition, Entity.FromNetworkId(args.PlaneNetworkId) as Vehicle));
                break;
            }

            case FPedTasksDTO.Events.LeaveVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LeaveVehicle>(request.Args);
                this._queueService.Queue(() => ped.Task.LeaveVehicle((LeaveVehicleFlags)args.Flags));
                break;
            }

            case FPedTasksDTO.Events.LeaveVehicleSpecified:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LeaveVehicleSpecified>(request.Args);
                this._queueService.Queue(() => ped.Task.LeaveVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, (LeaveVehicleFlags)args.Flags));
                break;
            }

            case FPedTasksDTO.Events.LeaveVehicleCloseDoor:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LeaveVehicleCloseDoor>(request.Args);
                this._queueService.Queue(() => ped.Task.LeaveVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, args.CloseDoor));
                break;
            }

            case FPedTasksDTO.Events.LookAtTarget:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LookAtTarget>(request.Args);
                this._queueService.Queue(() => ped.Task.LookAt(Entity.FromNetworkId(args.TargetNetworkId), args.Duration));
                break;
            }

            case FPedTasksDTO.Events.LookAtPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LookAtPosition>(request.Args);
                this._queueService.Queue(() => ped.Task.LookAt(args.Position, args.Duration));
                break;
            }

            case FPedTasksDTO.Events.ParachuteTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ParachuteTo>(request.Args);
                this._queueService.Queue(() => ped.Task.ParachuteTo(args.Position));
                break;
            }

            case FPedTasksDTO.Events.ParkVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ParkVehicle>(request.Args);
                this._queueService.Queue(() => ped.Task.ParkVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, args.Position, args.Heading,
                    args.Radius, args.KeepEngineOn));
                break;
            }

            case FPedTasksDTO.Events.PlayAnimation:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.PlayAnimation>(request.Args);

                async void RunAsync()
                {
                    Cfx.API.RequestAnimDict(args.AnimDict);
                    while (!Cfx.API.HasAnimDictLoaded(args.AnimDict))
                    {
                        await BaseScript.Delay(0);
                    }

                    ped.Task.PlayAnimation(args.AnimDict, args.AnimName);
                }

                this._queueService.Queue(RunAsync);

                break;
            }

            case FPedTasksDTO.Events.PlayAnimationWithBlendInSpeed:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.PlayAnimationWithBlendInSpeed>(request.Args);
                this._queueService.Queue(() =>
                    ped.Task.PlayAnimation(args.AnimDict, args.AnimName, args.BlendInSpeed, args.Duration, (AnimationFlags)args.Flags));
                break;
            }

            case FPedTasksDTO.Events.PlayAnimationWithPlaybackRate:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.PlayAnimationWithPlaybackRate>(request.Args);
                this._queueService.Queue(() => ped.Task.PlayAnimation(args.AnimDict, args.AnimName, args.Speed, args.Duration, args.PlaybackRate));
                break;
            }

            case FPedTasksDTO.Events.PlayAnimationWithBlendInAndOutSpeed:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.PlayAnimationWithBlendInAndOutSpeed>(request.Args);
                this._queueService.Queue(() => ped.Task.PlayAnimation(args.AnimDict, args.AnimName, args.BlendInSpeed, args.BlendOutSpeed, args.Duration,
                    (AnimationFlags)args.Flags,
                    args.PlaybackRate));
                break;
            }

            case FPedTasksDTO.Events.PutAwayMobilePhone:
            {
                this._queueService.Queue(() => ped.Task.PutAwayMobilePhone());
                break;
            }

            case FPedTasksDTO.Events.PutAwayParachute:
            {
                this._queueService.Queue(() => ped.Task.PutAwayParachute());
                break;
            }

            case FPedTasksDTO.Events.ReactAndFlee:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ReactAndFlee>(request.Args);
                this._queueService.Queue(() => ped.Task.ReactAndFlee(Entity.FromNetworkId(args.PedNetworkId) as Ped));
                break;
            }

            case FPedTasksDTO.Events.ReloadWeapon:
            {
                this._queueService.Queue(() => ped.Task.ReloadWeapon());
                break;
            }

            case FPedTasksDTO.Events.RunTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.RunTo>(request.Args);
                this._queueService.Queue(() => ped.Task.RunTo(args.Position, args.IgnorePaths, args.Timeout));
                break;
            }

            case FPedTasksDTO.Events.ShootAtTarget:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ShootAtTarget>(request.Args);
                this._queueService.Queue(() =>
                    ped.Task.ShootAt(Entity.FromNetworkId(args.TargetNetworkId) as Ped, args.Duration, (FiringPattern)args.FiringPattern));
                break;
            }

            case FPedTasksDTO.Events.ShootAtPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ShootAtPosition>(request.Args);
                this._queueService.Queue(() => ped.Task.ShootAt(args.Position, args.Duration, (FiringPattern)args.FiringPattern));
                break;
            }

            case FPedTasksDTO.Events.ShuffleToNextVehicleSeat:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ShuffleToNextVehicleSeat>(request.Args);
                this._queueService.Queue(() => ped.Task.ShuffleToNextVehicleSeat(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle));
                break;
            }

            case FPedTasksDTO.Events.Skydive:
            {
                this._queueService.Queue(() => ped.Task.Skydive());
                break;
            }

            case FPedTasksDTO.Events.SlideTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.SlideTo>(request.Args);
                this._queueService.Queue(() => ped.Task.SlideTo(args.Position, args.Heading));
                break;
            }

            case FPedTasksDTO.Events.StandStill:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.StandStill>(request.Args);
                this._queueService.Queue(() => this._queueService.Queue(() => ped.Task.StandStill(args.Duration)));
                break;
            }

            case FPedTasksDTO.Events.StartScenario:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.StartScenario>(request.Args);
                this._queueService.Queue(() => ped.Task.StartScenario(args.Name, args.Position));
                break;
            }

            case FPedTasksDTO.Events.SwapWeapon:
            {
                this._queueService.Queue(() => ped.Task.SwapWeapon());
                break;
            }

            case FPedTasksDTO.Events.TurnToEntity:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.TurnToEntity>(request.Args);
                this._queueService.Queue(() => ped.Task.TurnTo(Entity.FromNetworkId(args.EntityNetworkId), args.Duration));
                break;
            }

            case FPedTasksDTO.Events.TurnToPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.TurnToPosition>(request.Args);
                this._queueService.Queue(() => ped.Task.TurnTo(args.Position, args.Duration));
                break;
            }

            case FPedTasksDTO.Events.UseMobilePhone:
            {
                this._queueService.Queue(() => ped.Task.UseMobilePhone());
                break;
            }

            case FPedTasksDTO.Events.UseMobilePhoneWithDuration:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.UseMobilePhoneWithDuration>(request.Args);
                this._queueService.Queue(() => ped.Task.UseMobilePhone(args.Duration));
                break;
            }

            case FPedTasksDTO.Events.UseParachute:
            {
                this._queueService.Queue(() => ped.Task.UseParachute());
                break;
            }

            case FPedTasksDTO.Events.VehicleChase:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.VehicleChase>(request.Args);
                this._queueService.Queue(() => ped.Task.VehicleChase(Entity.FromNetworkId(args.TargetNetworkId) as Ped));
                break;
            }

            case FPedTasksDTO.Events.VehicleShootAtPed:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.VehicleShootAtPed>(request.Args);
                this._queueService.Queue(() => ped.Task.VehicleShootAtPed(Entity.FromNetworkId(args.TargetNetworkId) as Ped));
                break;
            }

            case FPedTasksDTO.Events.Wait:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.Wait>(request.Args);
                this._queueService.Queue(() => ped.Task.Wait(args.Duration));
                break;
            }

            case FPedTasksDTO.Events.WanderAround:
            {
                this._queueService.Queue(() => ped.Task.WanderAround());
                break;
            }

            case FPedTasksDTO.Events.WanderAroundPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.WanderAroundPosition>(request.Args);
                this._queueService.Queue(() => ped.Task.WanderAround(args.Position, args.Radius));
                break;
            }

            case FPedTasksDTO.Events.WarpIntoVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.WarpIntoVehicle>(request.Args);
                this._queueService.Queue(() => ped.Task.WarpIntoVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, (VehicleSeat)args.Seat));
                break;
            }

            case FPedTasksDTO.Events.WarpOutOfVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.WarpOutOfVehicle>(request.Args);
                this._queueService.Queue(() => ped.Task.WarpOutOfVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle));
                break;
            }
        }
    }

    private async void WeaponCollection(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<WeaponCollectionDTO.WeaponCollectionRequest>(serializedRequest);
        var ped = await this.FindPed(request.NetworkId);
        if (ped is null)
        {
            return;
        }

        switch (request.SubEvent)
        {
            case WeaponCollectionDTO.Events.Give:
            {
                var args = JsonConvert.DeserializeObject<WeaponCollectionDTO.Give>(request.Args);
                this._queueService.Queue(() => ped.Weapons.Give((WeaponHash)args.Hash, args.AmmoCount, args.EquipNow, args.IsAmmoLoaded));
                break;
            }

            case WeaponCollectionDTO.Events.Remove:
            {
                var args = JsonConvert.DeserializeObject<WeaponCollectionDTO.Remove>(request.Args);
                this._queueService.Queue(() => ped.Weapons.Remove((WeaponHash)args.Hash));
                break;
            }

            case WeaponCollectionDTO.Events.RemoveAll:
            {
                this._queueService.Queue(() => ped.Weapons.RemoveAll());
                break;
            }

            case WeaponCollectionDTO.Events.Select:
            {
                var args = JsonConvert.DeserializeObject<WeaponCollectionDTO.Select>(request.Args);
                this._queueService.Queue(() => ped.Weapons.Select((WeaponHash)args.Hash));
                break;
            }

            case WeaponCollectionDTO.Events.Drop:
            {
                this._queueService.Queue(() => ped.Weapons.Drop());
                break;
            }

            case WeaponCollectionDTO.Events.AddComponentToWeapon:
            {
                var args = JsonConvert.DeserializeObject<WeaponCollectionDTO.AddComponentToWeapon>(request.Args);
                if (Cfx.API.DoesWeaponTakeWeaponComponent(args.WeaponHash, args.ComponentHash))
                {
                    this._queueService.Queue(() => Cfx.API.GiveWeaponComponentToPed(Game.PlayerPed.Handle, args.WeaponHash, args.ComponentHash));
                }

                break;
            }
        }
    }

    private async void WeaponCollectionWithReturn(string eventCallbackId, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<WeaponCollectionDTO.WeaponCollectionRequest>(serializedRequest);
        var ped = await this.FindPed(request.NetworkId);
        if (ped is null)
        {
            return;
        }

        switch (request.SubEvent)
        {
            case WeaponCollectionDTO.Events.HasWeapon:
            {
                var args = JsonConvert.DeserializeObject<WeaponCollectionDTO.HasWeapon>(request.Args);
                var value = ped.Weapons.HasWeapon((WeaponHash)args.Hash);
                BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, value.ToString());
                break;
            }

            case WeaponCollectionDTO.Events.IsWeaponValid:
            {
                var args = JsonConvert.DeserializeObject<WeaponCollectionDTO.IsWeaponValid>(request.Args);
                var value = ped.Weapons.IsWeaponValid((WeaponHash)args.Hash);
                BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, value.ToString());
                break;
            }

            case WeaponCollectionDTO.Events.Current:
            {
                var weapon = ped.Weapons.Current;
                BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, JsonConvert.SerializeObject(new WeaponCollectionDTO.CurrentFWeapon
                {
                    Hash = (FWeaponHash)weapon.Hash,
                    Ammo = weapon.Ammo,
                    AmmoInClip = weapon.AmmoInClip,
                    MaxAmmo = weapon.MaxAmmo,
                    MaxAmmoInClip = weapon.MaxAmmoInClip,
                    DefaultClipSize = weapon.DefaultClipSize,
                    DisplayName = weapon.DisplayName,
                    LocalizedName = weapon.LocalizedName,
                    CanUseOnParachute = weapon.CanUseOnParachute,
                }));
                break;
            }
        }
    }

    private async Task<Ped> FindPed(int networkId)
    {
        // TODO: only wait a few seconds
        while (!Cfx.API.NetworkDoesEntityExistWithNetworkId(networkId))
        {
            await BaseScript.Delay(10);
        }

        return Entity.FromNetworkId(networkId) as Ped;
    }
}