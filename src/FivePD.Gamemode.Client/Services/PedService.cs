using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Builders;
using FivePD.Gamemode.Common.DTO;
using FivePD.Gamemode.Common.Enums;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services;

#pragma warning disable // TODO remove

/// <inheritdoc cref="FivePD.Gamemode.Client.Interfaces.IPedService" />
internal class PedService : IPedService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="PedService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public PedService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Ped.Search);
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
    public void GeneratePed(Ped ped)
    {
        var fPedBuilder = new CommonFPedBuilder(ped.NetworkId)
            .WithGender((FGender)ped.Gender);
        BaseScript.TriggerServerEvent(Events.Ped.Generate, JsonConvert.SerializeObject(fPedBuilder));
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
            FPedPropertiesDTO.Events.IsDead => ped.IsDead,
            FPedPropertiesDTO.Events.IsOnFoot => ped.IsOnFoot,
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
            case FPedPropertiesDTO.Events.BlockPermanentEvents:
            {
                ped.BlockPermanentEvents = bool.Parse(value);
                break;
            }

            case FPedPropertiesDTO.Events.AlwaysKeepTask:
            {
                ped.AlwaysKeepTask = bool.Parse(value);
                break;
            }

            case FPedPropertiesDTO.Events.CreateBlip:
            {
                var request = JsonConvert.DeserializeObject<FPedPropertiesDTO.CreateBlipRequest>(value);
                ped.CreateBlip(request.Sprite, request.Color, request.Text);
                break;
            }

            case FPedPropertiesDTO.Events.RemoveBlips:
            {
                ped.RemoveBlips();
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
                ped.Task.AchieveHeading(args.Heading, args.Timeout);
                break;
            }

            case FPedTasksDTO.Events.AimAt:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.AimAt>(request.Args);
                ped.Task.AimAt(args.Target, args.Duration);
                break;
            }

            case FPedTasksDTO.Events.Arrest:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.Arrest>(request.Args);
                ped.Task.Arrest(Entity.FromNetworkId(args.NetworkId) as Ped);
                break;
            }

            case FPedTasksDTO.Events.ChaseWithGroundVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ChaseWithGroundVehicle>(request.Args);
                ped.Task.ChaseWithGroundVehicle(Entity.FromNetworkId(args.TargetNetworkId));
                break;
            }
            
            case FPedTasksDTO.Events.ChaseWithHelicopter:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ChaseWithHelicopter>(request.Args);
                ped.Task.ChaseWithHelicopter(Entity.FromNetworkId(args.TargetNetworkId), args.Offset);
                break;
            }
            
            case FPedTasksDTO.Events.ChaseWithPlane:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ChaseWithPlane>(request.Args);
                ped.Task.ChaseWithPlane(Entity.FromNetworkId(args.TargetNetworkId), args.Offset);
                break;
            }
            
            case FPedTasksDTO.Events.ChatTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ChatTo>(request.Args);
                ped.Task.ChatTo(Entity.FromNetworkId(args.NetworkId) as Ped);
                break;
            }
            
            case FPedTasksDTO.Events.ClearAll:
            {
                ped.Task.ClearAll();
                break;
            }
            
            case FPedTasksDTO.Events.ClearAllImmediately:
            {
                ped.Task.ClearAllImmediately();
                break;
            }
            
            case FPedTasksDTO.Events.ClearAnimation:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ClearAnimation>(request.Args);
                ped.Task.ClearAnimation(args.AnimSet, args.AnimName);
                break;
            }
            
            case FPedTasksDTO.Events.ClearLookAt:
            {
                ped.Task.ClearLookAt();
                break;
            }
            
            case FPedTasksDTO.Events.ClearSecondary:
            {
                ped.Task.ClearSecondary();
                break;
            }
            
            case FPedTasksDTO.Events.Climb:
            {
                ped.Task.Climb();
                break;
            }
            
            case FPedTasksDTO.Events.ClimbLadder:
            {
                ped.Task.ClimbLadder();
                break;
            }
            
            case FPedTasksDTO.Events.Cower:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.Cower>(request.Args);
                ped.Task.Cower(args.Duration);
                break;
            }

            case FPedTasksDTO.Events.CruiseWithVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.CruiseWithVehicle>(request.Args);
                ped.Task.CruiseWithVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, args.Speed, args.DrivingStyle);
                break;
            }
            
            case FPedTasksDTO.Events.DriveTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.DriveTo>(request.Args);
                ped.Task.DriveTo(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, args.Target, args.Radius, args.Speed, args.DrivingStyle);
                break;
            }
            
            case FPedTasksDTO.Events.EnterAnyVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.EnterAnyVehicle>(request.Args);
                ped.Task.EnterAnyVehicle((VehicleSeat)args.Seat, args.Timeout, args.Speed, args.Flag);
                break;
            }
            
            case FPedTasksDTO.Events.EnterVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.EnterVehicle>(request.Args);
                ped.Task.EnterVehicle(Entity.FromHandle(args.VehicleNetworkId) as Vehicle, (VehicleSeat)args.Seat, args.Timeout, args.Speed, args.Flag);
                break;
            }
            
            case FPedTasksDTO.Events.FightAgainst:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FightAgainst>(request.Args);
                ped.Task.FightAgainst(Entity.FromHandle(args.PedNetworkId) as Ped, args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.FightAgainstHatedTargets:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FightAgainstHatedTargets>(request.Args);
                ped.Task.FightAgainstHatedTargets(args.Radius, args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.FleeFrom:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FleeFrom>(request.Args);
                ped.Task.FleeFrom(args.Position, args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.FollowPointRoute:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FollowPointRoute>(request.Args);
                ped.Task.FollowPointRoute(args.Points.ToArray());
                break;
            }
            
            case FPedTasksDTO.Events.FollowToOffsetFromEntity:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.FollowToOffsetFromEntity>(request.Args);
                ped.Task.FollowToOffsetFromEntity(Entity.FromNetworkId(args.TargetNetworkId), args.Offset, args.MovementSpeed, args.Timeout, args.StoppingRange, args.PersistFollowing);
                break;
            }
            
            case FPedTasksDTO.Events.GoToTarget:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.GoToTarget>(request.Args);
                ped.Task.GoTo(Entity.FromNetworkId(args.TargetNetworkId));
                break;
            }
            
            case FPedTasksDTO.Events.GoToTargetWithOffset:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.GoToTargetWithOffset>(request.Args);
                ped.Task.GoTo(Entity.FromNetworkId(args.TargetNetworkId), args.Offset, args.Timeout);
                break;
            }
            
            case FPedTasksDTO.Events.GoToPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.GoToPosition>(request.Args);
                ped.Task.GoTo(args.Position, args.IgnorePaths, args.Timeout);
                break;
            }
            
            case FPedTasksDTO.Events.GuardCurrentPosition:
            {
                ped.Task.GuardCurrentPosition();
                break;
            }
            
            case FPedTasksDTO.Events.HandsUp:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.HandsUp>(request.Args);
                ped.Task.HandsUp(args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.Jump:
            {
                ped.Task.Jump();
                break;
            }
            
            case FPedTasksDTO.Events.LandPlane:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LandPlane>(request.Args);
                ped.Task.LandPlane(args.StartPosition, args.TouchdownPosition, Entity.FromNetworkId(args.PlaneNetworkId) as Vehicle);
                break;
            }
            
            case FPedTasksDTO.Events.LeaveVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LeaveVehicle>(request.Args);
                ped.Task.LeaveVehicle((LeaveVehicleFlags)args.Flags);
                break;
            }
            
            case FPedTasksDTO.Events.LeaveVehicleSpecified:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LeaveVehicleSpecified>(request.Args);
                ped.Task.LeaveVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, (LeaveVehicleFlags)args.Flags);
                break;
            }
            
            case FPedTasksDTO.Events.LeaveVehicleCloseDoor:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LeaveVehicleCloseDoor>(request.Args);
                ped.Task.LeaveVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, args.CloseDoor);
                break;
            }
            
            case FPedTasksDTO.Events.LookAtTarget:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LookAtTarget>(request.Args);
                ped.Task.LookAt(Entity.FromNetworkId(args.TargetNetworkId), args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.LookAtPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.LookAtPosition>(request.Args);
                ped.Task.LookAt(args.Position, args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.ParachuteTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ParachuteTo>(request.Args);
                ped.Task.ParachuteTo(args.Position);
                break;
            }
            
            case FPedTasksDTO.Events.ParkVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ParkVehicle>(request.Args);
                ped.Task.ParkVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, args.Position, args.Heading, args.Radius, args.KeepEngineOn);
                break;
            }
            
            case FPedTasksDTO.Events.PlayAnimation:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.PlayAnimation>(request.Args);
                ped.Task.PlayAnimation(args.AnimDict, args.AnimName);
                break;
            }
            
            case FPedTasksDTO.Events.PlayAnimationWithBlendInSpeed:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.PlayAnimationWithBlendInSpeed>(request.Args);
                ped.Task.PlayAnimation(args.AnimDict, args.AnimName, args.BlendInSpeed, args.Duration, (AnimationFlags)args.Flags);
                break;
            }
            
            case FPedTasksDTO.Events.PlayAnimationWithPlaybackRate:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.PlayAnimationWithPlaybackRate>(request.Args);
                ped.Task.PlayAnimation(args.AnimDict, args.AnimName, args.Speed, args.Duration, args.PlaybackRate);
                break;
            }
            
            case FPedTasksDTO.Events.PlayAnimationWithBlendInAndOutSpeed:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.PlayAnimationWithBlendInAndOutSpeed>(request.Args);
                ped.Task.PlayAnimation(args.AnimDict, args.AnimName, args.BlendInSpeed, args.BlendOutSpeed, args.Duration, (AnimationFlags)args.Flags, args.PlaybackRate);
                break;
            }
            
            case FPedTasksDTO.Events.PutAwayMobilePhone:
            {
                ped.Task.PutAwayMobilePhone();
                break;
            }
            
            case FPedTasksDTO.Events.PutAwayParachute:
            {
                ped.Task.PutAwayParachute();
                break;
            }
            
            case FPedTasksDTO.Events.ReactAndFlee:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ReactAndFlee>(request.Args);
                ped.Task.ReactAndFlee(Entity.FromNetworkId(args.PedNetworkId) as Ped);
                break;
            }
            
            case FPedTasksDTO.Events.ReloadWeapon:
            {
                ped.Task.ReloadWeapon();
                break;
            }
            
            case FPedTasksDTO.Events.RunTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.RunTo>(request.Args);
                ped.Task.RunTo(args.Position, args.IgnorePaths, args.Timeout);
                break;
            }
            
            case FPedTasksDTO.Events.ShootAtTarget:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ShootAtTarget>(request.Args);
                ped.Task.ShootAt(Entity.FromNetworkId(args.TargetNetworkId) as Ped, args.Duration, (FiringPattern)args.FiringPattern);
                break;
            }
            
            case FPedTasksDTO.Events.ShootAtPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ShootAtPosition>(request.Args);
                ped.Task.ShootAt(args.Position, args.Duration, (FiringPattern)args.FiringPattern);
                break;
            }
            
            case FPedTasksDTO.Events.ShuffleToNextVehicleSeat:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.ShuffleToNextVehicleSeat>(request.Args);
                ped.Task.ShuffleToNextVehicleSeat(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle);
                break;
            }
            
            case FPedTasksDTO.Events.Skydive:
            {
                ped.Task.Skydive();
                break;
            }
            
            case FPedTasksDTO.Events.SlideTo:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.SlideTo>(request.Args);
                ped.Task.SlideTo(args.Position, args.Heading);
                break;
            }
            
            case FPedTasksDTO.Events.StandStill:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.StandStill>(request.Args);
                ped.Task.StandStill(args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.StartScenario:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.StartScenario>(request.Args);
                ped.Task.StartScenario(args.Name, args.Position);
                break;
            }
            
            case FPedTasksDTO.Events.SwapWeapon:
            {
                ped.Task.SwapWeapon();
                break;
            }

            case FPedTasksDTO.Events.TurnToEntity:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.TurnToEntity>(request.Args);
                ped.Task.TurnTo(Entity.FromNetworkId(args.EntityNetworkId), args.Duration);
                break;
            }

            case FPedTasksDTO.Events.TurnToPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.TurnToPosition>(request.Args);
                ped.Task.TurnTo(args.Position, args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.UseMobilePhone:
            {
                ped.Task.UseMobilePhone();
                break;
            }
            
            case FPedTasksDTO.Events.UseMobilePhoneWithDuration:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.UseMobilePhoneWithDuration>(request.Args);
                ped.Task.UseMobilePhone(args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.UseParachute:
            {
                ped.Task.UseParachute();
                break;
            }
            
            case FPedTasksDTO.Events.VehicleChase:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.VehicleChase>(request.Args);
                ped.Task.VehicleChase(Entity.FromNetworkId(args.TargetNetworkId) as Ped);
                break;
            }
            
            case FPedTasksDTO.Events.VehicleShootAtPed:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.VehicleShootAtPed>(request.Args);
                ped.Task.VehicleShootAtPed(Entity.FromNetworkId(args.TargetNetworkId) as Ped);
                break;
            }
            
            case FPedTasksDTO.Events.Wait:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.Wait>(request.Args);
                ped.Task.Wait(args.Duration);
                break;
            }
            
            case FPedTasksDTO.Events.WanderAround:
            {
                ped.Task.WanderAround();
                break;
            }
            
            case FPedTasksDTO.Events.WanderAroundPosition:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.WanderAroundPosition>(request.Args);
                ped.Task.WanderAround(args.Position, args.Radius);
                break;
            }
            
            case FPedTasksDTO.Events.WarpIntoVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.WarpIntoVehicle>(request.Args);
                ped.Task.WarpIntoVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle, (VehicleSeat)args.Seat);
                break;
            }
            
            case FPedTasksDTO.Events.WarpOutOfVehicle:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.WarpOutOfVehicle>(request.Args);
                ped.Task.WarpOutOfVehicle(Entity.FromNetworkId(args.VehicleNetworkId) as Vehicle);
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
                ped.Weapons.Give((WeaponHash)args.Hash, args.AmmoCount, args.EquipNow, args.IsAmmoLoaded);
                break;
            }

            case WeaponCollectionDTO.Events.Remove:
            {
                var args = JsonConvert.DeserializeObject<WeaponCollectionDTO.Remove>(request.Args);
                ped.Weapons.Remove((WeaponHash)args.Hash);
                break;
            }

            case WeaponCollectionDTO.Events.RemoveAll:
            {
                ped.Weapons.RemoveAll();
                break;
            }

            case WeaponCollectionDTO.Events.Select:
            {
                var args = JsonConvert.DeserializeObject<WeaponCollectionDTO.Select>(request.Args);
                ped.Weapons.Select((WeaponHash)args.Hash);
                break;
            }

            case WeaponCollectionDTO.Events.Drop:
            {
                ped.Weapons.Drop();
                break;
            }

            case WeaponCollectionDTO.Events.AddComponentToWeapon:
            {
                var args = JsonConvert.DeserializeObject<WeaponCollectionDTO.AddComponentToWeapon>(request.Args);
                if (Cfx.API.DoesWeaponTakeWeaponComponent(args.WeaponHash, args.ComponentHash))
                {
                    Cfx.API.GiveWeaponComponentToPed(Game.PlayerPed.Handle, args.WeaponHash, args.ComponentHash);
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
        while (!Cfx.API.NetworkDoesEntityExistWithNetworkId(networkId))
        {
            await BaseScript.Delay(10);
        }

        return Entity.FromNetworkId(networkId) as Ped;
    }
}