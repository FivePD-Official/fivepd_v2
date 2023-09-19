using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.EntityWrappers.PedWrapper;

#pragma warning disable VSTHRD200
#pragma warning disable SA1201

/// <summary>
/// Represents the properties of a <see cref="FPed"/>.
/// </summary>
public sealed class FPedProperties
{
    private readonly FPed _fPed;
    private readonly IEventService _eventService;

    private FGender _gender;
    private bool _fetchedGender;

    /// <summary>
    /// Initializes a new instance of the <see cref="FPedProperties"/> class.
    /// </summary>
    /// <param name="fPed">The <see cref="FPed"/> to use.</param>
    public FPedProperties(FPed fPed)
    {
        this._fPed = fPed;
        this._eventService = ServiceLocator.GetService<IEventService>();
        this._gender = this._fPed.Gender;
    }

    /// <summary>
    /// Gets the Accuracy.
    /// </summary>
    public Task<int> Accuracy => this.GetPropertyIntAsync(FPedPropertiesDTO.Events.Accuracy);

    /// <summary>
    /// Gets the Armor.
    /// </summary>
    public Task<int> Armor => this.GetPropertyIntAsync(FPedPropertiesDTO.Events.Armor);

    /// <summary>
    /// Gets the ArmorFloat.
    /// </summary>
    public Task<float> ArmorFloat => this.GetPropertyFloatAsync(FPedPropertiesDTO.Events.ArmorFloat);

    /// <summary>
    /// Gets the CanFlyThroughWindscreen.
    /// </summary>
    public Task<bool> CanFlyThroughWindscreen => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.CanFlyThroughWindscreen);

    /// <summary>
    /// Gets the CanRagdoll.
    /// </summary>
    public Task<bool> CanRagdoll => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.CanRagdoll);

    /// <summary>
    /// Gets the CanSufferCriticalHits.
    /// </summary>
    public Task<bool> CanSufferCriticalHits => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.CanSufferCriticalHits);

    /// <summary>
    /// Gets the CanWrithe.
    /// </summary>
    public Task<bool> CanWrithe => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.CanWrithe);

    /// <summary>
    /// Gets the DropsWeaponsOnDeath.
    /// </summary>
    public Task<bool> DropsWeaponsOnDeath => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.DropsWeaponsOnDeath);

    /// <summary>
    /// Gets the FatalInjuryHealthThreshold.
    /// </summary>
    public Task<float> FatalInjuryHealthThreshold => this.GetPropertyFloatAsync(FPedPropertiesDTO.Events.FatalInjuryHealthThreshold);

    /// <summary>
    /// Gets the gender.
    /// </summary>
    /// <returns>A task.</returns>
    public async Task<FGender> Gender()
    {
        if (this._fetchedGender)
        {
            return this._gender;
        }

        var gender = (FGender)await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.Gender);
        this._gender = gender;
        this._fPed.Gender = gender;
        this._fetchedGender = true;

        return gender;
    }

    /// <summary>
    /// Gets the CurrentVehicle.
    /// </summary>
    /// <returns>A task.</returns>
    public async Task<Vehicle> CurrentVehicle()
    {
        var vehicleNetworkId = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.CurrentVehicle);
        return Entity.FromNetworkId(vehicleNetworkId) as Vehicle;
    }

    /// <summary>
    /// Gets a value indicating whether the ped is dead.
    /// </summary>
    public bool IsDead => Cfx.API.GetEntityHealth(this._fPed.GetUnderlyingPed.Handle) <= 0;

    /// <summary>
    /// Gets a value indicating whether the ped is in a vehicle.
    /// </summary>
    public bool IsInVehicle => Cfx.API.GetVehiclePedIsIn(this._fPed.GetUnderlyingPed.Handle, false) != 0;

    /// <summary>
    /// Gets the InjuryHealthThreshold.
    /// </summary>
    public Task<float> InjuryHealthThreshold => this.GetPropertyFloatAsync(FPedPropertiesDTO.Events.InjuryHealthThreshold);

    /// <summary>
    /// Gets the IsAiming.
    /// </summary>
    public Task<bool> IsAiming => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsAiming);

    /// <summary>
    /// Gets the IsAimingFromCover.
    /// </summary>
    public Task<bool> IsAimingFromCover => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsAimingFromCover);

    /// <summary>
    /// Gets the IsAmbientSpeechEnabled.
    /// </summary>
    public Task<bool> IsAmbientSpeechEnabled => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsAmbientSpeechEnabled);

    /// <summary>
    /// Gets the IsAmbientSpeechplaying.
    /// </summary>
    public Task<bool> IsAmbientSpeechplaying => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsAmbientSpeechplaying);

    /// <summary>
    /// Gets the IsBeingJacked.
    /// </summary>
    public Task<bool> IsBeingJacked => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsBeingJacked);

    /// <summary>
    /// Gets the IsBeingStealthKilled.
    /// </summary>
    public Task<bool> IsBeingStealthKilled => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsBeingStealthKilled);

    /// <summary>
    /// Gets the IsBeingStunned.
    /// </summary>
    public Task<bool> IsBeingStunned => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsBeingStunned);

    /// <summary>
    /// Gets the IsClimbing.
    /// </summary>
    public Task<bool> IsClimbing => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsClimbing);

    /// <summary>
    /// Gets the IsCuffed.
    /// </summary>
    public Task<bool> IsCuffed => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsCuffed);

    /// <summary>
    /// Gets the IsDiving.
    /// </summary>
    public Task<bool> IsDiving => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsDiving);

    /// <summary>
    /// Gets the IsDoingDriveBy.
    /// </summary>
    public Task<bool> IsDoingDriveBy => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsDoingDriveBy);

    /// <summary>
    /// Gets the IsDucking.
    /// </summary>
    public Task<bool> IsDucking => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsDucking);

    /// <summary>
    /// Gets the IsFalling.
    /// </summary>
    public Task<bool> IsFalling => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsFalling);

    /// <summary>
    /// Gets the IsFleeing.
    /// </summary>
    public Task<bool> IsFleeing => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsFleeing);

    /// <summary>
    /// Gets the IsGettingIntoAVehicle.
    /// </summary>
    public Task<bool> IsGettingIntoAVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsGettingIntoAVehicle);

    /// <summary>
    /// Gets the IsGettingUp.
    /// </summary>
    public Task<bool> IsGettingUp => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsGettingUp);

    /// <summary>
    /// Gets the IsGoingIntoCover.
    /// </summary>
    public Task<bool> IsGoingIntoCover => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsGoingIntoCover);

    /// <summary>
    /// Gets the IsHuman.
    /// </summary>
    public Task<bool> IsHuman => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsHuman);

    /// <summary>
    /// Gets the IsIdle.
    /// </summary>
    public Task<bool> IsIdle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsIdle);

    /// <summary>
    /// Gets the IsInBoat.
    /// </summary>
    public Task<bool> IsInBoat => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInBoat);

    /// <summary>
    /// Gets the IsInCombat.
    /// </summary>
    public Task<bool> IsInCombat => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInCombat);

    /// <summary>
    /// Gets the IsInCoverFacingLeft.
    /// </summary>
    public Task<bool> IsInCoverFacingLeft => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInCoverFacingLeft);

    /// <summary>
    /// Gets the IsInFlyingVehicle.
    /// </summary>
    public Task<bool> IsInFlyingVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInFlyingVehicle);

    /// <summary>
    /// Gets the IsInGroup.
    /// </summary>
    public Task<bool> IsInGroup => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInGroup);

    /// <summary>
    /// Gets the IsInHeli.
    /// </summary>
    public Task<bool> IsInHeli => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInHeli);

    /// <summary>
    /// Gets the IsInjured.
    /// </summary>
    public Task<bool> IsInjured => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInjured);

    /// <summary>
    /// Gets the IsInMeleeCombat.
    /// </summary>
    public Task<bool> IsInMeleeCombat => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInMeleeCombat);

    /// <summary>
    /// Gets the IsInParachuteFreeFall.
    /// </summary>
    public Task<bool> IsInParachuteFreeFall => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInParachuteFreeFall);

    /// <summary>
    /// Gets the IsInPlane.
    /// </summary>
    public Task<bool> IsInPlane => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInPlane);

    /// <summary>
    /// Gets the IsInPoliceVehicle.
    /// </summary>
    public Task<bool> IsInPoliceVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInPoliceVehicle);

    /// <summary>
    /// Gets the IsInStealthMode.
    /// </summary>
    public Task<bool> IsInStealthMode => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInStealthMode);

    /// <summary>
    /// Gets the IsInSub.
    /// </summary>
    public Task<bool> IsInSub => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInSub);

    /// <summary>
    /// Gets the IsInTaxi.
    /// </summary>
    public Task<bool> IsInTaxi => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInTaxi);

    /// <summary>
    /// Gets the IsInTrain.
    /// </summary>
    public Task<bool> IsInTrain => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInTrain);

    /// <summary>
    /// Gets the IsJacking.
    /// </summary>
    public Task<bool> IsJacking => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsJacking);

    /// <summary>
    /// Gets the IsJumping.
    /// </summary>
    public Task<bool> IsJumping => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsJumping);

    /// <summary>
    /// Gets the IsJumpingOutOfVehicle.
    /// </summary>
    public Task<bool> IsJumpingOutOfVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsJumpingOutOfVehicle);

    /// <summary>
    /// Gets the IsOnBike.
    /// </summary>
    public Task<bool> IsOnBike => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsOnBike);

    /// <summary>
    /// Gets the IsOnFoot.
    /// </summary>
    public Task<bool> IsOnFoot => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsOnFoot);

    /// <summary>
    /// Gets the IsPerformingStealthKill.
    /// </summary>
    public Task<bool> IsPerformingStealthKill => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsPerformingStealthKill);

    /// <summary>
    /// Gets the IsPlantingBomb.
    /// </summary>
    public Task<bool> IsPlantingBomb => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsPlantingBomb);

    /// <summary>
    /// Gets the IsPlayer.
    /// </summary>
    public Task<bool> IsPlayer => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsPlayer);

    /// <summary>
    /// Gets the IsProne.
    /// </summary>
    public Task<bool> IsProne => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsProne);

    /// <summary>
    /// Gets the IsReloading.
    /// </summary>
    public Task<bool> IsReloading => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsReloading);

    /// <summary>
    /// Gets the IsRunning.
    /// </summary>
    public Task<bool> IsRunning => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsRunning);

    /// <summary>
    /// Gets the IsScriptedSpeechplaying.
    /// </summary>
    public Task<bool> IsScriptedSpeechplaying => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsScriptedSpeechplaying);

    /// <summary>
    /// Gets the IsShooting.
    /// </summary>
    public Task<bool> IsShooting => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsShooting);

    /// <summary>
    /// Gets the IsSprinting.
    /// </summary>
    public Task<bool> IsSprinting => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsSprinting);

    /// <summary>
    /// Gets the IsStopped.
    /// </summary>
    public Task<bool> IsStopped => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsStopped);

    /// <summary>
    /// Gets the IsSwimming.
    /// </summary>
    public Task<bool> IsSwimming => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsSwimming);

    /// <summary>
    /// Gets the IsSwimmingUnderWater.
    /// </summary>
    public Task<bool> IsSwimmingUnderWater => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsSwimmingUnderWater);

    /// <summary>
    /// Gets the IsTryingToEnterALockedVehicle.
    /// </summary>
    public Task<bool> IsTryingToEnterALockedVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsTryingToEnterALockedVehicle);

    /// <summary>
    /// Gets the IsVaulting.
    /// </summary>
    public Task<bool> IsVaulting => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsVaulting);

    /// <summary>
    /// Gets the IsWalking.
    /// </summary>
    public Task<bool> IsWalking => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsWalking);

    /// <summary>
    /// Gets the IsWearingHelmet.
    /// </summary>
    public Task<bool> IsWearingHelmet => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsWearingHelmet);

    /// <summary>
    /// Gets the LastVehicle.
    /// </summary>
    /// <returns>A task.</returns>
    public async Task<Vehicle> LastVehicle()
    {
        var vehicleNetworkId = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.LastVehicle);
        return Entity.FromNetworkId(vehicleNetworkId) as Vehicle;
    }

    /// <summary>
    /// Gets the Money.
    /// </summary>
    public Task<int> Money => this.GetPropertyIntAsync(FPedPropertiesDTO.Events.Money);

    /// <summary>
    /// Gets the SeatIndex.
    /// </summary>
    /// <returns>A task.</returns>
    public async Task<FVehicleSeat> SeatIndex()
    {
        var seat = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.IsWearingHelmet);
        return (FVehicleSeat)seat;
    }

    /// <summary>
    /// Gets the Sweat.
    /// </summary>
    public Task<float> Sweat => this.GetPropertyFloatAsync(FPedPropertiesDTO.Events.Sweat);

    /// <summary>
    /// Gets the VehicleTryingToEnter.
    /// </summary>
    /// <returns>A task.</returns>
    public async Task<Vehicle> VehicleTryingToEnter()
    {
        var vehicleNetworkId = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.VehicleTryingToEnter);
        return Entity.FromNetworkId(vehicleNetworkId) as Vehicle;
    }

    /// <summary>
    /// Gets the SeatIndex.
    /// </summary>
    /// <returns>A task.</returns>
    public async Task<FWeaponHash> VehicleWeapon()
    {
        var seat = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.VehicleWeapon);
        return (FWeaponHash)seat;
    }

    /// <summary>
    /// Gets the WasKilledByStealth.
    /// </summary>
    public Task<bool> WasKilledByStealth => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.WasKilledByStealth);

    /// <summary>
    /// Gets the WasKilledByTakedown.
    /// </summary>
    public Task<bool> WasKilledByTakedown => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.WasKilledByTakedown);

    /// <summary>
    /// Sets the "Accuracy" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetAccuracy(int value) => this.SetProperty(FPedPropertiesDTO.Events.Accuracy, value);

    /// <summary>
    /// Sets the "AlwaysDiesOnLowHealth" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetAlwaysDiesOnLowHealth(bool value) => this.SetProperty(FPedPropertiesDTO.Events.AlwaysDiesOnLowHealth, value);

    /// <summary>
    /// Sets the "AlwaysKeepTask" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetAlwaysKeepTask(bool value) => this.SetProperty(FPedPropertiesDTO.Events.AlwaysKeepTask, value);

    /// <summary>
    /// Sets the "Armor" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetArmor(int value) => this.SetProperty(FPedPropertiesDTO.Events.Armor, value);

    /// <summary>
    /// Sets the "ArmorFloat" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetArmorFloat(float value) => this.SetProperty(FPedPropertiesDTO.Events.ArmorFloat, value);

    /// <summary>
    /// Sets the "BlockPermanentEvents" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetBlockPermanentEvents(bool value) => this.SetProperty(FPedPropertiesDTO.Events.BlockPermanentEvents, value);

    /// <summary>
    /// Sets the "CanBeDraggedOutOfVehicle" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanBeDraggedOutOfVehicle(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanBeDraggedOutOfVehicle, value);

    /// <summary>
    /// Sets the "CanBeKnockedOffBike" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanBeKnockedOffBike(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanBeKnockedOffBike, value);

    /// <summary>
    /// Sets the "CanBeShotInVehicle" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanBeShotInVehicle(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanBeShotInVehicle, value);

    /// <summary>
    /// Sets the "CanBeTargetted" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanBeTargetted(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanBeTargetted, value);

    /// <summary>
    /// Sets the "CanFlyThroughWindscreen" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanFlyThroughWindscreen(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanFlyThroughWindscreen, value);

    /// <summary>
    /// Sets the "CanPlayGestures" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanPlayGestures(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanPlayGestures, value);

    /// <summary>
    /// Sets the "CanRagdoll" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanRagdoll(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanRagdoll, value);

    /// <summary>
    /// Sets the "CanSufferCriticalHits" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanSufferCriticalHits(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanSufferCriticalHits, value);

    /// <summary>
    /// Sets the "CanSwitchWeapons" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanSwitchWeapons(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanSwitchWeapons, value);

    /// <summary>
    /// Sets the "CanWearHelmet" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanWearHelmet(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanWearHelmet, value);

    /// <summary>
    /// Sets the "CanWrithe" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetCanWrithe(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanWrithe, value);

    /// <summary>
    /// Sets the "DiesInstantlyInWater" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetDiesInstantlyInWater(bool value) => this.SetProperty(FPedPropertiesDTO.Events.DiesInstantlyInWater, value);

    /// <summary>
    /// Sets the "DrivingSpeed" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetDrivingSpeed(float value) => this.SetProperty(FPedPropertiesDTO.Events.DrivingSpeed, value);

    /// <summary>
    /// Sets the "DrivingStyle" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetDrivingStyle(FDrivingStyle value) => this.SetProperty(FPedPropertiesDTO.Events.DrivingStyle, value);

    /// <summary>
    /// Sets the "DropsWeaponsOnDeath" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetDropsWeaponsOnDeath(bool value) => this.SetProperty(FPedPropertiesDTO.Events.DropsWeaponsOnDeath, value);

    /// <summary>
    /// Sets the "DrownsInSinkingVehicle" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetDrownsInSinkingVehicle(bool value) => this.SetProperty(FPedPropertiesDTO.Events.DrownsInSinkingVehicle, value);

    /// <summary>
    /// Sets the "DrownsInWater" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetDrownsInWater(bool value) => this.SetProperty(FPedPropertiesDTO.Events.DrownsInWater, value);

    /// <summary>
    /// Sets the "FatalInjuryHealthThreshold" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetFatalInjuryHealthThreshold(float value) => this.SetProperty(FPedPropertiesDTO.Events.FatalInjuryHealthThreshold, value);

    /// <summary>
    /// Sets the "InjuryHealthThreshold" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetInjuryHealthThreshold(float value) => this.SetProperty(FPedPropertiesDTO.Events.InjuryHealthThreshold, value);

    /// <summary>
    /// Sets the "IsDucking" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetIsDucking(float value) => this.SetProperty(FPedPropertiesDTO.Events.IsDucking, value);

    /// <summary>
    /// Sets the "IsEnemy" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetIsEnemy(float value) => this.SetProperty(FPedPropertiesDTO.Events.IsEnemy, value);

    /// <summary>
    /// Sets the "IsPainAudioEnabled" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetIsPainAudioEnabled(bool value) => this.SetProperty(FPedPropertiesDTO.Events.IsPainAudioEnabled, value);

    /// <summary>
    /// Sets the "IsPriorityTargetForEnemies" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetIsPriorityTargetForEnemies(bool value) => this.SetProperty(FPedPropertiesDTO.Events.IsPriorityTargetForEnemies, value);

    /// <summary>
    /// Sets the "MaxDrivingSpeed" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetMaxDrivingSpeed(float value) => this.SetProperty(FPedPropertiesDTO.Events.MaxDrivingSpeed, value);

    /// <summary>
    /// Sets the "Money" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetMoney(int value) => this.SetProperty(FPedPropertiesDTO.Events.Money, value);

    /// <summary>
    /// Sets the "MovementAnimationSet" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetMovementAnimationSet(string value) => this.SetProperty(FPedPropertiesDTO.Events.MovementAnimationSet, value);

    /// <summary>
    /// Sets the "NeverLeavesGroup" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetNeverLeavesGroup(bool value) => this.SetProperty(FPedPropertiesDTO.Events.NeverLeavesGroup, value);

    /// <summary>
    /// Sets the "ShootRate" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetShootRate(int value) => this.SetProperty(FPedPropertiesDTO.Events.ShootRate, value);

    /// <summary>
    /// Sets the "StaysInVehicleWhenJacked" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetStaysInVehicleWhenJacked(bool value) => this.SetProperty(FPedPropertiesDTO.Events.StaysInVehicleWhenJacked, value);

    /// <summary>
    /// Sets the "Sweat" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetSweat(float value) => this.SetProperty(FPedPropertiesDTO.Events.Sweat, value);

    /// <summary>
    /// Sets the "VehicleDrivingFlags" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetVehicleDrivingFlags(FVehicleDrivingFlags value) => this.SetProperty(FPedPropertiesDTO.Events.VehicleDrivingFlags, value);

    /// <summary>
    /// Sets the "Voice" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetVoice(string value) => this.SetProperty(FPedPropertiesDTO.Events.Voice, value);

    /// <summary>
    /// Sets the "WetnessHeight" property of the ped.
    /// </summary>
    /// <param name="value">Value.</param>
    public void SetWetnessHeight(float value) => this.SetProperty(FPedPropertiesDTO.Events.WetnessHeight, value);

    /// <summary>
    /// A.
    /// </summary>
    /// <param name="vehicle">B.</param>
    /// <param name="vehicleSeat">C.</param>
    public void SetIntoVehicle(Vehicle vehicle, FVehicleSeat vehicleSeat) => this.InvokeMethod(
        FPedPropertiesDTO.Events.SetIntoVehicle,
        new FPedPropertiesDTO.SetIntoVehicle
        {
            VehicleNetworkId = vehicle.NetworkId,
            VehicleSeat = (int)vehicleSeat,
        });

    /// <summary>
    /// Invokes the "MarkAsNoLongerNeeded" method on the ped.
    /// </summary>
    public void MarkAsNoLongerNeeded() => this.InvokeMethod<object>(FPedPropertiesDTO.Events.MarkAsNoLongerNeeded);

    /// <summary>
    /// Creates a blip for the <see cref="FPed"/>.
    /// </summary>
    /// <param name="sprite">The <see cref="FBlipSprite"/> to use.</param>
    /// <param name="color">The <see cref="FBlipColor"/> to use.</param>
    /// <param name="text">The text to display on the blip. If <see langword="null"/>, the blip will not have any text.</param>
    public void CreateBlip(FBlipSprite sprite, FBlipColor color, string text)
    {
        this.SetProperty(FPedPropertiesDTO.Events.CreateBlip, JsonConvert.SerializeObject(new FPedPropertiesDTO.CreateBlipRequest
        {
            Sprite = sprite,
            Color = color,
            Text = text,
        }));
    }

    /// <summary>
    /// Removes all blips from the <see cref="FPed"/>.
    /// </summary>
    public void RemoveBlips()
    {
        this.SetProperty(FPedPropertiesDTO.Events.RemoveBlips, string.Empty);
    }

    private async Task<int> GetPropertyIntAsync(string property)
    {
        var value = await this._eventService.TriggerEventAsync(this._fPed.Owner, Events.Ped.GetProperty, this._fPed.NetworkId, property);
        return int.Parse(value);
    }

    private async Task<float> GetPropertyFloatAsync(string property)
    {
        var value = await this._eventService.TriggerEventAsync(this._fPed.Owner, Events.Ped.GetProperty, this._fPed.NetworkId, property);
        return float.Parse(value);
    }

    private async Task<bool> GetPropertyBoolAsync(string property)
    {
        var value = await this._eventService.TriggerEventAsync(this._fPed.Owner, Events.Ped.GetProperty, this._fPed.NetworkId, property);
        return bool.Parse(value);
    }

    private void SetProperty<T>(string property, T value)
    {
        this._eventService.TriggerEventWithoutCallback(this._fPed.Owner, Events.Ped.SetProperty, this._fPed.NetworkId, property, value.ToString());
    }

    private void InvokeMethod<T>(string method, T args = default)
    {
        this._eventService.TriggerEventWithoutCallback(
            this._fPed.Owner,
            Events.Ped.InvokeMethod,
            JsonConvert.SerializeObject(new FPedPropertiesDTO.InvokeMethodRequest
            {
                NetworkId = this._fPed.NetworkId,
                Method = method,
                Args = JsonConvert.SerializeObject(args),
            }));
    }
}