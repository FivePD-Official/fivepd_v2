// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Common.Responses;
using FivePD.Common.UIModels;
using FivePD.Common.UIModels.Interfaces;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.UIModels;
using Newtonsoft.Json.Linq;

#pragma warning disable CS1591
#pragma warning disable SA1600

#pragma warning disable S125
#pragma warning disable SA1515

namespace FivePD.Gamemode.Server.Services
{
    public class MenuService : IMenuService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, string> _menus = new Dictionary<Type, string>(); // Menu type, menu key

        public MenuService(IServiceProvider serviceProvider, EventHandlerDictionary eventHandlerDictionary)
        {
            this._serviceProvider = serviceProvider;
            this._eventHandlerDictionary = eventHandlerDictionary;
        }

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this.Register<DutyMenu>();
            this._eventHandlerDictionary[Events.UI.Menu.OnButtonSelect] += new Action<Player, string>(this.OnButtonSelect);
            this._eventHandlerDictionary[Events.UI.Menu.OnCheckboxSelect] += new Action<Player, string, bool>(this.OnCheckboxSelect);
            this._eventHandlerDictionary[Events.UI.Menu.OnListSelect] += new Action<Player, string, int>(this.OnListSelect);
            this._eventHandlerDictionary[Events.UI.Menu.OnListIndexChange] += new Action<Player, string, int>(this.OnListIndexChange);
        }

        /// <inheritdoc />
        public void Register<T>()
            where T : Menu
        {
            var type = typeof(T);
            var menu = this.CreateMenuInstance<T>();

            if (this._menus.ContainsKey(type))
            {
                throw new ArgumentException($"A menu with the same type has been already registered: {type}");
            }

            if (this._menus.Any(m => m.Value == menu.Key))
            {
                throw new ArgumentException("Can't register menu due to conflicting key");
            }

            if (string.IsNullOrEmpty(menu.Title))
            {
                throw new ArgumentException("Can't register menu, the Title is null or empty");
            }

            this._menus.Add(type, menu.Key);
        }

        private T CreateMenuInstance<T>(Type type = null)
            where T : Menu
        {
            type ??= typeof(T);
            var parameters = type.GetConstructors()[0].GetParameters();
            var menu = (Menu)Activator.CreateInstance(
                type,
                parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray());
            return (T)menu;
        }

        [UIEvent(Events.UI.Menu.GetAll, true)]
        private void UIGetMenus([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var menus = this._menus.Keys.Select(this.CreateMenuInstance<Menu>).ToList();

            var response = new GetMenusResponse
            {
                Menus = new List<GetMenusResponse.UIMenuResponse>(),
            };
            foreach (var menu in menus)
            {
                var items = new List<GetMenusResponse.UIMenuResponse.UIMenuItemResponse>();
                foreach (var item in menu.Items)
                {
                    var listItemItems = new JArray();
                    if (item.Type == MenuItemType.List)
                    {
                        var casted = item as MenuListItem;
                        listItemItems = JArray.FromObject(casted!.Items);
                    }

                    items.Add(new GetMenusResponse.UIMenuResponse.UIMenuItemResponse
                    {
                        Hashcode = item.GetHashCode().ToString(),
                        Title = item.Title,
                        Description = item.Description,
                        Type = item.Type,
                        Items = listItemItems,
                    });
                }

                response.Menus.Add(new GetMenusResponse.UIMenuResponse
                {
                    Hashcode = menu.GetHashCode().ToString(),
                    Key = menu.Key,
                    Title = menu.Title,
                    Items = items,
                    UseLocalization = menu.UseLocalization,
                });
            }

            var fPlayer = (IFPlayerWithMenus)FPlayer.FromNetId(player.Handle);
            fPlayer.SetMenus(menus);

            networkCallbackDelegate.InvokeWithJson(response);
        }

        private void OnButtonSelect([FromSource] Player player, string hashcode)
        {
            var fPlayer = FPlayer.FromNetId(player.Handle);
            var menuItem = ((IFPlayerWithMenus)fPlayer).FindMenuItem(hashcode);
            if (menuItem is null)
            {
                return;
            }

            var casted = (IMenuButtonItemWithEvents)menuItem;
            casted.RaiseOnSelect(fPlayer);
        }

        private void OnCheckboxSelect([FromSource] Player player, string hashcode, bool @checked)
        {
            var fPlayer = FPlayer.FromNetId(player.Handle);
            var menuItem = ((IFPlayerWithMenus)fPlayer).FindMenuItem(hashcode);
            if (menuItem is null)
            {
                return;
            }

            var casted = (IMenuCheckboxItemWithEvents)menuItem;
            casted.RaiseOnSelect(fPlayer, @checked);
        }

        private void OnListSelect([FromSource] Player player, string hashcode, int index)
        {
            var fPlayer = FPlayer.FromNetId(player.Handle);
            var menuItem = ((IFPlayerWithMenus)fPlayer).FindMenuItem(hashcode);
            if (menuItem is null)
            {
                return;
            }

            var casted = (IMenuListItemWithEvents)(menuItem as MenuListItem);
            casted?.RaiseOnSelect(fPlayer, index);
        }

        private void OnListIndexChange([FromSource] Player player, string hashcode, int index)
        {
            var fPlayer = FPlayer.FromNetId(player.Handle);
            var menuItem = ((IFPlayerWithMenus)fPlayer).FindMenuItem(hashcode);
            if (menuItem is null)
            {
                return;
            }

            var casted = (IMenuListItemWithEvents)(menuItem as MenuListItem);
            casted?.RaiseOnIndexChange(fPlayer, index);
        }

        private MenuItem FindMenuItem(string hashcode)
        {
            return new MenuButtonItem("asd", hashcode);
        }
    }
}