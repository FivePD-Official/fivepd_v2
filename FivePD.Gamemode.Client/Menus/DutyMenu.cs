// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.ConfigModels;
using FivePD.Gamemode.Client.Models;
using FivePD.Gamemode.Client.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450

namespace FivePD.Gamemode.Client.Menus
{
    /// <inheritdoc />
    public class DutyMenu : Menu
    {
        private readonly LocalizationService _localizationService;
        private readonly NuiService _nuiService;
        private readonly ResourceService _resourceService;
        private readonly List<Loadout> _loadouts;

        /// <summary>
        /// Initializes a new instance of the <see cref="DutyMenu"/> class.
        /// </summary>
        /// <param name="localizationService">The <see cref="LocalizationService"/> to use.</param>
        /// <param name="nuiService">The <see cref="NuiService"/> to use.</param>
        /// <param name="resourceService">The <see cref="ResourceService"/> to use.</param>
        /// >
        public DutyMenu(LocalizationService localizationService, NuiService nuiService, ResourceService resourceService)
            : base("duty_menu", "DutyMenu.Title", "F10", true)
        {
            this._localizationService = localizationService;
            this._nuiService = nuiService;
            this._resourceService = resourceService;

            this._loadouts = this._resourceService.LoadFile<List<Loadout>>("/Config/loadouts.json");
            this.CreateAndSetItems();
        }

        /// <inheritdoc />
        protected sealed override void CreateAndSetItems()
        {
            var items = new List<MenuItem>
            {
                new MenuItemToggle("DutyMenu.OnDuty.Title", "DutyMenu.OnDuty.Description", (index, cb) =>
                {
                    Debug.WriteLine("Duty status toggle");
                    cb.Invoke(string.Empty);
                }),
                new MenuItemList("DutyMenu.GetLoadout.Title", "DutyMenu.GetLoadout.Description", this._loadouts.Select(x => x.Title).ToList(), (index, cb) =>
                {
                    var loadout = this._loadouts.ElementAt(index);

                    foreach (var weaponItem in loadout.Weapons)
                    {
                        var weapon = (WeaponHash)Cfx.API.GetHashKey(weaponItem.Key);
                        Game.PlayerPed.Weapons.Give(weapon, weaponItem.Ammunition, false, true);

                        foreach (var componentKey in weaponItem.ComponentKeys)
                        {
                            var componentHash = (uint)Cfx.API.GetHashKey(componentKey);
                            var weaponHash = (uint)weapon;
                            if (!Cfx.API.DoesWeaponTakeWeaponComponent(weaponHash, componentHash))
                            {
                                continue;
                            }

                            Cfx.API.GiveWeaponComponentToPed(Game.PlayerPed.Handle, weaponHash, componentHash);
                        }
                    }

                    cb.Invoke(string.Empty);
                }),
                new MenuItemList("DutyMenu.ChangeLanguage.Title", "DutyMenu.ChangeLanguage.Description", this._localizationService.Locales.Select(x => x.Title).ToList(), (index, cb) =>
                {
                    var locale = this._localizationService.Locales.ElementAt(index);
                    this._localizationService.LoadLocalization(locale.Key);
                    this._nuiService.SendMessage(new JObject
                    {
                        { "type", Events.NuiEventType.LocalizationChange },
                        { "data", JsonConvert.SerializeObject(this._localizationService.CurrentLocalization) },
                    });
                    cb.Invoke(string.Empty);
                }),
            };
            this.SetItems(items);
        }
    }
}