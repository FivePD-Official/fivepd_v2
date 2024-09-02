using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

internal class WaypointService : IWaypointService, IOnInit, IOnDestroy
{
    private readonly ITickService _tickService;

    public WaypointService(ITickService tickService)
    {
        this._tickService = tickService;
    }

    public void OnInit()
    {
        this._tickService.OnTick += this.OnTick;
    }

    public void OnDestroy()
    {
        this._tickService.OnTick -= this.OnTick;
    }

    private Vehicle _lastVehicle;
    private bool _sentWaypoint;

    private Task OnTick()
    {
        if (this._sentWaypoint)
        {
            return Task.FromResult(0);
        }

        var vehicle = Game.PlayerPed.CurrentVehicle;
        if (vehicle is null)
        {
            return Task.FromResult(0);
        }

        if (this._lastVehicle == vehicle)
        {
            return Task.FromResult(0);
        }

        this._lastVehicle = vehicle;

        var state = vehicle.State[StateBagKeys.Vehicle.Miscellaneous.RideShare];
        if (state is null)
        {
            return Task.FromResult(0);
        }

        if (state is not string eventId)
        {
            return Task.FromResult(0);
        }

        var eventName = $"rideshare:waypoint:selected::{eventId}";
        var blipInfoId = Cfx.API.GetFirstBlipInfoId(8);
        if (blipInfoId == 0)
        {
            return Task.FromResult(0);
        }

        var blipCoords = Cfx.API.GetBlipCoords(blipInfoId);
        BaseScript.TriggerServerEvent(eventName, JsonConvert.SerializeObject(blipCoords));

        this._sentWaypoint = true;

        return Task.FromResult(0);
    }
}