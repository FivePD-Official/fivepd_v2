// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Common.Responses;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S3011

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class MenuService : IMenuService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IUIService _uiService;
        private readonly IKeybindService _keybindService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="uiService">The <see cref="IUIService" /> to use.</param>
        /// <param name="keybindService">The <see cref="IKeybindService" /> to use.</param>
        public MenuService(
            EventHandlerDictionary eventHandlerDictionary,
            IUIService uiService,
            IKeybindService keybindService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._uiService = uiService;
            this._keybindService = keybindService;
        }

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Menu.GetAll, data =>
            {
                var response = JsonConvert.DeserializeObject<GetMenusResponse>(data);
                this.RegisterMenuKeybinds(response);
            });

            this._eventHandlerDictionary[Events.UI.Menu.UpdateMenuItem] += new Action<string>(data => this._uiService.SendMessage(data));
        }

        private void RegisterMenuKeybinds(GetMenusResponse response)
        {
            foreach (var menu in response.Menus)
            {
                void OnPress()
                {
                    this._uiService.Show(Events.NuiEventType.MenuView, menu.Hashcode, false, false);
                }

                this._keybindService.RegisterKeybind($"fivepd:openMenu:${menu.Hashcode}", menu.Title, menu.Key, OnPress);

                foreach (var item in menu.Items)
                {
                    switch (item.Type)
                    {
                        case MenuItemType.List:
                            this._eventHandlerDictionary.AddUIEvent($"{Events.UI.Menu.OnListSelect}:{item.Hashcode}", new Action<int, CallbackDelegate>(
                                (index, cb) =>
                                {
                                    BaseScript.TriggerServerEvent(Events.UI.Menu.OnListSelect, item.Hashcode, index);
                                    cb.Invoke(string.Empty);
                                }));

                            this._eventHandlerDictionary.AddUIEvent($"{Events.UI.Menu.OnListIndexChange}:{item.Hashcode}", new Action<int, CallbackDelegate>(
                                (index, cb) =>
                                {
                                    BaseScript.TriggerServerEvent(Events.UI.Menu.OnListIndexChange, item.Hashcode, index);
                                    cb.Invoke(string.Empty);
                                }));
                            break;
                        case MenuItemType.Checkbox:
                            this._eventHandlerDictionary.AddUIEvent($"{Events.UI.Menu.OnCheckboxSelect}:{item.Hashcode}", new Action<bool, CallbackDelegate>(
                                (@checked, cb) =>
                                {
                                    BaseScript.TriggerServerEvent(Events.UI.Menu.OnCheckboxSelect, item.Hashcode, @checked);
                                    cb.Invoke(string.Empty);
                                }));
                            break;
                        case MenuItemType.Button:
                            this._eventHandlerDictionary.AddUIEvent($"{Events.UI.Menu.OnButtonSelect}:{item.Hashcode}", new Action<int, CallbackDelegate>(
                                (_, cb) =>
                                {
                                    BaseScript.TriggerServerEvent(Events.UI.Menu.OnButtonSelect, item.Hashcode);
                                    cb.Invoke(string.Empty);
                                }));
                            break;
                    }
                }
            }
        }
    }
}