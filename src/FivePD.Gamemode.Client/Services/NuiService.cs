using System;
using System.Dynamic;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Client.Attributes;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc cref="INuiService" />
    public class NuiService : INuiService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly ILoggerService _loggerService;
        private bool _isNuiOpened;
        private bool _ready;

        /// <summary>Initializes a new instance of the <see cref="NuiService"/> class.</summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="loggerService">The <see cref="ILoggerService"/> to use.</param>
        /// >
        public NuiService(EventHandlerDictionary eventHandlerDictionary, ILoggerService loggerService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._loggerService = loggerService;
        }

        /// <summary>
        /// Delegate for event <see cref="OnNuiHide"/>.
        /// </summary>
        public delegate void OnHide();

        /// <inheritdoc />
        public event OnHide OnNuiHide;

        /// <inheritdoc />
        public void RegisterEventHandlers()
        {
            // Listens for NUI close event
            this._eventHandlerDictionary.AddNuiEvent(
                Events.NuiEventType.Close,
                new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
                {
                    this.Hide();
                    cb.Invoke(string.Empty);
                }));

            // Listens for NUI status checks
            this._eventHandlerDictionary.AddNuiEvent(
                Events.NuiEventType.ClientReady,
                new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
                {
                    this._loggerService.Info("Starting FivePD NUI...");
                    this._ready = true;

                    // If we can hear this, we're ready
                    cb.Invoke(true);
                }));

            this.RegisterUIEvents(new[]
            {
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
            });
        }

        /// <inheritdoc />
        [Keybind(KeybindIdentifiers.MenuClose, "BACK", "Close menu")]
        public void Hide()
        {
            this.SendMessage("{\"type\": \"" + Events.NuiEventType.Close + "\"}");
            Cfx.API.SetNuiFocus(false, false);
            this._isNuiOpened = false;
            this.OnNuiHide?.Invoke();
        }

        /// <inheritdoc />
        [Keybind(KeybindIdentifiers.OpenMDT, "B", "Open MDT")]
        public void OpenMdt()
        {
            this.Show(Events.NuiEventType.Mdt);
        }

        /// <inheritdoc />
        [Keybind(KeybindIdentifiers.MenuMoveUp, "UP", "Move up in menu")]
        public void MenuControlUp()
        {
            if (!this._isNuiOpened)
            {
                return;
            }

            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuUp + "\"}");
        }

        /// <inheritdoc />
        [Keybind(KeybindIdentifiers.MenuMoveDown, "DOWN", "Move down in menu")]
        public void MenuControlDown()
        {
            if (!this._isNuiOpened)
            {
                return;
            }

            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuDown + "\"}");
        }

        /// <inheritdoc />
        [Keybind(KeybindIdentifiers.MenuMoveLeft, "LEFT", "Move left in menu")]
        public void MenuControlLeft()
        {
            if (!this._isNuiOpened)
            {
                return;
            }

            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuLeft + "\"}");
        }

        /// <inheritdoc />
        [Keybind(KeybindIdentifiers.MenuMoveRight, "RIGHT", "Move right in menu")]
        public void MenuControlRight()
        {
            if (!this._isNuiOpened)
            {
                return;
            }

            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuRight + "\"}");
        }

        /// <inheritdoc />
        [Keybind(KeybindIdentifiers.MenuItemSelect, "RETURN", "Selects item in menu")]
        public void MenuControlSelect()
        {
            if (!this._isNuiOpened)
            {
                return;
            }

            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuSelect + "\"}");
        }

        /// <inheritdoc />
        public void ShowRadialMenu(string menuId)
        {
            // Centers cursor
            Cfx.API.SetCursorLocation(0.5f, 0.5f);

            this.Show(Events.NuiEventType.RadialMenu, menuId, false, false);
        }

        /// <param name="timeToHoldInMs"></param>
        /// <inheritdoc />
        public void ShowTimerBar(int timeToHoldInMs)
        {
            Cfx.API.SetNuiFocus(false, false);
            this.SendMessage(new JObject
            {
                { "type", Events.NuiEventType.TimerBar },
                { "show", true },
                { "title", "test" },
                { "timeToHold", timeToHoldInMs },
            });
        }

        /// <inheritdoc />
        public void SendMessage(string data)
        {
            Cfx.API.SendNuiMessage(data);
        }

        /// <inheritdoc />
        public void SendMessage(JObject data)
        {
            Cfx.API.SendNuiMessage(JsonConvert.SerializeObject(data));
        }

        /// <inheritdoc />
        public void Show(string type, string menuId = "#", bool hasFocus = true, bool hasCursor = true)
        {
            if (!this._ready)
            {
                return;
            }

            if (this._isNuiOpened && type != Events.NuiEventType.Mdt)
            {
                this.Hide();
                return;
            }

            this._isNuiOpened = true;

            this.SendMessage(new JObject
            {
                { "type", type },
                { "menuId", menuId },
            });
            Cfx.API.SetNuiFocus(hasFocus, hasCursor);
        }

        private void RegisterUIEvents(string[] events)
        {
            foreach (var e in events)
            {
                this._eventHandlerDictionary.AddUIEventServerResponse<ExpandoObject>(e);
            }
        }
    }
}