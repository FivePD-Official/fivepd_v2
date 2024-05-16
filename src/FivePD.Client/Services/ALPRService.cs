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
        this.taskGuid = Guid.Empty;
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
            else
            {
                this._tickService.DequeueInterval(this.taskGuid);
                this.taskGuid = Guid.Empty;
            }
        });

        this._eventHandlerDictionary[Events.ALPR.Notify] += new Action<bool, string>((wanted, content) =>
        {
            if (wanted)
            {
                this._notificationService.ShowNotification(content);
            }
        });
    }

    private void RunALPR()
    {
        var task = this._tickService.EnqueueInterval(
            () =>
            {
                List<Vehicle> nearbyVehicles = World.GetAllVehicles().Where(x => World.GetDistance(x.Position, Game.PlayerPed.Position) <= 20f).ToList();
                foreach (var nearbyVehicle in nearbyVehicles)
                {
                    this._vehicleService.GenerateVehicle(nearbyVehicle);
                    BaseScript.TriggerServerEvent(Events.ALPR.Check, nearbyVehicle.NetworkId);
                }
            },
            5000);
        this.taskGuid = task;
    }
}