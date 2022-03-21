// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Client.Models;
using FivePD.Gamemode.Client.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FivePD.Gamemode.Client.Menus
{
    /// <inheritdoc />
    public class DutyMenu : Menu
    {
        private readonly LocalizationService _localizationService;
        private readonly NuiService _nuiService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DutyMenu"/> class.
        /// </summary>
        /// <param name="localizationService">The <see cref="LocalizationService"/> to use.</param>
        /// <param name="nuiService">The <see cref="NuiService"/> to use.</param>
        /// >
        public DutyMenu(LocalizationService localizationService, NuiService nuiService)
            : base("duty_menu", "DutyMenu.Title", "F10", true)
        {
            this._localizationService = localizationService;
            this._nuiService = nuiService;
            this.CreateAndSetItems();
        }

        /// <inheritdoc />
        protected sealed override void CreateAndSetItems()
        {
            var items = new List<MenuItem>
            {
                new MenuItemToggle("DutyMenu.OnDuty.Title", "DutyMenu.OnDuty.Description", (body, cb) =>
                {
                    Debug.WriteLine("Duty status toggle");
                    cb.Invoke(string.Empty);
                }),
                new MenuItemList("DutyMenu.GetLoadout.Title", "DutyMenu.GetLoadout.Description", new List<string> { "Loadout 1", "Loadout 2", }, (body, cb) =>
                    {
                        Debug.WriteLine("On list select");
                        cb.Invoke(string.Empty);
                    }),
                new MenuItemList("DutyMenu.ChangeLanguage.Title", "DutyMenu.ChangeLanguage.Description", this._localizationService.Locales.Select(x => x.Title).ToList(), (body, cb) =>
                {
                    dynamic d = body;
                    var locale = this._localizationService.Locales.ElementAt((int)d.index);
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