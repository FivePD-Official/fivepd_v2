// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Common.Responses;
using FivePD.Gamemode.Client.Models;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.UIModels;
using FivePD.Gamemode.Server.UIModels.Menus;
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
        private readonly Dictionary<Type, Menu> _menus = new Dictionary<Type, Menu>();

        public MenuService(IServiceProvider serviceProvider, EventHandlerDictionary eventHandlerDictionary)
        {
            this._serviceProvider = serviceProvider;
            this._eventHandlerDictionary = eventHandlerDictionary;
        }

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this.Register<DutyMenu>();
            this._eventHandlerDictionary[Events.UI.Menu.OnItemSelect] += new Action<Player, string>(this.OnItemSelect);
            this._eventHandlerDictionary[Events.UI.Menu.OnListItemSelect] += new Action<Player, string, int>(this.OnListItemSelect);
            this._eventHandlerDictionary[Events.UI.Menu.OnListIndexChange] += new Action<Player, string, int>(this.OnListIndexChange);
        }

        /// <inheritdoc />
        public void Register<T>()
            where T : Menu
        {
            var type = typeof(T);
            var parameters = type.GetConstructors()[0].GetParameters();
            var menu = (Menu)Activator.CreateInstance(
                type,
                parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray());

            if (this._menus.ContainsKey(type))
            {
                throw new ArgumentException($"A menu with the same type has been already registered: {type}");
            }

            if (this._menus.Any(m => m.Value.Key == menu.Key))
            {
                throw new ArgumentException("Can't register menu due to conflicting key");
            }

            if (string.IsNullOrEmpty(menu.Title))
            {
                throw new ArgumentException("Can't register menu, the Title is null or empty");
            }

            this._menus.Add(type, menu);
        }

        [UIEvent(Events.UI.Menu.GetAll)]
        private void UIGetAllMenus(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var menus = new List<GetMenusResponse.UIMenuResponse>();
            foreach (var menu in this._menus.Values)
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

                menus.Add(new GetMenusResponse.UIMenuResponse
                {
                    Hashcode = menu.GetHashCode().ToString(),
                    Key = menu.Key,
                    Title = menu.Title,
                    Items = items,
                    UseLocalization = menu.UseLocalization,
                });
            }

            networkCallbackDelegate.InvokeWithJson(new GetMenusResponse
            {
                Menus = menus,
            });
        }

        private void OnItemSelect([FromSource] Player player, string hashcode)
        {
            this.InvokeItemEvent("RaiseOnItemSelect", player, hashcode, -1);
        }

        private void OnListItemSelect([FromSource] Player player, string hashcode, int index)
        {
            this.InvokeItemEvent("RaiseOnListItemSelect", player, hashcode, index);
        }

        private void OnListIndexChange([FromSource] Player player, string hashcode, int index)
        {
            this.InvokeItemEvent("RaiseOnListIndexChange", player, hashcode, index);
        }

        private void InvokeItemEvent(string methodName, Player player, string hashcode, int index)
        {
            var menuItem = this._menus.Values
                .Select(m => m.Items.FirstOrDefault(x => x.GetHashCode().ToString() == hashcode))
                .FirstOrDefault(found => found != null);

            var methodInfo = this.GetPrivateMethod(menuItem, methodName);
            var fPlayer = FPlayer.FromNetId(player.Handle);

            object[] parameters;
            // Not the best solution, but for now it should fine.
            // We only have two types of events, one with only the player
            // and the other is with the player and the selected index.
            if (methodInfo.GetParameters().Length == 1)
            {
                parameters = new object[] { fPlayer };
            }
            else
            {
                parameters = new object[] { fPlayer, index };
            }

            methodInfo.Invoke(menuItem, parameters);
        }

        private MethodInfo GetPrivateMethod(object objectVariable, string methodName)
        {
#pragma warning disable S3011
            return objectVariable.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore S3011
        }
    }
}