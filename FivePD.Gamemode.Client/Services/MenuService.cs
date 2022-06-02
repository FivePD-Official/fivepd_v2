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
using FivePD.Gamemode.Client.Extensions;
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
        private readonly INuiService _nuiService;
        private readonly IKeybindService _keybindService;
        private readonly ILocalizationService _localizationService;
        private readonly IResourceService _resourceService;
        private readonly IEntityService _entityService;
        private readonly List<Menu> _menus;
        private bool _isLocalizationReady;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="nuiService">The <see cref="INuiService" /> to use.</param>
        /// <param name="keybindService">The <see cref="IKeybindService" /> to use.</param>
        /// <param name="localizationService">The <see cref="ILocalizationService" /> to use.</param>
        /// <param name="resourceService">The <see cref="IResourceService"/> to use.</param>
        /// <param name="entityService">The <see cref="IEntityService"/> to use.</param>
        public MenuService(EventHandlerDictionary eventHandlerDictionary, INuiService nuiService, IKeybindService keybindService, ILocalizationService localizationService, IResourceService resourceService, IEntityService entityService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._nuiService = nuiService;
            this._keybindService = keybindService;
            this._localizationService = localizationService;
            this._resourceService = resourceService;
            this._entityService = entityService;
            this._menus = new List<Menu>();
        }

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this._isLocalizationReady = true;
            this._eventHandlerDictionary.AddNuiEvent(Events.NuiEventType.IsNuiReady, new Action<ExpandoObject, CallbackDelegate>(async (body, cb) =>
            {
                while (!this._isLocalizationReady)
                {
                    await BaseScript.Delay(10);
                }

                this.SendToNui();
                cb.Invoke(string.Empty);
            }));
        }

        /// <inheritdoc />
        public void CreateMenus()
        {
            this._menus.AddRange(new[]
            {
                new DutyMenu(this._localizationService, this._nuiService, this._resourceService, this._entityService),
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
                        var casted = item as MenuListItem;
                        itemData.Add("Items", JArray.FromObject(casted?.Items));
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
                { "data", menus },
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

                this._keybindService.RegisterKeybind($"fivepd:openMenu_${menu.Id}", "-", menu.Key, OnPress);

                foreach (var item in menu.Items)
                {
                    var eventName = $"fivepd:menu:trigger_item_{item.GetHashCode()}";
                    this._eventHandlerDictionary.AddNuiEvent(eventName, new Action<int, CallbackDelegate>(
                        (body, cb) =>
                        {
                            item.OnPress(body);
                            cb.Invoke(string.Empty);
                        }));
                }
            }
        }
    }
}