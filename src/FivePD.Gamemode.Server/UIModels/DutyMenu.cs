// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common.ConfigModels.MenuConfigs;
using FivePD.Common.Models;
using FivePD.Common.UIModels;
using FivePD.Common.UIModels.Interfaces;
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
#pragma warning disable SA1201
        private readonly IConfigService _configService;
#pragma warning restore SA1201
        private readonly ILocalizationService _localizationService;
        private readonly IMenuService _menuService;
        private readonly DutyMenuConfig _dutyMenuConfig;

        private MenuListItem _changeLanguageListItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="DutyMenu"/> class.
        /// </summary>
        /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
        /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
        /// <param name="menuService">The <see cref="IMenuService"/> to use.</param>
        public DutyMenu(IConfigService configService, ILocalizationService localizationService, IMenuService menuService)
            : base("DutyMenu.Title", "F10", true)
        {
            this._configService = configService;
            this._localizationService = localizationService;
            this._menuService = menuService;
            this._dutyMenuConfig = configService.GetMenuConfig().DutyMenu;
            this.SetItems(this.CreateItems());
        }

        /// <summary>
        /// Gets the duty checkbox item.
        /// </summary>
        public MenuCheckboxItem DutyCheckboxItem { get; private set; }

        /// <summary>
        /// Gets the vehicle spawner item.
        /// </summary>
        public MenuListItem SpawnVehicleListItem { get; private set; }

        /// <summary>
        /// Gets the loadout spawner item.
        /// </summary>
        public MenuListItem GetLoadoutListItem { get; private set; }

        private List<MenuItem> CreateItems()
        {
            var items = new List<MenuItem>();

            if (this._dutyMenuConfig.DutyToggle.Enabled)
            {
                this.DutyCheckboxItem = new MenuCheckboxItem("DutyMenu.OnDuty.Title", "DutyMenu.OnDuty.Description");
                this.DutyCheckboxItem.OnSelect += (player, @checked) =>
                {
                    player.DutyStatus.Value = @checked ? FDutyStatus.OnDutyAndReceiving : FDutyStatus.OffDuty;
                    var dutyMenu = player.GetMenu<DutyMenu>();
                    this._menuService.UpdateMenuItem<DutyMenu>(player, () => dutyMenu.DutyCheckboxItem, d => d.Title, @checked ? "Checked" : "Not checked");
                };
                items.Add(this.DutyCheckboxItem);
            }

            if (this._dutyMenuConfig.SpawnVehicle.Enabled)
            {
                this.SpawnVehicleListItem = new MenuListItem(
                    "DutyMenu.SpawnVehicle.Title",
                    "DutyMenu.SpawnVehicle.Description",
                    this._configService.GetVehiclesConfig().Police.Select(vehicle => vehicle.Name).ToList());
                this.SpawnVehicleListItem.OnSelect += (player, index) =>
                {
                    // var policeVehicle = this._policeVehicles.ElementAt(index);
                    // this._entityService.SpawnVehicle((uint)Cfx.API.GetHashKey(policeVehicle.Model), Game.PlayerPed.Position + (Game.PlayerPed.ForwardVector * 5f), Game.PlayerPed.Heading);
                };
                items.Add(this.SpawnVehicleListItem);
            }

            if (this._dutyMenuConfig.GetLoadout.Enabled)
            {
                this.GetLoadoutListItem = new MenuListItem(
                    "DutyMenu.GetLoadout.Title",
                    "DutyMenu.GetLoadout.Description",
                    this._configService.GetLoadoutsConfig().Select(x => x.Title).ToList());
                this.GetLoadoutListItem.OnSelect += (player, index) =>
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
                items.Add(this.GetLoadoutListItem);
            }

            this._changeLanguageListItem = new MenuListItem(
                "DutyMenu.ChangeLanguage.Title",
                "DutyMenu.ChangeLanguage.Description",
                this._localizationService.GetLocales().Select(x => x.Title).ToList());
            this._changeLanguageListItem.OnSelect += (player, index) =>
            {
                Debug.WriteLine($"_changeLanguageListItem.OnSelect - {player.GetUnderlyingPlayer().Name} - {index}");
                // var locale = this._localizationService.Locales.ElementAt(index);
                // this._localizationService.LoadLocalization(locale.Key);
                // this._nuiService.SendMessage(new JObject
                // {
                //     { "type", Events.NuiEventType.LocalizationChange },
                //     { "data", JObject.FromObject(this._localizationService.CurrentLocalization) },
                // });
            };
            this._changeLanguageListItem.OnIndexChange += (player, index) =>
            {
                Debug.WriteLine($"_changeLanguageListItem.OnIndexChange - {player.GetUnderlyingPlayer().Name} - {index}");
            };
            items.Add(this._changeLanguageListItem);

            return items;
        }
    }
}