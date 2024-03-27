using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Attributes;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Client.Models;
using FivePD.Client.Utilities;
using FivePD.Common;
using FivePD.Common.ConfigModels;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="IUIService" />
internal class UIService : IUIService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IConfigService _configService;
    private readonly IControlService _controlService;
    private readonly IUIManagerService _uiManagerService;
    private readonly ILocationService _locationService;
    private readonly ILocalPlayerService _localPlayerService;

    private GeneralConfig _generalConfig = new()
    {
        Mdt = new MdtConfig
        {
            Access = "EVERYWHERE",
        },
    };

    /// <summary>Initializes a new instance of the <see cref="UIService"/> class.</summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    /// <param name="uiManagerService">The <see cref="IUIManagerService"/> to use.</param>
    /// <param name="locationService">The <see cref="ILocationService"/> to use.</param>
    /// <param name="localPlayerService">The <see cref="ILocalPlayerService"/> to use.</param>
    public UIService(
        EventHandlerDictionary eventHandlerDictionary,
        IConfigService configService,
        IControlService controlService,
        IUIManagerService uiManagerService,
        ILocationService locationService,
        ILocalPlayerService localPlayerService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
        this._controlService = controlService;
        this._uiManagerService = uiManagerService;
        this._locationService = locationService;
        this._localPlayerService = localPlayerService;

#pragma warning disable CS4014
        this.GetGeneralConfigAsync();
#pragma warning restore CS4014
    }

    /// <inheritdoc />
    public void OnInit()
    {
        this._controlService.RegisterKeybind(Controls.OpenMDT.Id, Controls.OpenMDT.Key, Controls.OpenMDT.Title, this.OpenMDT);
        this._controlService.RegisterKeybind(Controls.OpenInformation.Id, Controls.OpenInformation.Key, Controls.OpenInformation.Title, this.OpenInformation);

        // Listens for NUI close event
        this._eventHandlerDictionary.AddUIEvent(
            Events.UIEvent.Close,
            new Action<ExpandoObject, CallbackDelegate>((_, cb) =>
            {
                this._uiManagerService.Close();
                cb.Invoke(string.Empty);
            }));

        this._eventHandlerDictionary.AddUIEvent(
            Events.UI.Config.GetGeneralConfig,
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
    }

    /// <inheritdoc />
    public void OpenMDT()
    {
        if (this._generalConfig.Mdt.AccessEverywhere() || this._localPlayerService.PlayerHasRole(UserRole.Admin))
        {
            this._uiManagerService.ToggleView(Events.UIEvent.Mdt);
            return;
        }

        var playerIsInVehicle = Game.PlayerPed.IsInVehicle();
        if (this._generalConfig.Mdt.AccessOnlyVehicle() && playerIsInVehicle)
        {
            this._uiManagerService.ToggleView(Events.UIEvent.Mdt);
            return;
        }

        var isInRangeOfMarkedLocation = this._locationService.Locations
            .Where(x => x.Type == LocationType.MdtAccess)
            .Any(x => Game.PlayerPed.IsInRangeOf(x.Position, 3f));
        if (this._generalConfig.Mdt.AccessOnlySpecificLocations() && isInRangeOfMarkedLocation)
        {
            this._uiManagerService.ToggleView(Events.UIEvent.Mdt);
            return;
        }

        if (this._generalConfig.Mdt.AccessOnlyVehicleAndSpecificLocations() && (playerIsInVehicle || isInRangeOfMarkedLocation))
        {
            this._uiManagerService.ToggleView(Events.UIEvent.Mdt);
        }
    }

    /// <inheritdoc />
    public void OpenInformation()
    {
        this._uiManagerService.ToggleView(Events.UIEvent.Information, string.Empty);
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuMoveUp, "UP", "Move up in menu")]
    public void MenuControlUp()
    {
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"Event\": \"" + Events.UIEvent.MenuUp + "\"}");
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuMoveDown, "DOWN", "Move down in menu")]
    public void MenuControlDown()
    {
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"Event\": \"" + Events.UIEvent.MenuDown + "\"}");
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuMoveLeft, "LEFT", "Move left in menu")]
    public void MenuControlLeft()
    {
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"Event\": \"" + Events.UIEvent.MenuLeft + "\"}");
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuMoveRight, "RIGHT", "Move right in menu")]
    public void MenuControlRight()
    {
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"Event\": \"" + Events.UIEvent.MenuRight + "\"}");
    }

    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuItemSelect, "RETURN", "Selects item in menu")]
    public void MenuControlSelect()
    {
        if (!this._uiManagerService.MenuOpen)
        {
            return;
        }

        this._uiManagerService.SendMessage("{\"Event\": \"" + Events.UIEvent.MenuSelect + "\"}");
    }

    /// <inheritdoc />
    public void ShowRadialMenu(string menuId)
    {
        // Centers cursor
        Cfx.API.SetCursorLocation(0.5f, 0.5f);

        this._uiManagerService.ToggleView(Events.UIEvent.RadialMenu, menuId);
    }

#pragma warning disable S3241
    private async Task GetGeneralConfigAsync()
#pragma warning restore S3241
    {
        this._generalConfig = await this._configService.GeneralConfig;
    }
}