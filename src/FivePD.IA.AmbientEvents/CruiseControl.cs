using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.IA.AmbientEvents;

#pragma warning disable

// TODO: create a public repo and a new solution to contain an example addon (actually remove the FivePD.IA.AmbientEvents completely from here)
public class CruiseControl : IExtension
{
    private readonly IControlService _controlService;
    private readonly ITickQueueService _tickQueueService;

    private readonly Dictionary<FPlayer, Options> _players = new();
    private Guid _intervalId;

    public CruiseControl(IControlService controlService, ITickQueueService tickQueueService)
    {
        this._controlService = controlService;
        this._tickQueueService = tickQueueService;
    }

    public Task OnStartedAsync()
    {
        this._controlService.RegisterKeybind("fivepd::cruiseControl::toggle", "K", "Toggle cruise control", player => this.OnKeyPress(player));
        this._intervalId = this._tickQueueService.EnqueueInterval(this.OnTick, 500);

        return Task.CompletedTask;
    }

    public Task OnStoppedAsync()
    {
        this._tickQueueService.DequeueInterval(this._intervalId);

        return Task.CompletedTask;
    }

    private async Task OnTick()
    {
        foreach (var vehicleWithSpeed in this._players.Values)
        {
            if (vehicleWithSpeed.Vehicle.GetUnderlyingVehicle is null)
            {
                continue;
            }

            vehicleWithSpeed.Vehicle.Properties.SetSpeed(vehicleWithSpeed.Speed);
        }
    }

    private async Task OnKeyPress(FPlayer player)
    {
        var vehicleHandle = Cfx.API.GetVehiclePedIsIn(player.GetUnderlyingPlayer().Character.Handle, false);
        if (vehicleHandle == 0)
        {
            return;
        }

        var fVehicle = FVehicle.FromVehicle(new Vehicle(vehicleHandle));
        if (fVehicle is null)
        {
            if (this._players.ContainsKey(player))
            {
                this._players.Remove(player);
            }

            return;
        }

        var speed = await fVehicle.Properties.Speed;
        if (this._players.ContainsKey(player))
        {
            this._players.Remove(player);
        }
        else
        {
            this._players[player] = new Options(fVehicle, speed);
        }
    }

    private sealed record Options(FVehicle Vehicle, float Speed)
    {
        public FVehicle Vehicle { get; } = Vehicle;

        public float Speed { get; } = Speed;
    }
}