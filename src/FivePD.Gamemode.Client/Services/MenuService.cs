using System;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Common.DTO;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S3011

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc cref="FivePD.Gamemode.Client.Interfaces.IMenuService" />
public class MenuService : IMenuService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IControlService _controlService;
    private readonly IUIService _uiService;
    private bool _registeredMenus;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="uiService">The <see cref="IUIService" /> to use.</param>
    /// <param name="controlService">The <see cref="IControlService" /> to use.</param>
    public MenuService(
        EventHandlerDictionary eventHandlerDictionary,
        IUIService uiService,
        IControlService controlService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._uiService = uiService;
        this._controlService = controlService;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Menu.GetAll, data =>
        {
            var response = JsonConvert.DeserializeObject<MenuDTO.GetAllMenu>(data);
            this.RegisterMenuKeybinds(response);
        });

        this._eventHandlerDictionary[Events.UI.Menu.UpdateMenuItem] += new Action<string>(data => this._uiService.SendMessage(data));
    }

    private void RegisterMenuKeybinds(MenuDTO.GetAllMenu response)
    {
        if (this._registeredMenus)
        {
            return;
        }

        this._registeredMenus = true;

        foreach (var menu in response.Menus)
        {
            void OnPress()
            {
                this._uiService.Show(Events.NuiEventType.MenuView, menu.Hashcode, false, false);
            }

            this._controlService.RegisterKeybind($"fivepd:openMenu_${menu.Hashcode}", menu.Key, "-", OnPress);

            foreach (var item in menu.Items)
            {
                switch (item.Type)
                {
                    case MenuItemType.List:
                        this._eventHandlerDictionary.AddUIEvent(
                            $"{Events.UI.Menu.OnListSelect}:{item.Hashcode}",
                            new Action<int, CallbackDelegate>(
                                (index, cb) =>
                                {
                                    BaseScript.TriggerServerEvent(Events.UI.Menu.OnListSelect, item.Hashcode, index);
                                    cb.Invoke(string.Empty);
                                }));
                        this._eventHandlerDictionary.AddUIEvent(
                            $"{Events.UI.Menu.OnListIndexChange}:{item.Hashcode}",
                            new Action<int, CallbackDelegate>(
                                (index, cb) =>
                                {
                                    BaseScript.TriggerServerEvent(Events.UI.Menu.OnListIndexChange, item.Hashcode, index);
                                    cb.Invoke(string.Empty);
                                }));
                        break;
                    case MenuItemType.Checkbox:
                        this._eventHandlerDictionary.AddUIEvent(
                            $"{Events.UI.Menu.OnCheckboxSelect}:{item.Hashcode}",
                            new Action<bool, CallbackDelegate>(
                                (@checked, cb) =>
                                {
                                    BaseScript.TriggerServerEvent(Events.UI.Menu.OnCheckboxSelect, item.Hashcode, @checked);
                                    cb.Invoke(string.Empty);
                                }));
                        break;
                    case MenuItemType.Button:
                        this._eventHandlerDictionary.AddUIEvent(
                            $"{Events.UI.Menu.OnButtonSelect}:{item.Hashcode}",
                            new Action<int, CallbackDelegate>(
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