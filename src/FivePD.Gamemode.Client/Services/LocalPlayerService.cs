using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Models;
using FivePD.Gamemode.Common.ConfigModels;
using FivePD.Gamemode.Common.Enums;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc cref="FivePD.Gamemode.Client.Interfaces.ILocalPlayerService" />
internal class LocalPlayerService : ILocalPlayerService, IServiceWithEvents
{
    private readonly ILocalizationService _localizationService;
    private readonly IConfigService _configService;
    private readonly ITickService _tickService;

    private GeneralConfig _generalConfig = new()
    {
        FeatureFlags = new()
        {
            DutyNotification = false,
            DisableWantedLevel = false,
            DisableWantedLevelAllTime = false,
        },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalPlayerService" /> class.
    /// </summary>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    public LocalPlayerService(ILocalizationService localizationService, IConfigService configService, ITickService tickService)
    {
        this._localizationService = localizationService;
        this._configService = configService;
        this._tickService = tickService;

#pragma warning disable CS4014
        this.GetGeneralConfigAsync();
#pragma warning restore CS4014

        this.UpdateDiscordStatus(FDutyStatus.OffDuty);
        this.Player.DutyStatus.Subscribe(this.ListenForDutyStatusChanges);
    }

    /// <inheritdoc />
    public FPlayer Player { get; } = FPlayer.FromNetId(Game.Player.ServerId);

    /// <inheritdoc/>
    public void RegisterEvents()
    {
        this._tickService.OnTick += this.OnTickAsync;
    }

    private Task OnTickAsync()
    {
        if (!this._generalConfig.FeatureFlags.DisableWantedLevel)
        {
            return Task.FromResult(0);
        }

        if (!this._generalConfig.FeatureFlags.DisableWantedLevelAllTime &&
            (this._generalConfig.FeatureFlags.DisableWantedLevelAllTime || this.Player.DutyStatus.Value != FDutyStatus.OnDuty))
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
    }

    private void ListenForDutyStatusChanges(FDutyStatus dutyStatus)
    {
        // Update the player's Discord presence.
        this.UpdateDiscordStatus(dutyStatus);
    }

    // Called when the player's duty status changes.
    private void UpdateDiscordStatus(FDutyStatus dutyStatus)
    {
        Cfx.API.SetDiscordAppId("1112111975862898718");
        Cfx.API.SetDiscordRichPresenceAsset("fpd_generic");

#pragma warning disable
        // TODO: find a nice user icon, red for off duty, green for on duty
        // Cfx.API.SetDiscordRichPresenceAssetSmall(dutyStatus > 0 ? "fpd_status_on_duty" : "fpd_status_off_duty")
#pragma warning restore

        var status = dutyStatus switch
        {
            FDutyStatus.OffDuty => this._localizationService.Get("PLAYER_STATUS.OFF_DUTY.GENERIC"),
            FDutyStatus.OnDuty => this._localizationService.Get("PLAYER_STATUS.ON_DUTY.GENERIC"),
            _ => "Unknown",
        };
        Cfx.API.SetRichPresence(status);
        Cfx.API.SetDiscordRichPresenceAssetText("FivePD is a law enforcement gamemode for FiveM. 👮 twitter.com/five_pd");
        Cfx.API.SetDiscordRichPresenceAssetSmallText($"FivePD v{Assembly.GetExecutingAssembly().GetName().Version}");
    }
}