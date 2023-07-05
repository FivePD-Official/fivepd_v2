using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Represents the tasks of a <see cref="FPed"/>.
/// </summary>
public sealed class FPedTasks
{
    private readonly FPed _fPed;
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FPedTasks"/> class.
    /// </summary>
    /// <param name="fPed">The <see cref="FPed"/> to use.</param>
    public FPedTasks(FPed fPed)
    {
        this._fPed = fPed;
        this._eventService = ServiceLocator.GetService<IEventService>();
    }

    /// <summary>
    /// Invokes the "ped.Task.AchieveHeading" native.
    /// </summary>
    /// <param name="heading">The heading.</param>
    /// <param name="timeout">The timeout.</param>
    public void AchieveHeading(float heading, int timeout = 0) =>
        this.SetTask(FPedTasksDTO.Events.AchieveHeading, new FPedTasksDTO.AchieveHeading { Heading = heading, Timeout = timeout });

    /// <summary>
    /// Invokes the "ped.Task.AimAt" native.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="duration">The duration.</param>
    public void AimAt(Vector3 target, int duration) =>
        this.SetTask(FPedTasksDTO.Events.AimAt, new FPedTasksDTO.AimAt { Target = target, Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.Arrest" native.
    /// </summary>
    /// <param name="ped">The ped.</param>
    public void Arrest(Ped ped) =>
        this.SetTask(FPedTasksDTO.Events.Arrest, new FPedTasksDTO.Arrest { NetworkId = ped.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.ChaseWithGroundVehicle" native.
    /// </summary>
    /// <param name="target">The target.</param>
    public void ChaseWithGroundVehicle(Entity target) =>
        this.SetTask(FPedTasksDTO.Events.ChaseWithGroundVehicle, new FPedTasksDTO.ChaseWithGroundVehicle { TargetNetworkId = target.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.ChaseWithHelicopter" native.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="offset">The offset.</param>
    public void ChaseWithHelicopter(Entity target, Vector3 offset) =>
        this.SetTask(FPedTasksDTO.Events.ChaseWithHelicopter, new FPedTasksDTO.ChaseWithHelicopter { TargetNetworkId = target.NetworkId, Offset = offset });

    /// <summary>
    /// Invokes the "ped.Task.ChaseWithPlane" native.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="offset">The offset.</param>
    public void ChaseWithPlane(Entity target, Vector3 offset) =>
        this.SetTask(FPedTasksDTO.Events.ChaseWithPlane, new FPedTasksDTO.ChaseWithPlane { TargetNetworkId = target.NetworkId, Offset = offset });

    /// <summary>
    /// Invokes the "ped.Task.ChatTo" native.
    /// </summary>
    /// <param name="ped">The ped.</param>
    public void ChatTo(Ped ped) =>
        this.SetTask(FPedTasksDTO.Events.ChatTo, new FPedTasksDTO.ChatTo { NetworkId = ped.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.ClearAll" native.
    /// </summary>
    public void ClearAll() =>
        this.SetTask(FPedTasksDTO.Events.ClearAll, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.ClearAllImmediately" native.
    /// </summary>
    public void ClearAllImmediately() =>
        this.SetTask(FPedTasksDTO.Events.ClearAllImmediately, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.ClearAnimation" native.
    /// </summary>
    /// <param name="animSet">The anim set.</param>
    /// <param name="animName">The anim name.</param>
    public void ClearAnimation(string animSet, string animName) =>
        this.SetTask(FPedTasksDTO.Events.ClearAnimation, new FPedTasksDTO.ClearAnimation { AnimSet = animSet, AnimName = animName });

    /// <summary>
    /// Invokes the "ped.Task.ClearLookAt" native.
    /// </summary>
    public void ClearLookAt() =>
        this.SetTask(FPedTasksDTO.Events.ClearLookAt, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.ClearSecondary" native.
    /// </summary>
    public void ClearSecondary() =>
        this.SetTask(FPedTasksDTO.Events.ClearSecondary, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.Climb" native.
    /// </summary>
    public void Climb() =>
        this.SetTask(FPedTasksDTO.Events.Climb, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.ClimbLadder" native.
    /// </summary>
    public void ClimbLadder() =>
        this.SetTask(FPedTasksDTO.Events.ClimbLadder, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.Cower" native.
    /// </summary>
    /// <param name="duration">The duration.</param>
    public void Cower(int duration) =>
        this.SetTask(FPedTasksDTO.Events.Cower, new FPedTasksDTO.Cower { Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.CruiseWithVehicle" native.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    /// <param name="speed">The speed.</param>
    /// <param name="drivingstyle">The drivingstyle.</param>
    public void CruiseWithVehicle(Vehicle vehicle, float speed, int drivingstyle = 0) =>
        this.SetTask(
            FPedTasksDTO.Events.CruiseWithVehicle,
            new FPedTasksDTO.CruiseWithVehicle { VehicleNetworkId = vehicle.NetworkId, Speed = speed, DrivingStyle = drivingstyle });

    /// <summary>
    /// Invokes the "ped.Task.StandStill" native.
    /// </summary>
    /// <param name="vehicle">The vehicle to drive.</param>
    /// <param name="target">The location to drive to.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="speed">The speed.</param>
    /// <param name="drivingStyle">The driving style flag.</param>
    public void DriveTo(Vehicle vehicle, Vector3 target, float radius, float speed, int drivingStyle = 0) =>
        this.SetTask(FPedTasksDTO.Events.DriveTo, new FPedTasksDTO.DriveTo
        {
            VehicleNetworkId = vehicle.NetworkId,
            Radius = radius,
            Speed = speed,
            Target = target,
            DrivingStyle = drivingStyle,
        });

    /// <summary>
    /// Invokes the "ped.Task.EnterAnyVehicle" native.
    /// </summary>
    /// <param name="seat">The seat.</param>
    /// <param name="timeout">The timeout.</param>
    /// <param name="speed">The speed.</param>
    /// <param name="flag">The flag.</param>
    public void EnterAnyVehicle(FVehicleSeat seat = FVehicleSeat.Any, int timeout = -1, float speed = 0.0f, int flag = 0) =>
        this.SetTask(FPedTasksDTO.Events.EnterAnyVehicle, new FPedTasksDTO.EnterAnyVehicle { Seat = seat, Flag = flag, Speed = speed, Timeout = timeout });

    /// <summary>
    /// Invokes the "ped.Task.EnterVehicle" native.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    /// <param name="seat">The seat.</param>
    /// <param name="timeout">The timeout.</param>
    /// <param name="speed">The speed.</param>
    /// <param name="flag">The flag.</param>
    public void EnterVehicle(Vehicle vehicle, FVehicleSeat seat = FVehicleSeat.Any, int timeout = -1, float speed = 0.0f, int flag = 0) =>
        this.SetTask(
            FPedTasksDTO.Events.EnterVehicle,
            new FPedTasksDTO.EnterVehicle { VehicleNetworkId = vehicle.NetworkId, Speed = speed, Seat = seat, Flag = flag, Timeout = timeout });

    /// <summary>
    /// Invokes the "ped.Task.FightAgainst" native.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="duration">The duration.</param>
    public void FightAgainst(Ped target, int duration) =>
        this.SetTask(FPedTasksDTO.Events.FightAgainst, new FPedTasksDTO.FightAgainst { PedNetworkId = target.NetworkId, Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.FightAgainstHatedTargets" native.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="duration">The duration.</param>
    public void FightAgainstHatedTargets(float radius, int duration) =>
        this.SetTask(FPedTasksDTO.Events.FightAgainstHatedTargets, new FPedTasksDTO.FightAgainstHatedTargets { Radius = radius, Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.FleeFrom" native.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="duration">The duration.</param>
    public void FleeFrom(Vector3 position, int duration = -1) =>
        this.SetTask(FPedTasksDTO.Events.FleeFrom, new FPedTasksDTO.FleeFrom { Position = position, Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.FollowPointRoute" native.
    /// </summary>
    /// <param name="points">The points.</param>
    public void FollowPointRoute(IEnumerable<Vector3> points) =>
        this.SetTask(FPedTasksDTO.Events.FollowPointRoute, new FPedTasksDTO.FollowPointRoute { Points = points.ToList() });

    /// <summary>
    /// Invokes the "ped.Task.FollowToOffsetFromEntity" native.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="movementSpeed">The movementSpeed.</param>
    /// <param name="timeout">The timeout.</param>
    /// <param name="stoppingRange">The stoppingRange.</param>
    /// <param name="persistFollowing">The persistFollowing.</param>
    public void FollowToOffsetFromEntity(Entity target, Vector3 offset, float movementSpeed, int timeout, float stoppingRange, bool persistFollowing) =>
        this.SetTask(
            FPedTasksDTO.Events.FollowToOffsetFromEntity,
            new FPedTasksDTO.FollowToOffsetFromEntity
            {
                TargetNetworkId = target.NetworkId,
                MovementSpeed = movementSpeed,
                Timeout = timeout,
                StoppingRange = stoppingRange,
                PersistFollowing = persistFollowing,
                Offset = offset,
            });

    /// <summary>
    /// Invokes the "ped.Task.GoTo" native.
    /// </summary>
    /// <param name="target">The target.</param>
    public void GoTo(Entity target) =>
        this.SetTask(FPedTasksDTO.Events.GoToTarget, new FPedTasksDTO.GoToTarget { TargetNetworkId = target.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.GoTo" native.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="timeout">The timeout.</param>
    public void GoTo(Entity target, Vector3 offset, int timeout = -1) =>
        this.SetTask(
            FPedTasksDTO.Events.GoToTargetWithOffset,
            new FPedTasksDTO.GoToTargetWithOffset { TargetNetworkId = target.NetworkId, Offset = offset, Timeout = timeout });

    /// <summary>
    /// Invokes the "ped.Task.GoTo" native.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="ignorePaths">The ignorePaths.</param>
    /// <param name="timeout">The timeout.</param>
    public void GoTo(Vector3 position, bool ignorePaths = false, int timeout = -1) =>
        this.SetTask(FPedTasksDTO.Events.GoToPosition, new FPedTasksDTO.GoToPosition { Position = position, Timeout = timeout, IgnorePaths = ignorePaths });

    /// <summary>
    /// Invokes the "ped.Task.GuardCurrentPosition" native.
    /// </summary>
    public void GuardCurrentPosition() => this.SetTask(FPedTasksDTO.Events.GuardCurrentPosition, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.HandsUp" native.
    /// </summary>
    /// <param name="duration">The duration.</param>
    public void HandsUp(int duration) => this.SetTask(FPedTasksDTO.Events.HandsUp, new FPedTasksDTO.HandsUp { Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.Jump" native.
    /// </summary>
    public void Jump() => this.SetTask(FPedTasksDTO.Events.Jump, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.LeaveVehicle" native.
    /// </summary>
    /// <param name="flags">The flags.</param>
    public void LeaveVehicle(FLeaveVehicleFlags flags = FLeaveVehicleFlags.None) =>
        this.SetTask(FPedTasksDTO.Events.LeaveVehicle, new FPedTasksDTO.LeaveVehicle { Flags = flags });

    /// <summary>
    /// Invokes the "ped.Task.LeaveVehicle" native.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    /// <param name="flags">The flags.</param>
    public void LeaveVehicle(Vehicle vehicle, FLeaveVehicleFlags flags) => this.SetTask(
        FPedTasksDTO.Events.LeaveVehicleSpecified,
        new FPedTasksDTO.LeaveVehicleSpecified { VehicleNetworkId = vehicle.NetworkId, Flags = flags });

    /// <summary>
    /// Invokes the "ped.Task.LeaveVehicle" native.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    /// <param name="closeDoor">The closeDoor.</param>
    public void LeaveVehicle(Vehicle vehicle, bool closeDoor) => this.SetTask(
        FPedTasksDTO.Events.LeaveVehicleCloseDoor,
        new FPedTasksDTO.LeaveVehicleCloseDoor { VehicleNetworkId = vehicle.NetworkId, CloseDoor = closeDoor });

    /// <summary>
    /// Invokes the "ped.Task.LookAt" native.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="duration">The duration.</param>
    public void LookAt(Entity target, int duration = 1) => this.SetTask(
        FPedTasksDTO.Events.LookAtTarget,
        new FPedTasksDTO.LookAtTarget { TargetNetworkId = target.NetworkId, Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.LookAt" native.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="duration">The duration.</param>
    public void LookAt(Vector3 position, int duration = 1) => this.SetTask(
        FPedTasksDTO.Events.LookAtPosition,
        new FPedTasksDTO.LookAtPosition { Position = position, Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.ParachuteTo" native.
    /// </summary>
    /// <param name="position">The position.</param>
    public void ParachuteTo(Vector3 position) => this.SetTask(FPedTasksDTO.Events.ParachuteTo, new FPedTasksDTO.ParachuteTo { Position = position });

    /// <summary>
    /// Invokes the "ped.Task.ParkVehicle" native.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    /// <param name="position">The position.</param>
    /// <param name="heading">The heading.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="keepEngineOn">The keepEngineOn.</param>
    public void ParkVehicle(Vehicle vehicle, Vector3 position, float heading, float radius = 20f, bool keepEngineOn = false) =>
        this.SetTask(
            FPedTasksDTO.Events.ParkVehicle,
            new FPedTasksDTO.ParkVehicle
            {
                VehicleNetworkId = vehicle.NetworkId,
                Position = position,
                Heading = heading,
                Radius = radius,
                KeepEngineOn = keepEngineOn,
            });

    /// <summary>
    /// Invokes the "ped.Task.PlayAnimation" native.
    /// </summary>
    /// <param name="animDict">The animDict.</param>
    /// <param name="animName">The animName.</param>
    public void PlayAnimation(string animDict, string animName) =>
        this.SetTask(FPedTasksDTO.Events.PlayAnimation, new FPedTasksDTO.PlayAnimation { AnimDict = animDict, AnimName = animName });

    /// <summary>
    /// Invokes the "ped.Task.PlayAnimation" native.
    /// </summary>
    /// <param name="animDict">The animDict.</param>
    /// <param name="animName">The animName.</param>
    /// <param name="blendInSpeed">The blendInSpeed.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="flags">The flags.</param>
    public void PlayAnimation(string animDict, string animName, float blendInSpeed, int duration, FAnimationFlags flags) =>
        this.SetTask(
            FPedTasksDTO.Events.PlayAnimationWithBlendInSpeed,
            new FPedTasksDTO.PlayAnimationWithBlendInSpeed
                { AnimDict = animDict, AnimName = animName, BlendInSpeed = blendInSpeed, Flags = flags, Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.PlayAnimation" native.
    /// </summary>
    /// <param name="animDict">The animDict.</param>
    /// <param name="animName">The animName.</param>
    /// <param name="speed">The speed.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="playbackRate">The playbackRate.</param>
    public void PlayAnimation(string animDict, string animName, float speed, int duration, float playbackRate) =>
        this.SetTask(
            FPedTasksDTO.Events.PlayAnimationWithPlaybackRate,
            new FPedTasksDTO.PlayAnimationWithPlaybackRate
                { AnimDict = animDict, AnimName = animName, Speed = speed, Duration = duration, PlaybackRate = playbackRate });

    /// <summary>
    /// Invokes the "ped.Task.PlayAnimation" native.
    /// </summary>
    /// <param name="animDict">The animDict.</param>
    /// <param name="animName">The animName.</param>
    /// <param name="blendInSpeed">The blendInSpeed.</param>
    /// <param name="blendOutSpeed">The blendOutSpeed.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="flags">The flags.</param>
    /// <param name="playbackRate">The playbackRate.</param>
    public void PlayAnimation(
        string animDict,
        string animName,
        float blendInSpeed,
        float blendOutSpeed,
        int duration,
        FAnimationFlags flags,
        float playbackRate) =>
        this.SetTask(
            FPedTasksDTO.Events.PlayAnimationWithBlendInAndOutSpeed,
            new FPedTasksDTO.PlayAnimationWithBlendInAndOutSpeed
            {
                AnimDict = animDict,
                AnimName = animName,
                BlendInSpeed = blendInSpeed,
                BlendOutSpeed = blendOutSpeed,
                Duration = duration,
                Flags = flags,
                PlaybackRate = playbackRate,
            });

    /// <summary>
    /// Invokes the "ped.Task.PutAwayMobilePhone" native.
    /// </summary>
    public void PutAwayMobilePhone() => this.SetTask(FPedTasksDTO.Events.PutAwayMobilePhone, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.PutAwayParachute" native.
    /// </summary>
    public void PutAwayParachute() => this.SetTask(FPedTasksDTO.Events.PutAwayParachute, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.ReactAndFlee" native.
    /// </summary>
    /// <param name="ped">The ped.</param>
    public void ReactAndFlee(Ped ped) => this.SetTask(FPedTasksDTO.Events.ReactAndFlee, new FPedTasksDTO.ReactAndFlee { PedNetworkId = ped.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.ReloadWeapon" native.
    /// </summary>
    public void ReloadWeapon() => this.SetTask(FPedTasksDTO.Events.ReloadWeapon, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.RunTo" native.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="ignorePaths">The ignorePaths.</param>
    /// <param name="timeout">The timeout.</param>
    public void RunTo(Vector3 position, bool ignorePaths = false, int timeout = -1) => this.SetTask(
        FPedTasksDTO.Events.RunTo,
        new FPedTasksDTO.RunTo { Position = position, Timeout = timeout, IgnorePaths = ignorePaths });

    /// <summary>
    /// Invokes the "ped.Task.ShootAt" native.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="pattern">The pattern.</param>
    public void ShootAt(Ped target, int duration = -1, FFiringPattern pattern = FFiringPattern.Default) => this.SetTask(
        FPedTasksDTO.Events.ShootAtTarget,
        new FPedTasksDTO.ShootAtTarget { TargetNetworkId = target.NetworkId, Duration = duration, FiringPattern = pattern });

    /// <summary>
    /// Invokes the "ped.Task.ShootAt" native.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="pattern">The pattern.</param>
    public void ShootAt(Vector3 position, int duration = -1, FFiringPattern pattern = FFiringPattern.Default) => this.SetTask(
        FPedTasksDTO.Events.ShootAtPosition,
        new FPedTasksDTO.ShootAtPosition { Position = position, Duration = duration, FiringPattern = pattern });

    /// <summary>
    /// Invokes the "ped.Task.ShuffleToNextVehicleSeat" native.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    public void ShuffleToNextVehicleSeat(Vehicle vehicle) => this.SetTask(
        FPedTasksDTO.Events.ShuffleToNextVehicleSeat,
        new FPedTasksDTO.ShuffleToNextVehicleSeat { VehicleNetworkId = vehicle.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.Skydive" native.
    /// </summary>
    public void Skydive() => this.SetTask(FPedTasksDTO.Events.Skydive, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.ShuffleToNextVehicleSeat" native.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="heading">The heading.</param>
    public void SlideTo(Vector3 position, float heading) => this.SetTask(
        FPedTasksDTO.Events.SlideTo,
        new FPedTasksDTO.SlideTo { Position = position, Heading = heading });

    /// <summary>
    /// Invokes the "ped.Task.StandStill" native.
    /// </summary>
    /// <param name="duration">The duration.</param>
    public void StandStill(int duration) => this.SetTask(FPedTasksDTO.Events.StandStill, new FPedTasksDTO.StandStill { Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.StartScenario" native.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="position">The position.</param>
    public void StartScenario(string name, Vector3 position) =>
        this.SetTask(FPedTasksDTO.Events.StartScenario, new FPedTasksDTO.StartScenario { Name = name, Position = position });

    /// <summary>
    /// Invokes the "ped.Task.SwapWeapon" native.
    /// </summary>
    public void SwapWeapon() => this.SetTask(FPedTasksDTO.Events.SwapWeapon, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.TurnTo" native.
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> to turn to.</param>
    /// <param name="duration">The duration.</param>
    public void TurnTo(Entity entity, int duration = -1) => this.SetTask(
        FPedTasksDTO.Events.TurnToEntity,
        new FPedTasksDTO.TurnToEntity { Duration = duration, EntityNetworkId = entity.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.TurnTo" native.
    /// </summary>
    /// <param name="position">The <see cref="Vector3"/> to turn to.</param>
    /// <param name="duration">The duration.</param>
    public void TurnTo(Vector3 position, int duration = -1) => this.SetTask(
        FPedTasksDTO.Events.TurnToPosition,
        new FPedTasksDTO.TurnToPosition { Duration = duration, Position = position });

    /// <summary>
    /// Invokes the "ped.Task.UseMobilePhone" native.
    /// </summary>
    public void UseMobilePhone() => this.SetTask(FPedTasksDTO.Events.UseMobilePhone, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.UseMobilePhone" native.
    /// </summary>
    /// <param name="duration">The duration.</param>
    public void UseMobilePhone(int duration) =>
        this.SetTask(FPedTasksDTO.Events.UseMobilePhoneWithDuration, new FPedTasksDTO.UseMobilePhoneWithDuration { Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.UseParachute" native.
    /// </summary>
    public void UseParachute() => this.SetTask(FPedTasksDTO.Events.UseParachute, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.VehicleChase" native.
    /// </summary>
    /// <param name="target">The target.</param>
    public void VehicleChase(Ped target) =>
        this.SetTask(FPedTasksDTO.Events.VehicleChase, new FPedTasksDTO.VehicleChase { TargetNetworkId = target.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.VehicleShootAtPed" native.
    /// </summary>
    /// <param name="target">The target.</param>
    public void VehicleShootAtPed(Ped target) =>
        this.SetTask(FPedTasksDTO.Events.VehicleShootAtPed, new FPedTasksDTO.VehicleShootAtPed { TargetNetworkId = target.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.Wait" native.
    /// </summary>
    /// <param name="duration">The duration.</param>
    public void Wait(int duration) =>
        this.SetTask(FPedTasksDTO.Events.Wait, new FPedTasksDTO.Wait { Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.WanderAround" native.
    /// </summary>
    public void WanderAround() => this.SetTask(FPedTasksDTO.Events.WanderAround, string.Empty);

    /// <summary>
    /// Invokes the "ped.Task.WanderAround" native.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="radius">The radius.</param>
    public void WanderAround(Vector3 position, float radius) =>
        this.SetTask(FPedTasksDTO.Events.WanderAroundPosition, new FPedTasksDTO.WanderAroundPosition { Position = position, Radius = radius });

    /// <summary>
    /// Invokes the "ped.Task.WarpIntoVehicle" native.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    /// <param name="seat">The seat.</param>
    public void WarpIntoVehicle(Vehicle vehicle, FVehicleSeat seat) =>
        this.SetTask(FPedTasksDTO.Events.WarpIntoVehicle, new FPedTasksDTO.WarpIntoVehicle { VehicleNetworkId = vehicle.NetworkId, Seat = seat });

    /// <summary>
    /// Invokes the "ped.Task.WarpOutOfVehicle" native.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    public void WarpOutOfVehicle(Vehicle vehicle) =>
        this.SetTask(FPedTasksDTO.Events.WarpOutOfVehicle, new FPedTasksDTO.WarpOutOfVehicle { VehicleNetworkId = vehicle.NetworkId });

    private void SetTask<T>(string task, T args)
    {
        this._eventService.TriggerEventWithoutCallback(
            this._fPed.Owner,
            Events.Ped.SetTask,
            JsonConvert.SerializeObject(new FPedTasksDTO.SetPedTaskRequest
            {
                NetworkId = this._fPed.GetFPed.NetworkId,
                Task = task,
                Args = JsonConvert.SerializeObject(args),
            }));
    }
}