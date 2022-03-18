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
using FivePD.Gamemode.Client.Menu;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class RadialMenuService : IRadialMenuService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly NuiService _nuiService;
        private readonly KeybindService _keybindService;
        private readonly List<RadialMenu> _menus = new List<RadialMenu>();

        /// <summary>Initializes a new instance of the <see cref="RadialMenuService"/> class.</summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="nuiService">The <see cref="NuiService" /> to use.</param>
        /// <param name="keybindService">The <see cref="KeybindService" /> to use.</param>
        /// >
        public RadialMenuService(EventHandlerDictionary eventHandlerDictionary, NuiService nuiService, KeybindService keybindService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._nuiService = nuiService;
            this._keybindService = keybindService;

            Cfx.API.RegisterNuiCallbackType(Events.Nui.GetRadialMenus);
            this._eventHandlerDictionary[$"__cfx_nui:{Events.Nui.GetRadialMenus}"] += new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                var radialMenus = new JArray();
                foreach (var menu in this._menus)
                {
                    var items = new JArray();
                    foreach (var item in menu.Items)
                    {
                        items.Add(new JObject
                        {
                            { "Title", item.Title },
                            { "Hashcode", item.GetHashCode().ToString() },
                        });
                    }

                    radialMenus.Add(new JObject
                    {
                        { "Title", menu.Title },
                        { "Hashcode", menu.GetHashCode().ToString() },
                        { "Items", items },
                    });
                }

                cb.Invoke(JsonConvert.SerializeObject(radialMenus));
            });
        }

        /// <inheritdoc />
        public void RegisterMenus()
        {
            var menu = new RadialMenu("Test radial menu", "H", new List<RadialMenuItem>
            {
                new RadialMenuItem("Item 1/1", cb =>
                {
                    Debug.WriteLine("Pressed item 1/1");
                    cb.Invoke(string.Empty);
                }),
                new RadialMenuItem("Item 1/2", cb =>
                {
                    Debug.WriteLine("Pressed item 1/2");
                    cb.Invoke(string.Empty);
                }),
            });
            this._menus.Add(menu);

            var menu2 = new RadialMenu("Test radial menu 2", "J", new List<RadialMenuItem>
            {
                new RadialMenuItem("Item 2/1", cb =>
                {
                    Debug.WriteLine("Pressed item 2/1");
                    cb.Invoke(string.Empty);
                }),
                new RadialMenuItem("Item 2/2", cb =>
                {
                    Debug.WriteLine("Pressed item 2/2");
                    cb.Invoke(string.Empty);
                }),
            });
            this._menus.Add(menu2);
        }

        /// <summary>
        /// Registers all menus' keybind and its items' actions.
        /// </summary>
        public void RegisterMenuKeybinds()
        {
            foreach (var menu in this._menus)
            {
                this._keybindService.RegisterKeybind($"fivepd_radial_menu_${menu.GetHashCode()}", "-", menu.Key, () =>
                {
                    this._nuiService.ShowRadialMenu(menu.GetHashCode().ToString());
                });

                foreach (var item in menu.Items)
                {
                    var eventName = $"fivepd:radialMenu:trigger_item_{item.GetHashCode()}";
                    Cfx.API.RegisterNuiCallbackType(eventName);
                    this._eventHandlerDictionary[$"__cfx_nui:{eventName}"] += new Action<ExpandoObject, CallbackDelegate>((body, callback) => item.OnPress(callback));
                }
            }
        }
    }
}