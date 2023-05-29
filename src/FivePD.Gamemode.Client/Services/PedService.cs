using System;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Models;
using FivePD.Common.Requests;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Requests;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc />
public class PedService : IPedService
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
        this._eventHandlerDictionary[Events.Ped.GetProperty] += new Action<string, int, string>(this.GetPedProperty);
        this._eventHandlerDictionary[Events.Ped.SetProperty] += new Action<int, string, string>(this.SetPedProperty);
        this._eventHandlerDictionary[Events.Ped.SetTask] += new Action<string>(this.SetTask);
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

    private void GetPedProperty(string eventCallbackId, int networkId, string property)
    {
        var ped = this.FindPed(networkId);
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

    private void SetPedProperty(int networkId, string property, string value)
    {
        var ped = this.FindPed(networkId);
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

    private void SetTask(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<SetPedTaskRequest>(serializedRequest);
        var ped = this.FindPed(request.NetworkId);
        if (ped is null)
        {
            return;
        }

        switch (request.Task)
        {
            case FPedTasksDTO.Events.StandStill:
            {
                var args = JsonConvert.DeserializeObject<FPedTasksDTO.StandStill>(request.Args);
                ped.Task.StandStill(args.Duration);
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
        }
    }

    private void WeaponCollection(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<WeaponCollectionRequest>(serializedRequest);
        var ped = this.FindPed(request.NetworkId);
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

    private void WeaponCollectionWithReturn(string eventCallbackId, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<WeaponCollectionRequest>(serializedRequest);
        var ped = this.FindPed(request.NetworkId);
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

    private Ped FindPed(int networkId)
    {
        return (Ped)Entity.FromNetworkId(networkId);
    }
}