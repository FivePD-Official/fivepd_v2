using System;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.DTO;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Client.Models;
using FivePD.Common;
using FivePD.Common.ConfigModels;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.ILocalPlayerService" />
internal class LocalPlayerService : ILocalPlayerService, IOnInit, IOnDestroy
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ILocalizationService _localizationService;
    private readonly IConfigService _configService;
    private readonly ITickService _tickService;
    private readonly IUIManagerService _uiManagerService;

    private readonly InformationUpdateDTO _information = new();
    private GeneralConfig _generalConfig = new()
    {
        Features = new FeatureFlagsConfig
        {
            DiscordRichPresence = true,
            DutyNotification = false,
            DisableWantedLevel = false,
            DisableWantedLevelAllTime = false,
        },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalPlayerService" /> class.
    /// </summary>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="uiManagerService">The <see cref="IUIManagerService"/> to use.</param>
    public LocalPlayerService(
        EventHandlerDictionary eventHandlerDictionary,
        ILocalizationService localizationService,
        IConfigService configService,
        ITickService tickService,
        IUIManagerService uiManagerService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._localizationService = localizationService;
        this._configService = configService;
        this._tickService = tickService;
        this._uiManagerService = uiManagerService;

#pragma warning disable CS4014
        this.GetGeneralConfigAsync();
#pragma warning restore CS4014

        this.Player.DutyStatus.Subscribe(this.ListenForDutyStatusChanges);
    }

    /// <inheritdoc />
    public FPlayer Player { get; } = FPlayer.FromNetId(Game.Player.ServerId);

    /// <inheritdoc />
    public FUnitAvailability Availability { get; private set; } = FUnitAvailability.NotReceiving;

    /// <inheritdoc />
    public string Status { get; private set; } = string.Empty;

    /// <inheritdoc/>
    public void OnInit()
    {
        this._tickService.OnTick += this.OnTickAsync;
        this._eventHandlerDictionary[Events.Player.UpdateUnitAvailability] += new Action<int>(availability => this.ListenForAvailabilityStatusChanges((FUnitAvailability)availability));
        this._eventHandlerDictionary[Events.Player.UpdateUnitStatus] += new Action<string>(this.ListenForStatusChanges);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Player.GetDetails);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Player.DismissedWelcomeModal);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Player.ChangeDivision);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Player.ChangeRank);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Player.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Player.ChangeRole);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Player.ChangeFixedRank);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.User.GetAllSimple);
    }

    /// <inheritdoc/>
    public void OnDestroy()
    {
        this._tickService.OnTick -= this.OnTickAsync;
    }

    /// <inheritdoc/>
    public void UpdateInformationCallout(ClientCalloutWrapper clientCalloutWrapper)
    {
        this._information.CalloutWrapper = clientCalloutWrapper;
        this.UpdateInformationInUI();
    }

    /// <inheritdoc/>
    public void UpdateInformationInUI()
    {
        this._uiManagerService.SendMessage(JsonConvert.SerializeObject(this._information));
    }

    private Task OnTickAsync()
    {
        if (!this._generalConfig.Features.DisableWantedLevel)
        {
            return Task.FromResult(0);
        }

        if (!this._generalConfig.Features.DisableWantedLevelAllTime &&
            (this._generalConfig.Features.DisableWantedLevelAllTime || this.Player.DutyStatus.Value != FDutyStatus.OnDuty))
        {
            return Task.FromResult(0);
        }

        Game.Player.WantedLevel = 0;
        Cfx.API.SetMaxWantedLevel(0);
        Cfx.API.SetPoliceIgnorePlayer(Game.Player.Handle, true);
        Cfx.API.SetDispatchCopsForPlayer(Game.Player.Handle, false);
        Cfx.API.SetPlayerWantedLevel(Game.Player.Handle, 0, false);
        Cfx.API.SetPlayerWantedLevelNow(Game.Player.Handle, false);

        return Task.FromResult(0);
    }

#pragma warning disable S3241
    private async Task GetGeneralConfigAsync()
#pragma warning restore S3241
    {
        this._generalConfig = await this._configService.GeneralConfig;
        this.UpdateDiscordStatus();
    }

    private void ListenForDutyStatusChanges(FDutyStatus dutyStatus)
    {
        this._information.DutyStatus = dutyStatus;
        this.UpdateDiscordStatus();
        this.UpdateInformationInUI();
    }

    private void ListenForAvailabilityStatusChanges(FUnitAvailability availability)
    {
        this.Availability = availability;
        this._information.Availability = availability;
        this.UpdateDiscordStatus();
        this.UpdateInformationInUI();
    }

    private void ListenForStatusChanges(string status)
    {
        this.Status = status;
        this._information.Status = status;
        this.UpdateInformationInUI();
    }

    // Called when the player's duty status changes.
    private void UpdateDiscordStatus()
    {
        if (!this._generalConfig.Features.DiscordRichPresence)
        {
            Cfx.API.SetDiscordAppId(string.Empty);
            Cfx.API.SetDiscordRichPresenceAsset(string.Empty);
            Cfx.API.SetRichPresence(string.Empty);
            Cfx.API.SetDiscordRichPresenceAssetText(string.Empty);
            Cfx.API.SetDiscordRichPresenceAssetSmallText(string.Empty);
            return;
        }

        Cfx.API.SetDiscordAppId("1112111975862898718");
        Cfx.API.SetDiscordRichPresenceAsset("fpd_generic");

#pragma warning disable
        // TODO: find a nice user icon, red for off duty, green for on duty
        // Cfx.API.SetDiscordRichPresenceAssetSmall(dutyStatus > 0 ? "fpd_status_on_duty" : "fpd_status_off_duty")
#pragma warning restore

        var status = this._information.DutyStatus switch
        {
            FDutyStatus.OffDuty => this._localizationService.Get("PLAYER_STATUS.OFF_DUTY.GENERIC"),
            FDutyStatus.OnDuty => this._localizationService.Get("PLAYER_STATUS.ON_DUTY.GENERIC"),
            _ => "Unknown",
        };
        Cfx.API.SetRichPresence(status);
        Cfx.API.SetDiscordRichPresenceAssetText("FivePD is a law enforcement mod for FiveM. 👮 twitter.com/five_pd");
        Cfx.API.SetDiscordRichPresenceAssetSmallText($"FivePD v{Assembly.GetExecutingAssembly().GetName().Version}");
    }
}