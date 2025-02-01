using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.ConfigModels;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using Newtonsoft.Json;

namespace FivePD.Client.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Client.Interfaces.IAlprService" />
internal class AlprService : IAlprService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IControlService _controlService;
    private readonly INotificationService _notificationService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly ITickService _tickService;
    private readonly IVehicleService _vehicleService;
    private readonly IConfigService _configService;

    private Guid _taskGuid = Guid.Empty;

    private GeneralConfig _generalConfig = new()
    {
        Audio = new()
        {
            Files = new(),
        }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AlprService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    /// <param name="gameNotificationService">The <see cref="IGameNotificationService"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    public AlprService(
        EventHandlerDictionary eventHandlerDictionary,
        IControlService controlService,
        INotificationService notificationService,
        IGameNotificationService gameNotificationService,
        ITickService tickService,
        IVehicleService vehicleService,
        IConfigService configService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._controlService = controlService;
        this._notificationService = notificationService;
        this._gameNotificationService = gameNotificationService;
        this._tickService = tickService;
        this._vehicleService = vehicleService;
        this._configService = configService;

#pragma warning disable CS4014
        this.GetGeneralConfigAsync();
#pragma warning restore CS4014
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._controlService.RegisterKeybind(Controls.ToggleAlpr.Id, Controls.ToggleAlpr.Key, Controls.ToggleAlpr.Title, this.OnToggle);

        this._eventHandlerDictionary[Events.ALPR.Notify] += new Action<string>(seralized =>
        {
            var obj = JsonConvert.DeserializeObject<CheckNotificationDTO>(seralized);
            if (obj.WantedStatus)
            {
                this._notificationService.ShowNotification(obj.NotificationContent);
            }
        });
    }

#pragma warning disable S3241
    private async Task GetGeneralConfigAsync()
#pragma warning restore S3241
    {
        this._generalConfig = await this._configService.GeneralConfig;
    }

    private void OnToggle()
    {
        if (!Game.PlayerPed.IsInVehicle())
        {
            return;
        }

        if (this._taskGuid == Guid.Empty)
        {
            this._taskGuid = this._tickService.EnqueueInterval(
                () =>
                {
                    var nearbyVehicles = World.GetAllVehicles()
                        .Where(x => World.GetDistance(x.Position, Game.PlayerPed.Position) <= 20f)
                        .ToList();
                    foreach (var nearbyVehicle in nearbyVehicles)
                    {
                        this._vehicleService.GenerateVehicle(nearbyVehicle);
                        // BaseScript.TriggerServerEvent(Events.ALPR.Check, nearbyVehicle.NetworkId);
                        this.Asd(nearbyVehicle);
                    }
                },
                5000);
            this._gameNotificationService.Notify(new GameNotificationOptions
            {
                Title = "FivePD",
                Message = "ALPR has been enabled.",
                PlayAudio = this._generalConfig.Audio.Files.AlprToggle,
            });
        }
        else
        {
            this._tickService.DequeueInterval(this._taskGuid);
            this._taskGuid = Guid.Empty;
            this._gameNotificationService.Notify(new GameNotificationOptions
            {
                Title = "FivePD",
                Message = "ALPR has been disabled.",
                PlayAudio = this._generalConfig.Audio.Files.AlprToggle,
            });
        }
    }

    private async void Asd(Vehicle nearbyVehicle)
    {
        await BaseScript.Delay(4000);
        BaseScript.TriggerServerEvent(Events.ALPR.Check, nearbyVehicle.NetworkId);
    }
}