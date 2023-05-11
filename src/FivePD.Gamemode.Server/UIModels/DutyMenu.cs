using System.Collections.Generic;
using System.Linq;
using FivePD.Common;
using FivePD.Common.ConfigModels.MenuConfigs;
using FivePD.Common.Models;
using FivePD.Common.UIModels;
using FivePD.Gamemode.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450
#pragma warning disable S125
#pragma warning disable SA1515

namespace FivePD.Gamemode.Server.UIModels
{
    /// <inheritdoc />
    public class DutyMenu : Menu
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
        public DutyMenu(IConfigService configService, ILocalizationService localizationService)
            : base("DUTY_MENU.TITLE", "F10", true)
        {
            this._configService = configService;
            this._localizationService = localizationService;
            this._dutyMenuConfig = configService.GetMenuConfig().DutyMenu;
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
                    this._configService.GetVehiclesConfig().Police.Select(vehicle => vehicle.Name).ToList());
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
                    this._configService.GetLoadoutsConfig().Select(x => x.Title).ToList());
                this.LoadoutItem.OnSelect += (player, index) =>
                {
                    // var loadout = this._loadouts.ElementAt(index);
                    //
                    // foreach (var weaponItem in loadout.Weapons)
                    // {
                    //     var weapon = (WeaponHash)Cfx.API.GetHashKey(weaponItem.Key);
                    //     Game.PlayerPed.Weapons.Give(weapon, weaponItem.Ammunition, false, true);
                    //
                    //     foreach (var componentKey in weaponItem.ComponentKeys)
                    //     {
                    //         var componentHash = (uint)Cfx.API.GetHashKey(componentKey);
                    //         var weaponHash = (uint)weapon;
                    //         if (!Cfx.API.DoesWeaponTakeWeaponComponent(weaponHash, componentHash))
                    //         {
                    //             continue;
                    //         }
                    //
                    //         Cfx.API.GiveWeaponComponentToPed(Game.PlayerPed.Handle, weaponHash, componentHash);
                    //     }
                    // }
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
            this._changeLanguageListItem.OnIndexChange += (player, index) =>
            {
                // Debug.WriteLine($"_changeLanguageListItem.OnIndexChange - {player.GetUnderlyingPlayer().Name} - {index}");
            };
            items.Add(this._changeLanguageListItem);

            return items;
        }
    }
}