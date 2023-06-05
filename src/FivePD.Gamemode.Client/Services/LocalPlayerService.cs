using System;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Models;
using FivePD.Gamemode.Common.Enums;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc />
internal class LocalPlayerService : ILocalPlayerService
{
    private readonly ILocalizationService _localizationService;

    // TODO: I'd create a RandomService (using Guid as seed), so the generated numbers won't be the same anywhere
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalPlayerService" /> class.
    /// </summary>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    public LocalPlayerService(ILocalizationService localizationService)
    {
        this._localizationService = localizationService;

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
        Cfx.API.SetDiscordAppId("1112111975862898718");
        Cfx.API.SetDiscordRichPresenceAsset("fpd_generic");

#pragma warning disable
        // TODO: find a nice user icon, red for off duty, green for on duty
        // Cfx.API.SetDiscordRichPresenceAssetSmall(dutyStatus > 0 ? "fpd_status_on_duty" : "fpd_status_off_duty")
#pragma warning restore

        var status = dutyStatus switch
        {
            FDutyStatus.OffDuty => this._localizationService.Get("PLAYER_STATUS.OFF_DUTY.GENERIC"),
            FDutyStatus.OnDutyAndReceiving => this._localizationService.Get("PLAYER_STATUS.ON_DUTY.GENERIC_RECEIVING"),
            FDutyStatus.OnDutyAndNotReceiving => this._localizationService.Get("PLAYER_STATUS.ON_DUTY.GENERIC_NOT_RECEIVING"),
            _ => "Unknown",
        };
        Cfx.API.SetRichPresence(status);
        Cfx.API.SetDiscordRichPresenceAssetText("FivePD is a law enforcement gamemode for FiveM. 👮 twitter.com/five_pd");
        Cfx.API.SetDiscordRichPresenceAssetSmallText($"FivePD v{Assembly.GetExecutingAssembly().GetName().Version}");
    }
}