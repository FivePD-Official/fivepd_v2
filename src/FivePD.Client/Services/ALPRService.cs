using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Client.Models;
using FivePD.Common;
using FivePD.Common.DTO;

namespace FivePD.Client.Services;

/// <inheritdoc />
internal class AlprService : IAlprService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IControlService _controlservice;
    private readonly INotificationService _notificationService;
    private readonly ITickService _tickService;
    private readonly IVehicleService _vehicleService;

    private bool enabled = false;
    private Guid defaultGuid = Guid.NewGuid();
    private Guid taskGuid;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlprService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
    public AlprService(
        EventHandlerDictionary eventHandlerDictionary,
        IControlService controlService,
        INotificationService notificationService,
        ITickService tickService,
        IVehicleService vehicleService)
    {
        this.taskGuid = this.defaultGuid;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._controlservice = controlService;
        this._notificationService = notificationService;
        this._tickService = tickService;
        this._vehicleService = vehicleService;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._controlservice.RegisterKeybind(Controls.ToggleALPR.Id, Controls.ToggleALPR.Key, Controls.ToggleALPR.Title, () =>
        {
            if (!Game.PlayerPed.IsInPoliceVehicle)
            {
                return;
            }

            this.enabled = !this.enabled;
            string content = this.enabled ? "ALPR ~g~Enabled~s~" : "ALPR ~r~Disabled~s~";
            this._notificationService.ShowTooltip(content, true);
            if (this.enabled)
            {
                this.RunALPR();
            }
        });
    }

    private List<Vehicle> GetNearbyVehicles(float radius = 20f)
    {
        List<Vehicle> nearbyVehicles = new();
        foreach (var veh in World.GetAllVehicles())
        {
            if (veh == null)
            {
                continue;
            }

            if (!veh.Exists())
            {
                continue;
            }

            if (veh.Position.DistanceToSquared(Game.PlayerPed.Position) > radius)
            {
                continue;
            }

            nearbyVehicles.Add(veh);
        }

        return nearbyVehicles;
    }

    private void RunALPR()
    {
        var task = this._tickService.EnqueueInterval(
            () =>
            {
                if (!this.enabled)
                {
                    this._tickService.DequeueInterval(this.taskGuid);
                    this.taskGuid = this.defaultGuid;
                }

                try
                {
                    List<Vehicle> nearbyVehicles = this.GetNearbyVehicles();
                    nearbyVehicles.ForEach((vehicle) =>
                    {
                        this._vehicleService.GenerateVehicle(vehicle);
                        BaseScript.TriggerServerEvent(Events.Vehicle.FetchData, vehicle.NetworkId);

                        this._eventHandlerDictionary[Events.Vehicle.FetchData] += new Action<bool, string>((wanted, content) =>
                        {
                            if (wanted)
                            {
                                this._notificationService.ShowNotification(content);
                            }

                            this._eventHandlerDictionary.Remove(Events.Vehicle.FetchData);
                        });
                    });
                }
                catch (Exception ex)
                {
                    // Do nothing.
                    Debug.WriteLine(ex.ToString());
                }
            },
            5000);
        this.taskGuid = task;
    }
}