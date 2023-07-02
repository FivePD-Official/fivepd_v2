using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;

namespace FivePD.Gamemode.Common.DTO;

#pragma warning disable

internal sealed record FPedTasksDTO
{
    public record struct SetPedTaskRequest
    {
        public int NetworkId { get; set; }

        public string Task { get; set; }

        public string Args { get; set; }
    }

    public record struct AchieveHeading
    {
        public required float Heading { get; init; }

        public required int Timeout { get; init; }
    }

    public record struct AimAt
    {
        public required Vector3 Target { get; init; }

        public required int Duration { get; init; }
    }

    public record struct Arrest
    {
        public required int NetworkId { get; init; }
    }

    public record struct ChaseWithGroundVehicle
    {
        public required int TargetNetworkId { get; init; }
    }

    public record struct ChaseWithPlane
    {
        public required int TargetNetworkId { get; init; }

        public required Vector3 Offset { get; init; }
    }

    public record struct ChaseWithHelicopter
    {
        public required int TargetNetworkId { get; init; }

        public required Vector3 Offset { get; init; }
    }

    public record struct ChatTo
    {
        public required int NetworkId { get; init; }
    }

    public record struct ClearAnimation
    {
        public required string AnimSet { get; init; }

        public required string AnimName { get; init; }
    }

    public record struct Cower
    {
        public required int Duration { get; init; }
    }

    public record struct CruiseWithVehicle
    {
        public required int VehicleNetworkId { get; init; }

        public required float Speed { get; init; }

        public required int DrivingStyle { get; init; }
    }
    
    public record struct DriveTo
    {
        public required int VehicleNetworkId { get; init; }

        public required Vector3 Target { get; init; }

        public required float Radius { get; init; }

        public required float Speed { get; init; }

        public required int DrivingStyle { get; init; }
    }

    public record struct EnterAnyVehicle
    {
        public required FVehicleSeat Seat { get; init; }

        public required int Timeout { get; init; }

        public required float Speed { get; init; }

        public required int Flag { get; init; }
    }

    public record struct EnterVehicle
    {
        public required int VehicleNetworkId { get; init; }

        public required FVehicleSeat Seat { get; init; }

        public required int Timeout { get; init; }

        public required float Speed { get; init; }

        public required int Flag { get; init; }
    }

    public record struct FightAgainst
    {
        public required int PedNetworkId { get; init; }

        public required int Duration { get; init; }
    }

    public record struct FightAgainstHatedTargets
    {
        public required float Radius { get; init; }

        public required int Duration { get; init; }
    }

    public record struct FleeFrom
    {
        public required Vector3 Position { get; init; }

        public required int Duration { get; init; }
    }

    public record struct FollowPointRoute
    {
        public required List<Vector3> Points { get; init; }
    }

    public record struct FollowToOffsetFromEntity
    {
        public required int TargetNetworkId { get; init; }

        public required Vector3 Offset { get; init; }

        public required float MovementSpeed { get; init; }

        public required int Timeout { get; init; }

        public required float StoppingRange { get; init; }

        public required bool PersistFollowing { get; init; }
    }

    public record struct GoToTarget
    {
        public required int TargetNetworkId { get; init; }
    }

    public record struct GoToTargetWithOffset
    {
        public required int TargetNetworkId { get; init; }

        public required Vector3 Offset { get; init; }

        public required int Timeout { get; init; }
    }

    public record struct GoToPosition
    {
        public required Vector3 Position { get; init; }

        public required bool IgnorePaths { get; init; }

        public required int Timeout { get; init; }
    }
    
    public record struct HandsUp
    {
        public required int Duration { get; init; }
    }
    
    public record struct LandPlane
    {
        public required Vector3 StartPosition { get; init; }

        public required Vector3 TouchdownPosition { get; init; }

        public required int PlaneNetworkId { get; init; }
    }
    
    public record struct LeaveVehicle
    {
        public required FLeaveVehicleFlags Flags { get; init; }
    }
    
    public record struct LeaveVehicleSpecified
    {
        public required int VehicleNetworkId { get; init; }

        public required FLeaveVehicleFlags Flags { get; init; }
    }
    
    public record struct LeaveVehicleCloseDoor
    {
        public required int VehicleNetworkId { get; init; }

        public required bool CloseDoor { get; init; }
    }
    
    public record struct LookAtTarget
    {
        public required int TargetNetworkId { get; init; }

        public required int Duration { get; init; }
    }
    
    public record struct LookAtPosition
    {
        public required Vector3 Position { get; init; }

        public required int Duration { get; init; }
    }
    
    public record struct ParachuteTo
    {
        public required Vector3 Position { get; init; }
    }
    
    public record struct ParkVehicle
    {
        public required int VehicleNetworkId { get; init; }
        
        public required Vector3 Position { get; init; }
        
        public required float Heading { get; init; }
        
        public required float Radius { get; init; }
        
        public required bool KeepEngineOn { get; init; }
    }
    
    public record struct PlayAnimation
    {
        public required string AnimDict { get; init; }

        public required string AnimName { get; init; }
    }
    
    public record struct PlayAnimationWithBlendInSpeed
    {
        public required string AnimDict { get; init; }

        public required string AnimName { get; init; }
        
        public required float BlendInSpeed { get; init; }
        
        public required int Duration { get; init; }
        
        public required FAnimationFlags Flags { get; init; }
    }
    
    public record struct PlayAnimationWithPlaybackRate
    {
        public required string AnimDict { get; init; }

        public required string AnimName { get; init; }
        
        public required float Speed { get; init; }
        
        public required int Duration { get; init; }
        
        public required float PlaybackRate { get; init; }
    }
    
    public record struct PlayAnimationWithBlendInAndOutSpeed
    {
        public required string AnimDict { get; init; }

        public required string AnimName { get; init; }
        
        public required float BlendInSpeed { get; init; }

        public required float BlendOutSpeed { get; init; }
        
        public required int Duration { get; init; }
        
        public required FAnimationFlags Flags { get; init; }

        public required float PlaybackRate { get; init; }
    }
    
    public record struct ReactAndFlee
    {
        public required int PedNetworkId { get; init; }
    }
    
    public record struct RunTo
    {
        public required Vector3 Position { get; init; }
        
        public required bool IgnorePaths { get; init; }
        
        public required int Timeout { get; init; }
    }
    
    public record struct ShootAtTarget
    {
        public required int TargetNetworkId { get; init; }
        
        public required int Duration { get; init; }

        public required FFiringPattern FiringPattern { get; init; }
    }
    
    public record struct ShootAtPosition
    {
        public required Vector3 Position { get; init; }
        
        public required int Duration { get; init; }

        public required FFiringPattern FiringPattern { get; init; }
    }
    
    public record struct ShuffleToNextVehicleSeat
    {
        public required int VehicleNetworkId { get; init; }
    }
    
    public record struct SlideTo
    {
        public required Vector3 Position { get; init; }

        public required float Heading { get; init; }
    }

    public record struct StandStill
    {
        public required int Duration { get; init; }
    }
    
    public record struct StartScenario
    {
        public required string Name { get; init; }

        public required Vector3 Position { get; init; }
    }

    public record struct TurnToEntity
    {
        public required int EntityNetworkId { get; init; }

        public required int Duration { get; init; }
    }

    public record struct TurnToPosition
    {
        public required Vector3 Position { get; init; }

        public required int Duration { get; init; }
    }
    
    public record struct UseMobilePhoneWithDuration
    {
        public required int Duration { get; init; }
    }
    
    public record struct VehicleChase
    {
        public required int TargetNetworkId { get; init; }
    }
    
    public record struct VehicleShootAtPed
    {
        public required int TargetNetworkId { get; init; }
    }
    
    public record struct Wait
    {
        public required int Duration { get; init; }
    }
    
    public record struct WanderAroundPosition
    {
        public required Vector3 Position { get; init; }

        public required float Radius { get; init; }
    }
    
    public record struct WarpIntoVehicle
    {
        public required int VehicleNetworkId { get; init; }

        public required FVehicleSeat Seat { get; init; }
    }
    
    public record struct WarpOutOfVehicle
    {
        public required int VehicleNetworkId { get; init; }
    }

    public sealed record Events
    {
        public const string AchieveHeading = "AchieveHeading";

        public const string AimAt = "AimAt";

        public const string Arrest = "Arrest";

        public const string ChaseWithGroundVehicle = "ChaseWithGroundVehicle";

        public const string ChaseWithHelicopter = "ChaseWithHelicopter";

        public const string ChaseWithPlane = "ChaseWithPlane";

        public const string ChatTo = "ChatTo";

        public const string ClearAll = "ClearAll";

        public const string ClearAllImmediately = "ClearAllImmediately";

        public const string ClearAnimation = "ClearAnimation";

        public const string ClearLookAt = "ClearLookAt";

        public const string ClearSecondary = "ClearSecondary";

        public const string Climb = "Climb";

        public const string ClimbLadder = "ClimbLadder";

        public const string Cower = "Cower";

        public const string CruiseWithVehicle = "CruiseWithVehicle";

        public const string DriveTo = "DriveTo";

        public const string EnterAnyVehicle = "EnterAnyVehicle";

        public const string EnterVehicle = "EnterVehicle";

        public const string FightAgainst = "FightAgainst";

        public const string FightAgainstHatedTargets = "FightAgainstHatedTargets";

        public const string FleeFrom = "FleeFrom";

        public const string FollowPointRoute = "FollowPointRoute";

        public const string FollowToOffsetFromEntity = "FollowToOffsetFromEntity";

        public const string GoToTarget = "GoToTarget";

        public const string GoToTargetWithOffset = "GoToTargetWithOffset";

        public const string GoToPosition = "GoToPosition";

        public const string GuardCurrentPosition = "GuardCurrentPosition";

        public const string HandsUp = "HandsUp";

        public const string Jump = "Jump";

        public const string LandPlane = "LandPlane";

        public const string LeaveVehicle = "LeaveVehicle";
        
        public const string LeaveVehicleSpecified = "LeaveVehicleSpecified";
        
        public const string LeaveVehicleCloseDoor = "LeaveVehicleCloseDoor";

        public const string LookAtTarget = "LookAtTarget";

        public const string LookAtPosition = "LookAtPosition";

        public const string ParachuteTo = "ParachuteTo";

        public const string ParkVehicle = "ParkVehicle";

        public const string PlayAnimation = "PlayAnimation";
        
        public const string PlayAnimationWithBlendInSpeed = "PlayAnimationWithBlendInSpeed";

        public const string PlayAnimationWithPlaybackRate = "PlayAnimationWithPlaybackRate";

        public const string PlayAnimationWithBlendInAndOutSpeed = "PlayAnimationWithBlendInAndOutSpeed";

        public const string PutAwayMobilePhone = "PutAwayMobilePhone";

        public const string PutAwayParachute = "PutAwayParachute";

        public const string ReactAndFlee = "ReactAndFlee";

        public const string ReloadWeapon = "ReloadWeapon";

        public const string RunTo = "RunTo";

        public const string ShootAtTarget = "ShootAtTarget";

        public const string ShootAtPosition = "ShootAtPosition";

        public const string ShuffleToNextVehicleSeat = "ShuffleToNextVehicleSeat";

        public const string Skydive = "Skydive";

        public const string SlideTo = "SlideTo";

        public const string StandStill = "StandStill";

        public const string StartScenario = "StartScenario";

        public const string SwapWeapon = "SwapWeapon";

        public const string TurnToEntity = "TurnToEntity";

        public const string TurnToPosition = "TurnToPosition";

        public const string UseMobilePhone = "UseMobilePhone";

        public const string UseMobilePhoneWithDuration = "UseMobilePhoneWithDuration";

        public const string UseParachute = "UseParachute";

        public const string VehicleChase = "VehicleChase";

        public const string VehicleShootAtPed = "VehicleShootAtPed";

        public const string Wait = "Wait";

        public const string WanderAround = "WanderAround";

        public const string WanderAroundPosition = "WanderAroundPosition";

        public const string WarpIntoVehicle = "WarpIntoVehicle";

        public const string WarpOutOfVehicle = "WarpOutOfVehicle";
    }
}