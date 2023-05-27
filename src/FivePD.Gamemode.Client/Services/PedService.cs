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
        this._eventHandlerDictionary[Events.Ped.SetProperty] += new Action<string, int, string, string>(this.SetPedProperty);
        this._eventHandlerDictionary[Events.Ped.CreateBlip] += new Action<string>(this.CreateBlip);
        this._eventHandlerDictionary[Events.Ped.RemoveBlips] += new Action<string>(this.RemoveBlips);
        this._eventHandlerDictionary[Events.Ped.SetTask] += new Action<string>(this.SetTask);
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

    private void GetPedProperty(string eventId, int networkId, string property)
    {
        var ped = this.FindPed(networkId);
        if (ped is null)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, eventId, string.Empty);
            return;
        }

        object value = property switch
        {
            SupportedFPedProperties.IsDead => ped.IsDead,
            SupportedFPedProperties.IsOnFoot => ped.IsOnFoot,
            SupportedFPedProperties.IsInVehicle => ped.IsInVehicle(),
            _ => string.Empty,
        };

        BaseScript.TriggerServerEvent(Events.Generic.Callback, eventId, value.ToString());
    }

    private void SetPedProperty(string eventId, int networkId, string property, string value)
    {
        var ped = this.FindPed(networkId);
        if (ped is null)
        {
            return;
        }

        switch (property)
        {
            case SupportedFPedProperties.BlockPermanentEvents:
            {
                ped.BlockPermanentEvents = bool.Parse(value);
                break;
            }

            case SupportedFPedProperties.AlwaysKeepTask:
            {
                ped.AlwaysKeepTask = bool.Parse(value);
                break;
            }
        }
    }

    private void CreateBlip(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateBlipRequest>(serializedRequest);
        var ped = this.FindPed(request.NetworkId);
        ped?.CreateBlip(request.Sprite, request.Color, request.Text);
    }

    private void RemoveBlips(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<RemoveBlipsRequest>(serializedRequest);
        var ped = this.FindPed(request.NetworkId);
        ped?.RemoveBlips();
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
            case SupportedFPedTasks.StandStill:
            {
                var args = JsonConvert.DeserializeObject<PedTasks.StandStill>(request.Args);
                ped.Task.StandStill(args.Duration);
                break;
            }

            case SupportedFPedTasks.TurnToEntity:
            {
                var args = JsonConvert.DeserializeObject<PedTasks.TurnToEntity>(request.Args);
                ped.Task.TurnTo(Entity.FromNetworkId(args.EntityNetworkId), args.Duration);
                break;
            }

            case SupportedFPedTasks.TurnToPosition:
            {
                var args = JsonConvert.DeserializeObject<PedTasks.TurnToPosition>(request.Args);
                ped.Task.TurnTo(args.Position, args.Duration);
                break;
            }
        }
    }

    private Ped FindPed(int networkId)
    {
        return (Ped)Entity.FromNetworkId(networkId);
    }
}