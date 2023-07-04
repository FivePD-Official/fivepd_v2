using System;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.Attributes;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Models;
using FivePD.Client.Utilities;
using FivePD.Gamemode.Client.DTO;
using FivePD.Gamemode.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="IUIService" />
internal class UIService : IUIService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IConfigService _configService;
    private readonly IControlService _controlService;
    private readonly IUIManagerService _uiManagerService;

    /// <summary>Initializes a new instance of the <see cref="UIService"/> class.</summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    /// <param name="uiManagerService">The <see cref="IUIManagerService"/> to use.</param>
    public UIService(
        EventHandlerDictionary eventHandlerDictionary,
        IConfigService configService,
        IControlService controlService,
        IUIManagerService uiManagerService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
        this._controlService = controlService;
        this._uiManagerService = uiManagerService;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._controlService.RegisterKeybind(Controls.OpenMDT.Id, Controls.OpenMDT.Key, Controls.OpenMDT.Title, this.OpenMDT);
        this._controlService.RegisterKeybind(Controls.OpenInformation.Id, Controls.OpenInformation.Key, Controls.OpenInformation.Title, this.OpenInformation);

        // Listens for NUI close event
        this._eventHandlerDictionary.AddUIEvent(
            Events.NuiEventType.Close,
            new Action<ExpandoObject, CallbackDelegate>((_, cb) =>
            {
                this._uiManagerService.Close();
                cb.Invoke(string.Empty);
            }));

        this._eventHandlerDictionary.AddUIEvent(
            Events.DevTools.GetCurrentPlayerCoordinate,
            new Action<ExpandoObject, CallbackDelegate>((body, cb) => { cb.Invoke(JsonConvert.SerializeObject(Game.PlayerPed.Position)); }));

        this._eventHandlerDictionary.AddUIEvent(
            Events.UI.Config.GetGeneralSettings,
            new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                async void RunAsync()
                {
                    cb.Invoke(JsonConvert.SerializeObject(await this._configService.GeneralConfig));
                }

                RunAsync();
            }));

        this._eventHandlerDictionary.AddUIEvent(
            Events.UI.Config.GetSafeZoneSize,
            new Action<ExpandoObject, CallbackDelegate>((_, cb) =>
            {
                cb.Invoke(JsonConvert.SerializeObject(new JObject
                {
                    ["SafeZoneSize"] = Cfx.API.GetSafeZoneSize(),
                }));
            }));

        this._eventHandlerDictionary.AddUIToServerEventOverrideData(
            Events.Callout.GetAllActiveUI,
            serverResponse =>
            {
                var callouts = JsonConvert.DeserializeObject<Page<CalloutDTO.CalloutToClientDTO>>(serverResponse);
                var newCallouts = callouts.Items.Select(callout => new CalloutDTO.CalloutToUIDTO
                    {
                        Title = callout.Title,
                        Description = callout.Description,
                        Units = callout.Units,
                        Address = callout.Location.GetStreetName(),
                    })
                    .ToList();
                var newPage = new Page<CalloutDTO.CalloutToUIDTO>
                {
                    Items = newCallouts,
                    CurrentPage = 1,
                    ItemCount = newCallouts.Count,
                    PageCount = 1,
                    PageSize = newCallouts.Count,
                };

                return JsonConvert.SerializeObject(newPage);
            });

        this.RegisterUIEvents(new[]
        {
            Events.DevTools.DeleteEntities,
            Events.UI.Department.List,
            Events.UI.Department.List,
            Events.UI.Department.GetById,
            Events.UI.Department.GetUsers,
            Events.UI.Department.GetRank,
            Events.UI.Department.GetRanks,
            Events.UI.Department.GetDivision,
            Events.UI.Department.GetDivisions,
            Events.UI.Department.DeleteDivision,
            Events.UI.Department.EditDivision,
            Events.UI.Department.CreateDivision,
            Events.UI.Department.DeleteRank,
            Events.UI.Department.EditRank,
            Events.UI.Department.CreateRank,
            Events.UI.Department.Join,
            Events.UI.Department.Leave,
            Events.UI.Department.Edit,
            Events.UI.Department.Delete,
            Events.UI.Department.Create,
            Events.UI.Department.CheckIfDepartmentNameTaken,
            Events.UI.Player.GetDetails,
            Events.UI.Player.ChangeDivision,
            Events.UI.Player.ChangeRank,
            Events.UI.Player.GetAllPageable,
            Events.UI.Player.ChangeRole,
            Events.UI.LooseData.GetEntity,
            Events.UI.LooseData.GetEntities,
            Events.UI.LooseData.AddEntity,
            Events.UI.LooseData.EditEntity,
            Events.UI.LooseData.DeleteEntity,
            Events.UI.LooseData.GetSetAttribute,
            Events.UI.LooseData.GetSetAttributes,
            Events.UI.LooseData.GetSetAttributesPageable,
            Events.UI.LooseData.AddSetAttribute,
            Events.UI.LooseData.EditSetAttribute,
            Events.UI.LooseData.DeleteSetAttribute,
            Events.UI.Unit.GetAll,
            Events.UI.Unit.Join,
            Events.UI.Unit.RequestToJoin,
            Events.UI.Unit.Leave,
            Events.UI.Callout.GetAllTypes,
            Events.UI.Callout.Dispatch,
        });
    }

    /// <inheritdoc />
    public void OpenMDT()
    {
        this._uiManagerService.Show(Events.NuiEventType.Mdt);
    }

    /// <inheritdoc />
    public void OpenInformation()
    {
        this._uiManagerService.Show(Events.NuiEventType.Information, string.Empty);
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuMoveUp, "UP", "Move up in menu")]
    public void MenuControlUp()
    {
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuUp + "\"}");
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuMoveDown, "DOWN", "Move down in menu")]
    public void MenuControlDown()
    {
        Debug.WriteLine($"{this._uiManagerService.MenuOpen}");
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuDown + "\"}");
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuMoveLeft, "LEFT", "Move left in menu")]
    public void MenuControlLeft()
    {
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuLeft + "\"}");
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuMoveRight, "RIGHT", "Move right in menu")]
    public void MenuControlRight()
    {
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuRight + "\"}");
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuItemSelect, "RETURN", "Selects item in menu")]
    public void MenuControlSelect()
    {
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuSelect + "\"}");
    }

    /// <inheritdoc />
    public void ShowRadialMenu(string menuId)
    {
        // Centers cursor
        Cfx.API.SetCursorLocation(0.5f, 0.5f);

        this._uiManagerService.Show(Events.NuiEventType.RadialMenu, menuId);
    }

    private void RegisterUIEvents(string[] events)
    {
        foreach (var e in events)
        {
            this._eventHandlerDictionary.AddUIToServerEvent(e);
        }
    }
}