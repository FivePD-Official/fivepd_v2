using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.DTO;

namespace FivePD.Client.Services;

internal class AlprService : IAlprService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IControlService _controlservice;
    private readonly INotificationService _notificationService;
    private readonly ITickService _tickService;
    private readonly IVehicleService _vehicleService;

    private bool enabled = false;
    private Guid taskGuid = new Guid("0");

    public AlprService(EventHandlerDictionary eventHandlerDictionary, IControlService controlService, INotificationService notificationService, ITickService tickService, IVehicleService vehicleService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._controlservice = controlService;
        this._notificationService = notificationService;
        this._tickService = tickService;
        this._vehicleService = vehicleService;
    }

    public void OnInit()
    {
        this._controlservice.RegisterKeybind(Controls.ToggleALPR.Id, Controls.ToggleALPR.Key, Controls.ToggleALPR.Title, () =>
        {
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

            if (veh.Position.DistanceToSquared(Game.PlayerPed.Position) > 50f)
            {
                continue;
            }

            nearbyVehicles.Add(veh);
        }

        return nearbyVehicles;
    }

    private void RunALPR()
    {
        if (!this.enabled)
        {
            return;
        }

        var task = this._tickService.EnqueueInterval(
            () =>
            {
                if (!this.enabled)
                {
                    this._tickService.DequeueInterval(this.taskGuid);
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
                catch (Exception)
                {
                    // Do nothing.
                }
            }, 5000);
        this.taskGuid = task;
    }
}