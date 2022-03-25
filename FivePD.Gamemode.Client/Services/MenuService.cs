// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Dynamic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Menus;
using FivePD.Gamemode.Client.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class MenuService : IMenuService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly NuiService _nuiService;
        private readonly KeybindService _keybindService;
        private readonly LocalizationService _localizationService;
        private readonly ResourceService _resourceService;
        private readonly List<Menu> _menus;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="nuiService">The <see cref="NuiService" /> to use.</param>
        /// <param name="keybindService">The <see cref="KeybindService" /> to use.</param>
        /// <param name="localizationService">The <see cref="LocalizationService" /> to use.</param>
        /// <param name="resourceService">The <see cref="ResourceService"/> to use.</param>
        public MenuService(EventHandlerDictionary eventHandlerDictionary, NuiService nuiService, KeybindService keybindService, LocalizationService localizationService, ResourceService resourceService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._nuiService = nuiService;
            this._keybindService = keybindService;
            this._localizationService = localizationService;
            this._resourceService = resourceService;
            this._menus = new List<Menu>();
        }

        /// <inheritdoc />
        public void CreateMenus()
        {
            this._menus.AddRange(new[]
            {
                new DutyMenu(this._localizationService, this._nuiService, this._resourceService),
            });
        }

        /// <inheritdoc />
        public void SendToNui()
        {
            var menus = new JArray();
            foreach (var menu in this._menus)
            {
                var items = new JArray();
                foreach (var item in menu.Items)
                {
                    var itemData = new JObject
                    {
                        { "Title", item.Title },
                        { "Description", item.Description },
                        { "Hashcode", item.GetHashCode().ToString() },
                        { "Type", (int)item.Type },
                    };

                    if (item.Type == MenuItem.MenuItemType.List)
                    {
                        var casted = item as MenuItemList;
                        itemData.Add("Items", JsonConvert.SerializeObject(casted?.Items));
                    }

                    items.Add(itemData);
                }

                menus.Add(new JObject
                {
                    { "Title", menu.Title },
                    { "UseLocalization", menu.UseLocalization },
                    { "Id", menu.Id },
                    { "Items", items },
                });
            }

            this._nuiService.SendMessage(new JObject
            {
                { "type", Events.NuiEventType.SendMenuObjects },
                { "data", JsonConvert.SerializeObject(menus) },
            });
        }

        /// <inheritdoc />
        public void RegisterMenuKeybinds()
        {
            foreach (var menu in this._menus)
            {
                void OnPress()
                {
                    this._nuiService.Show(Events.NuiEventType.MenuView, menu.Id, false, false);
                }

                this._keybindService.RegisterKeybind($"fivepd_menu_${menu.Id}", "-", menu.Key, OnPress);

                foreach (var item in menu.Items)
                {
                    var eventName = $"fivepd:menu:trigger_item_{item.GetHashCode()}";
                    Cfx.API.RegisterNuiCallbackType(eventName);
                    this._eventHandlerDictionary[$"__cfx_nui:{eventName}"] += new Action<ExpandoObject, CallbackDelegate>((body, callback) => item.OnPress(body, callback));
                }
            }
        }
    }
}