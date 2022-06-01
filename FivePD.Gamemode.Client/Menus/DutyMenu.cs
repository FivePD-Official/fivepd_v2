// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.ConfigModels;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450

namespace FivePD.Gamemode.Client.Menus
{
    /// <inheritdoc />
    public class DutyMenu : Menu
    {
        private readonly ILocalizationService _localizationService;
        private readonly INuiService _nuiService;
        private readonly IResourceService _resourceService;
        private readonly IEntityService _entityService;
        private readonly List<Loadout> _loadouts;
        private readonly List<Vehicles.PoliceVehicle> _policeVehicles;

        /// <summary>
        /// Initializes a new instance of the <see cref="DutyMenu"/> class.
        /// </summary>
        /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
        /// <param name="nuiService">The <see cref="INuiService"/> to use.</param>
        /// <param name="resourceService">The <see cref="IResourceService"/> to use.</param>
        /// <param name="entityService">The <see cref="IEntityService"/> to use.</param>
        /// >
        public DutyMenu(ILocalizationService localizationService, INuiService nuiService, IResourceService resourceService, IEntityService entityService)
            : base("duty_menu", "DutyMenu.Title", "F10", true)
        {
            this._localizationService = localizationService;
            this._nuiService = nuiService;
            this._resourceService = resourceService;
            this._entityService = entityService;

            this._loadouts = this._resourceService.LoadFile<List<Loadout>>("/Config/loadouts.json");
            this._policeVehicles = this._resourceService.LoadFile<Vehicles>("/Config/vehicles.json").Police;
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
                new MenuItemList("DutyMenu.SpawnVehicle.Title", "DutyMenu.SpawnVehicle.Description", this._policeVehicles.Select(vehicle => vehicle.Name).ToList(), async (index, cb) =>
                {
                    var policeVehicle = this._policeVehicles.ElementAt(index);
                    await this._entityService.SpawnVehicle((uint)Cfx.API.GetHashKey(policeVehicle.Model), Game.PlayerPed.Position + (Game.PlayerPed.ForwardVector * 5f), Game.PlayerPed.Heading);
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
                        { "data", JObject.FromObject(this._localizationService.CurrentLocalization) },
                    });
                    cb.Invoke(string.Empty);
                }),
            };
            this.SetItems(items);
        }
    }
}