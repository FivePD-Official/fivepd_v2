using System;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using Newtonsoft.Json;

#pragma warning disable S3011

namespace FivePD.Client.Services;

/// <inheritdoc cref="IMenuService" />
internal class MenuService : IMenuService, IOnInit, IOnServerBootstrapped
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IControlService _controlService;
    private readonly IUIManagerService _uiService;
    private readonly INetworkEventService _networkEventService;

    private bool _serverBootstrapped;
    private bool _uiReady;
    private string _getAllResponse;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="uiService">The <see cref="IUIManagerService" /> to use.</param>
    /// <param name="controlService">The <see cref="IControlService" /> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService" /> to use.</param>
    public MenuService(
        EventHandlerDictionary eventHandlerDictionary,
        IUIManagerService uiService,
        IControlService controlService,
        INetworkEventService networkEventService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._uiService = uiService;
        this._controlService = controlService;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Menu.GetAll, data =>
        {
            this._uiReady = true;
            this._getAllResponse = data;
            this.CheckAndRegisterMenuKeybinds();
        });
        this._eventHandlerDictionary[Events.UI.Menu.UpdateMenuItem] += new Action<string>(data => this._uiService.SendMessage(data));
        this._eventHandlerDictionary[Events.UI.Menu.UpdateMenuListOption] += new Action<string>(data => this._uiService.SendMessage(data));
        this._eventHandlerDictionary[Events.UI.Menu.AddMenuItem] += new Action<string>(this.AddMenuItem);
        this._eventHandlerDictionary[Events.UI.Menu.RemoveMenuItem] += new Action<string>(data => this._uiService.SendMessage(data));
    }

    /// <inheritdoc/>
    public void OnServerBootstrapped()
    {
        this._serverBootstrapped = true;
        this.CheckAndRegisterMenuKeybinds();
    }

    private void CheckAndRegisterMenuKeybinds()
    {
        if (!this._uiReady || !this._serverBootstrapped)
        {
            return;
        }

        var response = JsonConvert.DeserializeObject<MenuDTO.GetAllMenu>(this._getAllResponse);
        this.RegisterMenuKeybinds(response);
    }

    private void AddMenuItem(string serializedData)
    {
        var data = JsonConvert.DeserializeObject<MenuDTO.AddMenuItem>(serializedData);
        this._uiService.SendMessage(serializedData);
        this.RegisterItemActions(data.Item);
    }

    private void RegisterMenuKeybinds(MenuDTO.GetAllMenu response)
    {
        foreach (var menu in response.Menus)
        {
            void OnPress()
            {
                this._networkEventService.TriggerServerEvent(Events.UI.Menu.OnOpen, menu.Id, new Action<bool>(canOpen =>
                {
                    if (canOpen)
                    {
                        this._uiService.ToggleView(Events.UIEvent.MenuView, menu.Id);
                    }
                }));
            }

            this._controlService.RegisterKeybind($"fivepd:openMenu:{menu.Id}", menu.Key, "-", OnPress);

            foreach (var item in menu.Items)
            {
                this.RegisterItemActions(item);
            }
        }
    }

    private void RegisterItemActions(MenuDTO.GetAllMenu.UIMenuResponse.UIMenuItemResponse item)
    {
        switch (item.Type)
        {
            case MenuItemType.List:
                this._eventHandlerDictionary.AddUIEvent(
                    $"{Events.UI.Menu.OnListSelect}:{item.Hashcode}",
                    new Action<int, CallbackDelegate>(
                        (index, cb) =>
                        {
                            this._networkEventService.TriggerServerEvent(Events.UI.Menu.OnListSelect, item.Hashcode, index);
                            cb.Invoke(string.Empty);
                        }));
                this._eventHandlerDictionary.AddUIEvent(
                    $"{Events.UI.Menu.OnListIndexChange}:{item.Hashcode}",
                    new Action<int, CallbackDelegate>(
                        (index, cb) =>
                        {
                            this._networkEventService.TriggerServerEvent(Events.UI.Menu.OnListIndexChange, item.Hashcode, index);
                            cb.Invoke(string.Empty);
                        }));
                break;
            case MenuItemType.Checkbox:
                this._eventHandlerDictionary.AddUIEvent(
                    $"{Events.UI.Menu.OnCheckboxSelect}:{item.Hashcode}",
                    new Action<bool, CallbackDelegate>(
                        (@checked, cb) =>
                        {
                            this._networkEventService.TriggerServerEvent(Events.UI.Menu.OnCheckboxSelect, item.Hashcode, @checked);
                            cb.Invoke(string.Empty);
                        }));
                break;
            case MenuItemType.Button:
                this._eventHandlerDictionary.AddUIEvent(
                    $"{Events.UI.Menu.OnButtonSelect}:{item.Hashcode}",
                    new Action<int, CallbackDelegate>(
                        (_, cb) =>
                        {
                            this._networkEventService.TriggerServerEvent(Events.UI.Menu.OnButtonSelect, item.Hashcode);
                            cb.Invoke(string.Empty);
                        }));
                break;
        }
    }
}