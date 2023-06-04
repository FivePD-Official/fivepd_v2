using System.Collections.Generic;
using System.Linq;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.ConfigModels.MenuConfigs;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Common.UIModels;
using FivePD.Gamemode.Server.EntityWrappers.PedWrapper;
using FivePD.Gamemode.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450
#pragma warning disable S125
#pragma warning disable SA1515

namespace FivePD.Gamemode.Server.UIModels;

/// <inheritdoc />
public sealed class DutyMenu : Menu
{
    private readonly IConfigService _configService;
    private readonly ILocalizationService _localizationService;
    private readonly DutyMenuConfig _dutyMenuConfig;

    private MenuListItem _changeLanguageListItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="DutyMenu"/> class.
    /// </summary>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    /// <param name="eventService">The <see cref="IEventService"/> to use.</param>
    public DutyMenu(IConfigService configService, ILocalizationService localizationService, IEventService eventService)
        : base("fivepd_duty_menu", "DUTY_MENU.TITLE", "F10", true)
    {
        this._configService = configService;
        this._localizationService = localizationService;
        this._dutyMenuConfig = configService.MenuConfig.DutyMenu;
        this.Items = this.CreateItems();
    }

    /// <summary>
    /// Gets the duty checkbox item.
    /// </summary>
    public MenuCheckboxItem DutyToggleItem { get; private set; }

    /// <summary>
    /// Gets the vehicle spawner item.
    /// </summary>
    public MenuListItem VehicleSpawnerItem { get; private set; }

    /// <summary>
    /// Gets the loadout spawner item.
    /// </summary>
    public MenuListItem LoadoutItem { get; private set; }

    private List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        if (this._dutyMenuConfig.DutyToggle.Enabled)
        {
            this.DutyToggleItem = new MenuCheckboxItem("DUTY_MENU.ITEM.ON_DUTY.TITLE", "DUTY_MENU.ITEM.ON_DUTY.DESCRIPTION");
            this.DutyToggleItem.OnSelect += (player, @checked) =>
            {
                player.DutyStatus.Value = @checked ? FDutyStatus.OnDutyAndReceiving : FDutyStatus.OffDuty;
            };
            items.Add(this.DutyToggleItem);
        }

        if (this._dutyMenuConfig.SpawnVehicle.Enabled)
        {
            this.VehicleSpawnerItem = new MenuListItem(
                "DUTY_MENU.ITEM.SPAWN_VEHICLE.TITLE",
                "DUTY_MENU.ITEM.SPAWN_VEHICLE.DESCRIPTION",
                this._configService.VehiclesConfig.Police.Select(vehicle => vehicle.Name).ToList());
            this.VehicleSpawnerItem.OnSelect += (player, index) =>
            {
                // var policeVehicle = this._policeVehicles.ElementAt(index);
                // this._entityService.SpawnVehicle((uint)Cfx.API.GetHashKey(policeVehicle.Model), Game.PlayerPed.Position + (Game.PlayerPed.ForwardVector * 5f), Game.PlayerPed.Heading);
            };
            items.Add(this.VehicleSpawnerItem);
        }

        if (this._dutyMenuConfig.GetLoadout.Enabled)
        {
            this.LoadoutItem = new MenuListItem(
                "DUTY_MENU.ITEM.GET_LOADOUT.TITLE",
                "DUTY_MENU.ITEM.GET_LOADOUT.DESCRIPTION",
                this._configService.LoadoutsConfig.Select(x => x.Title).ToList());
            this.LoadoutItem.OnSelect += (player, index) =>
            {
                var playerPed = FPed.FromFPlayer(player);
                var loadout = this._configService.LoadoutsConfig.ElementAt(index);
                foreach (var weaponItem in loadout.Weapons)
                {
                    var weaponHash = (FWeaponHash)Cfx.API.GetHashKey(weaponItem.Key);
                    playerPed.Weapons.Give(weaponHash, weaponItem.Ammunition, false, true);

                    foreach (var componentKey in weaponItem.ComponentKeys)
                    {
                        playerPed.Weapons.AddComponentToWeapon(weaponHash, (uint)Cfx.API.GetHashKey(componentKey));
                    }
                }
            };
            items.Add(this.LoadoutItem);
        }

        this._changeLanguageListItem = new MenuListItem(
            "DUTY_MENU.ITEM.CHANGE_LANGUAGE.TITLE",
            "DUTY_MENU.ITEM.CHANGE_LANGUAGE.DESCRIPTION",
            this._localizationService.AvailableLocalizations.Values.ToList());
        this._changeLanguageListItem.OnSelect += (player, index) =>
        {
            var locale = this._localizationService.AvailableLocalizations.Keys.ToList()[index];
            player.GetUnderlyingPlayer().TriggerEvent(Events.UI.Player.LocalizationChange, this._localizationService.GetLocalization(locale));
        };
        items.Add(this._changeLanguageListItem);

        return items;
    }
}