using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FivePD.Common;
using FivePD.Common.ConfigModels.MenuConfigs;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.UIModels.MenuModels;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450
#pragma warning disable S125
#pragma warning disable SA1515

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class DutyMenu : Menu
{
    private readonly IConfigService _configService;
    private readonly ILocalizationService _localizationService;
    private readonly DutyMenuConfig _dutyMenuConfig;

    private MenuListItem _changeLanguageListItem;

    public DutyMenu(IConfigService configService, ILocalizationService localizationService)
        : base(Controls.DutyMenu.Id, Controls.DutyMenu.Title, Controls.DutyMenu.Key, true)
    {
        this._configService = configService;
        this._localizationService = localizationService;
        this._dutyMenuConfig = configService.MenuConfig.DutyMenu;
    }

    public MenuCheckboxItem DutyToggleItem { get; private set; }

    public MenuListItem VehicleSpawnerItem { get; private set; }

    public MenuListItem LoadoutItem { get; private set; }

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        if (this._dutyMenuConfig.DutyToggle.Enabled)
        {
            this.DutyToggleItem = new MenuCheckboxItem("DUTY_MENU.ITEM.ON_DUTY.TITLE", "DUTY_MENU.ITEM.ON_DUTY.DESCRIPTION");
            this.DutyToggleItem.OnSelect += async (player, @checked) =>
            {
                player.DutyStatus.Value = @checked ? FDutyStatus.OnDuty : FDutyStatus.OffDuty;
            };
            items.Add(this.DutyToggleItem);
        }

        if (this._dutyMenuConfig.SpawnVehicle.Enabled)
        {
            this.VehicleSpawnerItem = new MenuListItem(
                "DUTY_MENU.ITEM.SPAWN_VEHICLE.TITLE",
                "DUTY_MENU.ITEM.SPAWN_VEHICLE.DESCRIPTION",
                this._configService.VehiclesConfig.Police.Select(vehicle => vehicle.Name).ToList());
            this.VehicleSpawnerItem.OnSelect += async (player, index) =>
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
            this.LoadoutItem.OnSelect += async (player, index) =>
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
        this._changeLanguageListItem.OnSelect += async (player, index) =>
        {
            var locale = this._localizationService.AvailableLocalizations.Keys.ToList()[index];
            player.GetUnderlyingPlayer().TriggerEvent(Events.UI.Player.LocalizationChange, this._localizationService.GetLocalization(locale));
        };
        items.Add(this._changeLanguageListItem);

        return items;
    }
}