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
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.UIModels;
using FivePD.Server.UIModels.Interfaces;
using FivePD.Server.UIModels.MenuModels;
using Newtonsoft.Json.Linq;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Server.Interfaces.IMenuService" />
[Obfuscate]
public class MenuService : IMenuService, IOnInit
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
    void IOnInit.OnInit()
    {
        this.Register<VehicleSelectorMenu>();
        this.Register<LoadoutSelectorMenu>();
        this.Register<DispatchMenu>();
        this.Register<AssistanceMenu>();
        this.Register<InteractionMenu>();

        this._eventHandlerDictionary[Events.UI.Menu.OnOpen] += new Action<Player, string, NetworkCallbackDelegate>(this.OnMenuOpen);
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
    public void UpdateMenuItem<TMenu, TProperty>(
        FPlayer player,
        Expression<Func<TMenu, MenuItem>> item,
        Expression<Func<MenuItem, TProperty>> property,
        TProperty value)
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
            { "Hashcode", propertyObj.GetHashCode().ToString() },
            { "Property", propertyMember.Name },
            { "Value", JToken.FromObject(value) },
        });
    }

    /// <inheritdoc />
    public void UpdateMenuListOption<TMenu, TProperty>(
        FPlayer player,
        Expression<Func<TMenu, MenuListItem>> item,
        int index,
        Expression<Func<MenuListOption, TProperty>> property,
        TProperty value)
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
        if (actualMenuItemMember is null)
        {
            return;
        }

        var listItemValue = this.GetValueOfProperty(actualMenuItemMember, menu) as MenuListItem;
        if (listItemValue is null)
        {
            return;
        }

        var actualPropertyMember = this.GetMember(typeof(MenuListOption), propertyMember.Name);

        this.SetValueOfProperty(actualPropertyMember, listItemValue.Options[index], value);
        player.TriggerEvent(Events.UI.Menu.UpdateMenuListOption, new JObject
        {
            { "Hashcode", listItemValue.GetHashCode().ToString() },
            { "Index", index },
            { "Property", propertyMember.Name },
            { "Value", JToken.FromObject(value) },
        });
    }

    /// <inheritdoc />
    public string AddMenuItem<TMenu>(
        FPlayer player,
        MenuItem item)
        where TMenu : Menu
    {
        if (item is null)
        {
            return string.Empty;
        }

        var menu = player.GetMenu<Menu>(typeof(TMenu));
        if (menu is null)
        {
            return string.Empty;
        }

        menu.InternalItems.Add(item);

        var listItemItems = new JArray();
        if (item.Type == MenuItemType.List)
        {
            var casted = item as MenuListItem;
            listItemItems = JArray.FromObject(casted!.Options);
        }

        var menuItemDto = new MenuDTO.GetAllMenu.UIMenuResponse.UIMenuItemResponse
        {
            Hashcode = item.GetHashCode().ToString(),
            Title = item.Title,
            Description = item.Description,
            Type = item.Type,
            Items = listItemItems,
        };

        player.TriggerEvent(Events.UI.Menu.AddMenuItem, new JObject
        {
            { "MenuId", menu.Id },
            { "Item", JToken.FromObject(menuItemDto) }
        });

        return menuItemDto.Hashcode;
    }

    /// <inheritdoc />
    public void RemoveMenuItem<TMenu>(
        FPlayer player,
        MenuItem item)
        where TMenu : Menu
    {
        if (item is null)
        {
            return;
        }

        var menu = player.GetMenu<Menu>(typeof(TMenu));
        if (menu is null)
        {
            return;
        }

        menu.InternalItems.Remove(item);

        player.TriggerEvent(Events.UI.Menu.RemoveMenuItem, new JObject
        {
            { "MenuId", menu.Id },
            { "ItemHashcode", item.GetHashCode().ToString() },
        });
    }

    private MemberInfo GetMember(Type type, string memberName)
    {
#pragma warning disable S3011
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            .Cast<MemberInfo>()
            .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            .FirstOrDefault(x => x.Name == memberName);
#pragma warning restore S3011
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
        var instance = (T)Activator.CreateInstance(
            type,
            parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray());
        instance.CreateAndSetItems();
        return instance;
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
            foreach (var item in menu.InternalItems)
            {
                var listItemItems = new JArray();
                if (item.Type == MenuItemType.List)
                {
                    var casted = item as MenuListItem;
                    listItemItems = JArray.FromObject(casted!.Options);
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
            });
        }

        var fPlayer = (IFPlayerWithMenus)FPlayer.FromNetId(player.Handle);
        fPlayer.SetMenus(menus);
        networkCallbackDelegate.InvokeWithJson(response);
    }

    private void OnMenuOpen([FromSource] Player player, string menuId, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var fPlayer = FPlayer.FromNetId(player.Handle);
        if (fPlayer is null)
        {
            networkCallbackDelegate.Invoke(false);
            return;
        }

        var menu = ((IFPlayerWithMenus)fPlayer).FindMenu(menuId);
        if (menu is null)
        {
            networkCallbackDelegate.Invoke(false);
            return;
        }

        var canOpen = menu.CanOpen(FPlayer.FromPlayer(player));
        networkCallbackDelegate.Invoke(canOpen);
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