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
#pragma warning disable CS4014
#pragma warning disable S3241

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

        private MenuCheckboxItem _dutyCheckboxItem;
        private MenuListItem _spawnVehicleListItem;
        private MenuListItem _getLoadoutListItem;
        private MenuListItem _changeLanguageListItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="DutyMenu"/> class.
        /// </summary>
        /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
        /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
        public DutyMenu(IConfigService configService, ILocalizationService localizationService)
            : base("DutyMenu.Title", "F10", true)
        {
            this._configService = configService;
            this._localizationService = localizationService;
            this._dutyMenuConfig = configService.GetMenuConfig().DutyMenu;
            this.SetItems(this.CreateItems());
        }

        /// <summary>
        /// Gets qweqwe.
        /// </summary>
        /// <returns>asdasd.</returns>
        public IPublicMenuCheckboxItem DutyCheckboxItem => this._dutyCheckboxItem;

        /// <summary>
        /// Gets qweqwe.
        /// </summary>
        /// <returns>asdasd.</returns>
        public IPublicMenuListItem SpawnVehicleItem => this._spawnVehicleListItem;

        /// <summary>
        /// Gets qweqwe.
        /// </summary>
        /// <returns>asdasd.</returns>
        public IPublicMenuListItem LoadoutItem => this._getLoadoutListItem;

        private List<MenuItem> CreateItems()
        {
            var items = new List<MenuItem>();

            if (this._dutyMenuConfig.DutyToggle.Enabled)
            {
                this._dutyCheckboxItem = new MenuCheckboxItem("DutyMenu.OnDuty.Title", "DutyMenu.OnDuty.Description");
                this._dutyCheckboxItem.OnSelect += (player, @checked) =>
                {
                    player.DutyStatus.Value = @checked ? FDutyStatus.OnDutyAndReceiving : FDutyStatus.OffDuty;
                };
                items.Add(this._dutyCheckboxItem);
            }

            if (this._dutyMenuConfig.SpawnVehicle.Enabled)
            {
                this._spawnVehicleListItem = new MenuListItem(
                    "DutyMenu.SpawnVehicle.Title",
                    "DutyMenu.SpawnVehicle.Description",
                    this._configService.GetVehiclesConfig().Police.Select(vehicle => vehicle.Name).ToList());
                this._spawnVehicleListItem.OnSelect += (player, index) =>
                {
                    // var policeVehicle = this._policeVehicles.ElementAt(index);
                    // this._entityService.SpawnVehicle((uint)Cfx.API.GetHashKey(policeVehicle.Model), Game.PlayerPed.Position + (Game.PlayerPed.ForwardVector * 5f), Game.PlayerPed.Heading);
                };
                items.Add(this._spawnVehicleListItem);
            }

            if (this._dutyMenuConfig.GetLoadout.Enabled)
            {
                this._getLoadoutListItem = new MenuListItem(
                    "DutyMenu.GetLoadout.Title",
                    "DutyMenu.GetLoadout.Description",
                    this._configService.GetLoadoutsConfig().Select(x => x.Title).ToList());
                this._getLoadoutListItem.OnSelect += (player, index) =>
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
                items.Add(this._getLoadoutListItem);
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