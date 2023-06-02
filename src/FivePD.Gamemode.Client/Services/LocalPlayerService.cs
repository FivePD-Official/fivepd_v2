using System;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Gamemode.Client.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc />
public class LocalPlayerService : ILocalPlayerService
{
    private readonly ILocalizationService _localizationService;

    // TODO: I'd create a RandomService (using Guid as seed), so the generated numbers won't be the same anywhere
    private readonly Random _random = new();

    private readonly string[] _onDutyOptions;
    private readonly string[] _offDutyOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalPlayerService" /> class.
    /// </summary>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    public LocalPlayerService(ILocalizationService localizationService)
    {
        this._localizationService = localizationService;

        // Can't use reflection... so let's just hard code this for now...
        this._onDutyOptions = this.GetPossibleOnDutyStatuses();
        this._offDutyOptions = this.GetPossibleOffDutyStatuses();

        this.Player.DutyStatus.Subscribe(this.ListenForDutyStatusChanges);
    }

    /// <inheritdoc />
    public FPlayer Player { get; } = FPlayer.FromNetId(Game.Player.ServerId);

    private void ListenForDutyStatusChanges(FDutyStatus dutyStatus)
    {
        // Update the player's Discord presence.
        this.UpdateDiscordStatus(dutyStatus);
    }

    // Called when the player's duty status changes.
    private void UpdateDiscordStatus(FDutyStatus dutyStatus)
    {
        // TODO: not working
        // Sets the Discord App ID.
        Cfx.API.SetDiscordAppId("1112111975862898718");

        // Configures primary asset..
        Cfx.API.SetDiscordRichPresenceAsset("fpd_generic");

        // Configures small asset according to duty status.
        Cfx.API.SetDiscordRichPresenceAssetSmall(dutyStatus > 0 ? "fpd_status_on_duty" : "fpd_status_off_duty");

        // Configures the presence data.
        Cfx.API.SetRichPresence(this.GetRandomStatus(dutyStatus));
        Cfx.API.SetDiscordRichPresenceAssetText("FivePD is a law enforcement gamemode for Cfx.re's FiveM. 👮 twitter/five_pd");
        Cfx.API.SetDiscordRichPresenceAssetSmallText($"FivePD v{Assembly.GetExecutingAssembly().GetName().Version}");
    }

    private string GetRandomStatus(FDutyStatus dutyStatus)
    {
        // If on duty
        if (dutyStatus > 0)
        {
            // Get random status from on duty options.
            return this._onDutyOptions[this._random.Next(this._onDutyOptions.Length)];
        }

        // Get random status from off duty options.
        return this._offDutyOptions[this._random.Next(this._offDutyOptions.Length)];
    }

    private string[] GetPossibleOnDutyStatuses()
    {
        return new[]
        {
            this._localizationService.Get("PLAYER_STATUS.ON_DUTY.OPTION_A"),
            this._localizationService.Get("PLAYER_STATUS.ON_DUTY.OPTION_B"),
            this._localizationService.Get("PLAYER_STATUS.ON_DUTY.OPTION_C"),
            this._localizationService.Get("PLAYER_STATUS.ON_DUTY.OPTION_D"),
        };
    }

    private string[] GetPossibleOffDutyStatuses()
    {
        return new[]
        {
            this._localizationService.Get("PLAYER_STATUS.OFF_DUTY.OPTION_A"),
            this._localizationService.Get("PLAYER_STATUS.OFF_DUTY.OPTION_B"),
            this._localizationService.Get("PLAYER_STATUS.OFF_DUTY.OPTION_C"),
            this._localizationService.Get("PLAYER_STATUS.OFF_DUTY.OPTION_D"),
        };
    }
}