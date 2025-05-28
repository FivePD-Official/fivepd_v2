using FivePD.Common.Attributes;
using FivePD.Common.Enums;

namespace FivePD.Common.DTO;

#pragma warning disable

[Obfuscate]
internal sealed record FPedPropertiesDTO
{
    public record struct InvokeMethodRequest
    {
        public int NetworkId { get; set; }
    
        public string Method { get; set; }
    
        public string Args { get; set; }
    }

    public record struct CreateBlipRequest
    {
        public required FBlipSprite Sprite { get; init; }

        public required FBlipColor Color { get; init; }

        public required string Text { get; init; }
    }
    
    public record struct SetIntoVehicle
    {
        public required int VehicleNetworkId { get; init; }
    
        public required int VehicleSeat { get; init; }
    }

    public sealed record Events
    {
        // Properties
        public const string Accuracy = "Accuracy"; // int get set
        public const string AlwaysDiesOnLowHealth = "AlwaysDiesOnLowHealth"; // bool set
        public const string AlwaysKeepTask = "AlwaysKeepTask"; // bool set
        public const string Armor = "Armor"; // int get set
        public const string ArmorFloat = "ArmorFloat"; // float get set
        public const string BlockPermanentEvents = "BlockPermanentEvents"; // bool set
        public const string CanBeDraggedOutOfVehicle = "CanBeDraggedOutOfVehicle"; // bool set
        public const string CanBeKnockedOffBike = "CanBeKnockedOffBike"; // bool set
        public const string CanBeShotInVehicle = "CanBeShotInVehicle"; // bool set
        public const string CanBeTargetted = "CanBeTargetted"; // bool set
        public const string CanFlyThroughWindscreen = "CanFlyThroughWindscreen"; // bool get set
        public const string CanPlayGestures = "CanPlayGestures"; // bool set
        public const string CanRagdoll = "CanRagdoll"; // bool get set
        public const string CanSufferCriticalHits = "CanSufferCriticalHits"; // bool get set
        public const string CanSwitchWeapons = "CanSwitchWeapons"; // bool set
        public const string CanWearHelmet = "CanWearHelmet"; // bool set
        public const string CanWrithe = "CanWrithe"; // bool get set
        public const string CurrentVehicle = "CurrentVehicle"; // Vehicle get
        public const string DiesInstantlyInWater = "DiesInstantlyInWater"; // bool set
        public const string DrivingSpeed = "DrivingSpeed"; // float set
        public const string DrivingStyle = "DrivingStyle"; // DrivingStyle set
        public const string DropsWeaponsOnDeath = "DropsWeaponsOnDeath"; // bool get set
        public const string DrownsInSinkingVehicle = "DrownsInSinkingVehicle"; // bool set
        public const string DrownsInWater = "DrownsInWater"; // bool set
        public const string FatalInjuryHealthThreshold = "FatalInjuryHealthThreshold"; // float get set
        public const string FiringPattern = "FiringPattern"; // FiringPattern set
        public const string Gender = "Gender"; // Gender get
        public const string InjuryHealthThreshold = "InjuryHealthThreshold"; // float get set
        public const string IsAiming = "IsAiming"; // bool get
        public const string IsAimingFromCover = "IsAimingFromCover"; // bool get
        public const string IsAmbientSpeechEnabled = "IsAmbientSpeechEnabled"; // bool get
        public const string IsAmbientSpeechplaying = "IsAmbientSpeechplaying"; // bool get
        public const string IsBeingJacked = "IsBeingJacked"; // bool get
        public const string IsBeingStealthKilled = "IsBeingStealthKilled"; // bool get
        public const string IsBeingStunned = "IsBeingStunned"; // bool get
        public const string IsClimbing = "IsClimbing"; // bool get
        public const string IsCuffed = "IsCuffed"; // bool get
        public const string IsDiving = "IsDiving"; // bool get
        public const string IsDoingDriveBy = "IsDoingDriveBy"; // bool get
        public const string IsDucking = "IsDucking"; // bool get set
        public const string IsEnemy = "IsEnemy"; // bool set
        public const string IsFalling = "IsFalling"; // bool get
        public const string IsFleeing = "IsFleeing"; // bool get
        public const string IsGettingIntoAVehicle = "IsGettingIntoAVehicle"; // bool get
        public const string IsGettingUp = "IsGettingUp"; // bool get
        public const string IsGoingIntoCover = "IsGoingIntoCover"; // bool get
        public const string IsHuman = "IsHuman"; // bool get
        public const string IsIdle = "IsIdle"; // bool get
        public const string IsInBoat = "IsInBoat"; // bool get
        public const string IsInCombat = "IsInCombat"; // bool get
        public const string IsInCoverFacingLeft = "IsInCoverFacingLeft"; // bool get
        public const string IsInFlyingVehicle = "IsInFlyingVehicle"; // bool get
        public const string IsInGroup = "IsInGroup"; // bool get
        public const string IsInHeli = "IsInHeli"; // bool get
        public const string IsInjured = "IsInjured"; // bool get
        public const string IsInMeleeCombat = "IsInMeleeCombat"; // bool get
        public const string IsInParachuteFreeFall = "IsInParachuteFreeFall"; // bool get
        public const string IsInPlane = "IsInPlane"; // bool get
        public const string IsInPoliceVehicle = "IsInPoliceVehicle"; // bool get
        public const string IsInStealthMode = "IsInStealthMode"; // bool get
        public const string IsInSub = "IsInSub"; // bool get
        public const string IsInTaxi = "IsInTaxi"; // bool get
        public const string IsInTrain = "IsInTrain"; // bool get
        public const string IsJacking = "IsJacking"; // bool get
        public const string IsJumping = "IsJumping"; // bool get
        public const string IsJumpingOutOfVehicle = "IsJumpingOutOfVehicle"; // bool get
        public const string IsOnBike = "IsOnBike"; // bool get
        public const string IsOnFoot = "IsOnFoot"; // bool get
        public const string IsPainAudioEnabled = "IsPainAudioEnabled"; // bool set
        public const string IsPerformingStealthKill = "IsPerformingStealthKill"; // bool get
        public const string IsPlantingBomb = "IsPlantingBomb"; // bool get
        public const string IsPlayer = "IsPlayer"; // bool get
        public const string IsPriorityTargetForEnemies = "IsPriorityTargetForEnemies"; // bool set
        public const string IsProne = "IsProne"; // bool get
        public const string IsReloading = "IsReloading"; // bool get
        public const string IsRunning = "IsRunning"; // bool get
        public const string IsScriptedSpeechplaying = "IsScriptedSpeechplaying"; // bool get
        public const string IsShooting = "IsShooting"; // bool get
        public const string IsSprinting = "IsSprinting"; // bool get
        public const string IsStopped = "IsStopped"; // bool get
        public const string IsSwimming = "IsSwimming"; // bool get
        public const string IsSwimmingUnderWater = "IsSwimmingUnderWater"; // bool get
        public const string IsTryingToEnterALockedVehicle = "IsTryingToEnterALockedVehicle"; // bool get
        public const string IsVaulting = "IsVaulting"; // bool get
        public const string IsWalking = "IsWalking"; // bool get
        public const string IsWearingHelmet = "IsWearingHelmet"; // bool get
        public const string LastVehicle = "LastVehicle"; // Vehicle get
        public const string MaxDrivingSpeed = "MaxDrivingSpeed"; // float set
        public const string Money = "Money"; // int get set
        public const string MovementAnimationSet = "MovementAnimationSet"; // string set
        public const string NeverLeavesGroup = "NeverLeavesGroup"; // bool set
        public const string SeatIndex = "SeatIndex"; // VehicleSeat get
        public const string ShootRate = "ShootRate"; // int set
        public const string StaysInVehicleWhenJacked = "StaysInVehicleWhenJacked"; // bool set
        public const string Sweat = "Sweat"; // float get set
        public const string VehicleDrivingFlags = "VehicleDrivingFlags"; // VehicleDrivingFlags set
        public const string VehicleTryingToEnter = "VehicleTryingToEnter"; // Vehicle get
        public const string VehicleWeapon = "VehicleWeapon"; // VehicleWeaponHash get
        public const string Voice = "Voice"; // string set
        public const string WasKilledByStealth = "WasKilledByStealth"; // bool get
        public const string WasKilledByTakedown = "WasKilledByTakedown"; // bool get
        public const string WetnessHeight = "WetnessHeight"; // float set

        public const string IsInVehicle = "IsInVehicle";

        public const string CreateBlip = "CreateBlip";
        public const string RemoveBlips = "RemoveBlips";
        public const string SetIntoVehicle = "SetIntoVehicle";
        public const string MarkAsNoLongerNeeded = "MarkAsNoLongerNeeded";
        
        // Base entity class properties
        public const string IsDead = "IsDead";

        // TODO: not implemented for now
        // public ParachuteLandingType ParachuteLandingType => throw null;
        // public ParachuteState ParachuteState => throw null;
        // public PedGroup PedGroup => throw null;
        // public RelationshipGroup RelationshipGroup
        // public Style Style => throw null;
        // public int TaskSequenceProgress => throw null;
    }
}