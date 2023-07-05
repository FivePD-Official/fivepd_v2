using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.UIModels;
using FivePD.Server.UIModels.Interfaces;
using FivePD.Server.UIModels.MenuModels;
using Newtonsoft.Json.Linq;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IMenuService" />
public class MenuService : IMenuService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    private readonly Dictionary<Type, string> _menus = new(); // Menu type, menu key

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public MenuService(IServiceProvider serviceProvider, EventHandlerDictionary eventHandlerDictionary, ILogger logger)
    {
        this._serviceProvider = serviceProvider;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._logger = logger;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this.Register<DutyMenu>();
        this.Register<DispatchMenu>();

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
            this._logger.Error("Can not register menu with type \"{Type}\". A menu with the same type has been already registered.", type);
            return;
        }

        if (this._menus.Any(m => m.Value == menu.Key))
        {
            this._logger.Error("Can not register menu with type \"{Type}\". The key conflicts with an already registered menu's key.", type);
            return;
        }

        if (string.IsNullOrEmpty(menu.Title))
        {
            this._logger.Error("Can not register menu with type \"{Type}\". The Title is null or empty.", type);
            return;
        }

        this._menus.Add(type, menu.Key);
    }

    /// <inheritdoc />
    public void UpdateMenuItem<TMenu>(FPlayer player, Expression<Func<TMenu, MenuItem>> item, Expression<Func<MenuItem, object>> property, object value)
        where TMenu : Menu
    {
        if (value is null)
        {
            return;
        }

        var menuItemMember = (item.Body as MemberExpression)?.Member;
        var propertyMember = (property.Body as MemberExpression)?.Member;
        if (menuItemMember is null || propertyMember is null)
        {
            return;
        }

        var menu = player.GetMenu<Menu>(typeof(TMenu));
        var actualMenuItemMember = this.GetMember(typeof(TMenu), menuItemMember.Name);
        var actualPropertyMember = this.GetMember(typeof(MenuItem), propertyMember.Name);
        if (actualMenuItemMember is null || actualPropertyMember is null)
        {
            return;
        }

        var propertyObj = this.GetValueOfProperty(actualMenuItemMember, menu);
        if (propertyObj is null)
        {
            return;
        }

        this.SetValueOfProperty(actualPropertyMember, propertyObj, value);
        player.TriggerEvent(Events.UI.Menu.UpdateMenuItem, new JObject
        {
            { "hashcode", propertyObj.GetHashCode().ToString() },
            { "property", propertyMember.Name },
            { "value", JToken.FromObject(value) },
        });
    }

    private MemberInfo GetMember(Type type, string memberName)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Cast<MemberInfo>()
            .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .FirstOrDefault(x => x.Name == memberName);
    }

    private object GetValueOfProperty(MemberInfo member, object target)
    {
        return member.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)member).GetValue(target),
            MemberTypes.Property => ((PropertyInfo)member).GetValue(target),
            _ => null,
        };
    }

    private void SetValueOfProperty(MemberInfo member, object target, object value)
    {
        try
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)member).SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)member).SetValue(target, value);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error while updating menu item value: {e}");
        }
    }

    private T CreateMenuInstance<T>(Type type = null)
        where T : Menu
    {
        type ??= typeof(T);
        var parameters = type.GetConstructors()[0].GetParameters();
        return (T)Activator.CreateInstance(
            type,
            parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray());
    }

    [UIEvent(Events.UI.Menu.GetAll, true)]
    private void UIGetMenus([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var menus = this._menus.Keys.Select(this.CreateMenuInstance<Menu>).ToList();

        var response = new MenuDTO.GetAllMenu
        {
            Menus = new List<MenuDTO.GetAllMenu.UIMenuResponse>(),
        };
        foreach (var menu in menus)
        {
            var items = new List<MenuDTO.GetAllMenu.UIMenuResponse.UIMenuItemResponse>();
            foreach (var item in menu.Items)
            {
                var listItemItems = new JArray();
                if (item.Type == MenuItemType.List)
                {
                    var casted = item as MenuListItem;
                    listItemItems = JArray.FromObject(casted!.Items);
                }

                items.Add(new MenuDTO.GetAllMenu.UIMenuResponse.UIMenuItemResponse
                {
                    Hashcode = item.GetHashCode().ToString(),
                    Title = item.Title,
                    Description = item.Description,
                    Type = item.Type,
                    Items = listItemItems,
                });
            }

            response.Menus.Add(new MenuDTO.GetAllMenu.UIMenuResponse
            {
                Id = menu.Id,
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
}