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
/// Represents the properties of a <see cref="FPed"/>, providing access to various ped attributes and behaviors.
/// </summary>
public sealed class FPedProperties
{
    private readonly FPed _fPed;
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FPedProperties"/> class.
    /// </summary>
    /// <param name="fPed">The <see cref="FPed"/> instance these properties belong to.</param>
    internal FPedProperties(FPed fPed)
    {
        this._fPed = fPed;
        this._eventService = ServiceLocator.GetService<IEventService>();
    }

    /// <summary>
    /// Gets the ped's accuracy value, affecting their ability to hit targets.
    /// </summary>
    public Task<int> Accuracy => this.GetPropertyIntAsync(FPedPropertiesDTO.Events.Accuracy);

    /// <summary>
    /// Gets the ped's armor value as an integer.
    /// </summary>
    public Task<int> Armor => this.GetPropertyIntAsync(FPedPropertiesDTO.Events.Armor);

    /// <summary>
    /// Gets the ped's armor value as a float.
    /// </summary>
    public Task<float> ArmorFloat => this.GetPropertyFloatAsync(FPedPropertiesDTO.Events.ArmorFloat);

    /// <summary>
    /// Gets a value indicating whether the ped can be ejected through the windscreen in vehicle collisions.
    /// </summary>
    public Task<bool> CanFlyThroughWindscreen => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.CanFlyThroughWindscreen);

    /// <summary>
    /// Gets the ped's armor value as a float.
    /// </summary>
    public Task<bool> CanRagdoll => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.CanRagdoll);

    /// <summary>
    /// Gets the ped's armor value as a float.
    /// </summary>
    public Task<bool> CanSufferCriticalHits => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.CanSufferCriticalHits);

    /// <summary>
    /// Gets the ped's armor value as a float.
    /// </summary>
    public Task<bool> CanWrithe => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.CanWrithe);

    /// <summary>
    /// Gets the ped's armor value as a float.
    /// </summary>
    public Task<bool> DropsWeaponsOnDeath => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.DropsWeaponsOnDeath);

    /// <summary>
    /// Gets the ped's armor value as a float.
    /// </summary>
    public Task<float> FatalInjuryHealthThreshold => this.GetPropertyFloatAsync(FPedPropertiesDTO.Events.FatalInjuryHealthThreshold);

    /// <summary>
    /// Gets the vehicle the ped is currently in, if any.
    /// </summary>
    /// <returns>A task that resolves to the current vehicle, or null if not in a vehicle.</returns>
    public async Task<Vehicle> CurrentVehicle()
    {
        var vehicleNetworkId = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.CurrentVehicle);
        return Entity.FromNetworkId(vehicleNetworkId) as Vehicle;
    }

    /// <summary>
    /// Gets a value indicating whether the ped is dead.
    /// </summary>
    public bool IsDead => Cfx.API.GetEntityHealth(this._fPed.GetUnderlyingPed?.Handle ?? 0) <= 0;

    /// <summary>
    /// Gets a value indicating whether the ped is in a vehicle.
    /// </summary>
    public bool IsInVehicle => Cfx.API.GetVehiclePedIsIn(this._fPed.GetUnderlyingPed?.Handle ?? 0, false) != 0;

    /// <summary>
    /// Gets the health threshold at which the ped will be considered injured.
    /// </summary>
    public Task<float> InjuryHealthThreshold => this.GetPropertyFloatAsync(FPedPropertiesDTO.Events.InjuryHealthThreshold);

    /// <summary>
    /// Gets a value indicating whether the ped is currently aiming a weapon.
    /// </summary>
    public Task<bool> IsAiming => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsAiming);

    /// <summary>
    /// Gets a value indicating whether the ped is aiming while in cover.
    /// </summary>
    public Task<bool> IsAimingFromCover => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsAimingFromCover);

    /// <summary>
    /// Gets a value indicating whether ambient speech is enabled for the ped.
    /// </summary>
    public Task<bool> IsAmbientSpeechEnabled => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsAmbientSpeechEnabled);

    /// <summary>
    /// Gets a value indicating whether the ped is currently playing ambient speech.
    /// </summary>
    public Task<bool> IsAmbientSpeechplaying => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsAmbientSpeechplaying);

    /// <summary>
    /// Gets a value indicating whether the ped is currently being carjacked.
    /// </summary>
    public Task<bool> IsBeingJacked => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsBeingJacked);

    /// <summary>
    /// Gets a value indicating whether the ped is being stealth killed.
    /// </summary>
    public Task<bool> IsBeingStealthKilled => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsBeingStealthKilled);

    /// <summary>
    /// Gets a value indicating whether the ped is being stunned.
    /// </summary>
    public Task<bool> IsBeingStunned => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsBeingStunned);

    /// <summary>
    /// Gets a value indicating whether the ped is climbing.
    /// </summary>
    public Task<bool> IsClimbing => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsClimbing);

    /// <summary>
    /// Gets a value indicating whether the ped is handcuffed.
    /// </summary>
    public Task<bool> IsCuffed => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsCuffed);

    /// <summary>
    /// Gets a value indicating whether the ped is diving.
    /// </summary>
    public Task<bool> IsDiving => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsDiving);

    /// <summary>
    /// Gets a value indicating whether the ped is performing a drive-by shooting.
    /// </summary>
    public Task<bool> IsDoingDriveBy => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsDoingDriveBy);

    /// <summary>
    /// Gets a value indicating whether the ped is in a ducking stance.
    /// </summary>
    public Task<bool> IsDucking => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsDucking);

    /// <summary>
    /// Gets a value indicating whether the ped is falling.
    /// </summary>
    public Task<bool> IsFalling => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsFalling);

    /// <summary>
    /// Gets a value indicating whether the ped is fleeing from a threat.
    /// </summary>
    public Task<bool> IsFleeing => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsFleeing);

    /// <summary>
    /// Gets a value indicating whether the ped is entering a vehicle.
    /// </summary>
    public Task<bool> IsGettingIntoAVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsGettingIntoAVehicle);

    /// <summary>
    /// Gets a value indicating whether the ped is getting up from the ground.
    /// </summary>
    public Task<bool> IsGettingUp => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsGettingUp);

    /// <summary>
    /// Gets a value indicating whether the ped is moving to take cover.
    /// </summary>
    public Task<bool> IsGoingIntoCover => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsGoingIntoCover);

    /// <summary>
    /// Gets a value indicating whether the ped is a human (as opposed to an animal).
    /// </summary>
    public Task<bool> IsHuman => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsHuman);

    /// <summary>
    /// Gets a value indicating whether the ped is in an idle state.
    /// </summary>
    public Task<bool> IsIdle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsIdle);

    /// <summary>
    /// Gets a value indicating whether the ped is in a boat.
    /// </summary>
    public Task<bool> IsInBoat => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInBoat);

    /// <summary>
    /// Gets a value indicating whether the ped is engaged in combat.
    /// </summary>
    public Task<bool> IsInCombat => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInCombat);

    /// <summary>
    /// Gets a value indicating whether the ped is in cover and facing left.
    /// </summary>
    public Task<bool> IsInCoverFacingLeft => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInCoverFacingLeft);

    /// <summary>
    /// Gets a value indicating whether the ped is in a flying vehicle.
    /// </summary>
    public Task<bool> IsInFlyingVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInFlyingVehicle);

    /// <summary>
    /// Gets a value indicating whether the ped is part of a group.
    /// </summary>
    public Task<bool> IsInGroup => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInGroup);

    /// <summary>
    /// Gets a value indicating whether the ped is in a helicopter.
    /// </summary>
    public Task<bool> IsInHeli => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInHeli);

    /// <summary>
    /// Gets a value indicating whether the ped is injured.
    /// </summary>
    public Task<bool> IsInjured => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInjured);

    /// <summary>
    /// Gets a value indicating whether the ped is engaged in melee combat.
    /// </summary>
    public Task<bool> IsInMeleeCombat => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInMeleeCombat);

    /// <summary>
    /// Gets a value indicating whether the ped is in a parachute free fall.
    /// </summary>
    public Task<bool> IsInParachuteFreeFall => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInParachuteFreeFall);

    /// <summary>
    /// Gets a value indicating whether the ped is in a plane.
    /// </summary>
    public Task<bool> IsInPlane => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInPlane);

    /// <summary>
    /// Gets a value indicating whether the ped is in a police vehicle.
    /// </summary>
    public Task<bool> IsInPoliceVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInPoliceVehicle);

    /// <summary>
    /// Gets a value indicating whether the ped is in stealth mode.
    /// </summary>
    public Task<bool> IsInStealthMode => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInStealthMode);

    /// <summary>
    /// Gets a value indicating whether the ped is in a submarine.
    /// </summary>
    public Task<bool> IsInSub => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInSub);

    /// <summary>
    /// Gets a value indicating whether the ped is in a taxi.
    /// </summary>
    public Task<bool> IsInTaxi => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInTaxi);

    /// <summary>
    /// Gets a value indicating whether the ped is in a train.
    /// </summary>
    public Task<bool> IsInTrain => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInTrain);

    /// <summary>
    /// Gets a value indicating whether the ped is carjacking another vehicle.
    /// </summary>
    public Task<bool> IsJacking => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsJacking);

    /// <summary>
    /// Gets a value indicating whether the ped is jumping.
    /// </summary>
    public Task<bool> IsJumping => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsJumping);

    /// <summary>
    /// Gets a value indicating whether the ped is jumping out of a vehicle.
    /// </summary>
    public Task<bool> IsJumpingOutOfVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsJumpingOutOfVehicle);

    /// <summary>
    /// Gets a value indicating whether the ped is on a bike.
    /// </summary>
    public Task<bool> IsOnBike => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsOnBike);

    /// <summary>
    /// Gets a value indicating whether the ped is on foot.
    /// </summary>
    public Task<bool> IsOnFoot => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsOnFoot);

    /// <summary>
    /// Gets a value indicating whether the ped is performing a stealth kill.
    /// </summary>
    public Task<bool> IsPerformingStealthKill => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsPerformingStealthKill);

    /// <summary>
    /// Gets a value indicating whether the ped is planting a bomb.
    /// </summary>
    public Task<bool> IsPlantingBomb => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsPlantingBomb);

    /// <summary>
    /// Gets a value indicating whether the ped is a player character.
    /// </summary>
    public Task<bool> IsPlayer => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsPlayer);

    /// <summary>
    /// Gets a value indicating whether the ped is prone.
    /// </summary>
    public Task<bool> IsProne => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsProne);

    /// <summary>
    /// Gets a value indicating whether the ped is reloading their weapon.
    /// </summary>
    public Task<bool> IsReloading => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsReloading);

    /// <summary>
    /// Gets a value indicating whether the ped is running.
    /// </summary>
    public Task<bool> IsRunning => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsRunning);

    /// <summary>
    /// Gets a value indicating whether the ped is playing scripted speech.
    /// </summary>
    public Task<bool> IsScriptedSpeechplaying => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsScriptedSpeechplaying);

    /// <summary>
    /// Gets a value indicating whether the ped is shooting.
    /// </summary>
    public Task<bool> IsShooting => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsShooting);

    /// <summary>
    /// Gets a value indicating whether the ped is sprinting.
    /// </summary>
    public Task<bool> IsSprinting => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsSprinting);

    /// <summary>
    /// Gets a value indicating whether the ped is stopped.
    /// </summary>
    public Task<bool> IsStopped => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsStopped);

    /// <summary>
    /// Gets a value indicating whether the ped is swimming.
    /// </summary>
    public Task<bool> IsSwimming => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsSwimming);

    /// <summary>
    /// Gets a value indicating whether the ped is swimming underwater.
    /// </summary>
    public Task<bool> IsSwimmingUnderWater => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsSwimmingUnderWater);

    /// <summary>
    /// Gets a value indicating whether the ped is attempting to enter a locked vehicle.
    /// </summary>
    public Task<bool> IsTryingToEnterALockedVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsTryingToEnterALockedVehicle);

    /// <summary>
    /// Gets a value indicating whether the ped is vaulting over an obstacle.
    /// </summary>
    public Task<bool> IsVaulting => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsVaulting);

    /// <summary>
    /// Gets a value indicating whether the ped is walking.
    /// </summary>
    public Task<bool> IsWalking => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsWalking);

    /// <summary>
    /// Gets a value indicating whether the ped is wearing a helmet.
    /// </summary>
    public Task<bool> IsWearingHelmet => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsWearingHelmet);

    /// <summary>
    /// Gets the last vehicle the ped was in.
    /// </summary>
    /// <returns>A task that resolves to the last vehicle, or null if none.</returns>
    public async Task<Vehicle> LastVehicle()
    {
        var vehicleNetworkId = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.LastVehicle);
        return Entity.FromNetworkId(vehicleNetworkId) as Vehicle;
    }

    /// <summary>
    /// Gets the amount of money the ped is carrying.
    /// </summary>
    public Task<int> Money => this.GetPropertyIntAsync(FPedPropertiesDTO.Events.Money);

    /// <summary>
    /// Gets the ped's current seat index in their vehicle.
    /// </summary>
    /// <returns>A task that resolves to the vehicle seat position.</returns>
    public async Task<FVehicleSeat> SeatIndex()
    {
        var seat = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.IsWearingHelmet);
        return (FVehicleSeat)seat;
    }

    /// <summary>
    /// Gets the ped's sweat level.
    /// </summary>
    public Task<float> Sweat => this.GetPropertyFloatAsync(FPedPropertiesDTO.Events.Sweat);

    /// <summary>
    /// Gets the vehicle the ped is attempting to enter.
    /// </summary>
    /// <returns>A task that resolves to the target vehicle, or null if none.</returns>
    public async Task<Vehicle> VehicleTryingToEnter()
    {
        var vehicleNetworkId = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.VehicleTryingToEnter);
        return Entity.FromNetworkId(vehicleNetworkId) as Vehicle;
    }

    /// <summary>
    /// Gets the weapon hash of the ped's vehicle weapon.
    /// </summary>
    /// <returns>A task that resolves to the weapon hash.</returns>
    public async Task<FWeaponHash> VehicleWeapon()
    {
        var seat = await this.GetPropertyIntAsync(FPedPropertiesDTO.Events.VehicleWeapon);
        return (FWeaponHash)seat;
    }

    /// <summary>
    /// Gets a value indicating whether the ped was killed by a stealth attack.
    /// </summary>
    public Task<bool> WasKilledByStealth => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.WasKilledByStealth);

    /// <summary>
    /// Gets a value indicating whether the ped was killed by a takedown.
    /// </summary>
    public Task<bool> WasKilledByTakedown => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.WasKilledByTakedown);

    /// <summary>
    /// Sets the accuracy value for the ped, affecting their ability to hit targets.
    /// </summary>
    /// <param name="value">The accuracy value to set.</param>
    public void SetAccuracy(int value) => this.SetProperty(FPedPropertiesDTO.Events.Accuracy, value);

    /// <summary>
    /// Sets whether the ped should always die when their health is low.
    /// </summary>
    /// <param name="value">True to make the ped die on low health, false otherwise.</param>
    public void SetAlwaysDiesOnLowHealth(bool value) => this.SetProperty(FPedPropertiesDTO.Events.AlwaysDiesOnLowHealth, value);

    /// <summary>
    /// Sets whether the ped should always keep their current task.
    /// </summary>
    /// <param name="value">True to make the ped keep their current task, false otherwise.</param>
    public void SetAlwaysKeepTask(bool value) => this.SetProperty(FPedPropertiesDTO.Events.AlwaysKeepTask, value);

    /// <summary>
    /// Sets the armor value for the ped.
    /// </summary>
    /// <param name="value">The armor value to set.</param>
    public void SetArmor(int value) => this.SetProperty(FPedPropertiesDTO.Events.Armor, value);

    /// <summary>
    /// Sets the armor value for the ped as a float.
    /// </summary>
    /// <param name="value">The armor value to set.</param>
    public void SetArmorFloat(float value) => this.SetProperty(FPedPropertiesDTO.Events.ArmorFloat, value);

    /// <summary>
    /// Sets whether the ped should block permanent events.
    /// </summary>
    /// <param name="value">True to block permanent events, false otherwise.</param>
    public void SetBlockPermanentEvents(bool value) => this.SetProperty(FPedPropertiesDTO.Events.BlockPermanentEvents, value);

    /// <summary>
    /// Sets whether the ped can be dragged out of vehicles.
    /// </summary>
    /// <param name="value">True to allow being dragged out, false otherwise.</param>
    public void SetCanBeDraggedOutOfVehicle(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanBeDraggedOutOfVehicle, value);

    /// <summary>
    /// Sets whether the ped can be knocked off bikes.
    /// </summary>
    /// <param name="value">True to allow being knocked off bikes, false otherwise.</param>
    public void SetCanBeKnockedOffBike(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanBeKnockedOffBike, value);

    /// <summary>
    /// Sets whether the ped can be shot while in a vehicle.
    /// </summary>
    /// <param name="value">True to allow being shot in vehicles, false otherwise.</param>
    public void SetCanBeShotInVehicle(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanBeShotInVehicle, value);

    /// <summary>
    /// Sets whether the ped can be targeted by other entities.
    /// </summary>
    /// <param name="value">True to allow being targeted, false otherwise.</param>
    public void SetCanBeTargetted(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanBeTargetted, value);

    /// <summary>
    /// Sets whether the ped can fly through windscreens in collisions.
    /// </summary>
    /// <param name="value">True to allow flying through windscreens, false otherwise.</param>
    public void SetCanFlyThroughWindscreen(bool value) => this.SetProperty(FPedPropertiesDTO.Events.CanFlyThroughWindscreen, value);

    /// <summary>
    /// Sets whether the ped can perform gesture animations.
    /// </summary>
    /// <param name="value">True to allow gesture animations, false otherwise.</param>
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
    /// Places the ped into a specific vehicle seat.
    /// </summary>
    /// <param name="vehicle">The vehicle to enter.</param>
    /// <param name="vehicleSeat">The seat position to use.</param>
    public void SetIntoVehicle(Vehicle vehicle, FVehicleSeat vehicleSeat) => this.InvokeMethod(
        FPedPropertiesDTO.Events.SetIntoVehicle,
        new FPedPropertiesDTO.SetIntoVehicle
        {
            VehicleNetworkId = vehicle.NetworkId,
            VehicleSeat = (int)vehicleSeat,
        });

    /// <summary>
    /// Marks the ped as no longer needed, allowing it to be cleaned up by the game.
    /// </summary>
    public void MarkAsNoLongerNeeded() => this.InvokeMethod<object>(FPedPropertiesDTO.Events.MarkAsNoLongerNeeded);

    /// <summary>
    /// Creates a blip on the map for this ped.
    /// </summary>
    /// <param name="sprite">The sprite to use for the blip.</param>
    /// <param name="color">The color to use for the blip.</param>
    /// <param name="text">Optional text to display on the blip.</param>
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
    /// Removes all blips associated with this ped.
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