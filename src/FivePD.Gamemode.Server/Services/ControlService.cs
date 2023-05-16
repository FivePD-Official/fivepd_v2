using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Common.Utilities;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;

#pragma warning disable

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IControlService" />
    public class ControlService : IControlService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly List<Control> _controls = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlService"/> class.
        /// </summary>
        public ControlService(EventHandlerDictionary eventHandlerDictionary)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
        }
        
        /// <inheritdoc />
        public void RegisterEvents()
        {
            this.RegisterKeybind("test", "test", "J", () => Debug.WriteLine("test"));
            this._eventHandlerDictionary[Events.Control.GetAll] += new Action<NetworkCallbackDelegate>(this.OnGetAll);
        }

        /// <inheritdoc />
        public void RegisterKeybind(string id, string description, string key, Action onPress = null, Action onRelease = null)
        {
            this.RegisterControl(id, description, key, "KEYBOARD", onPress, onRelease);
        }

        /// <inheritdoc />
        public void RegisterMomentaryKeybind(string id, string description, string key, int timeToHoldInMs = 0, Action onPress = null, Action onRelease = null)
        {
            this.RegisterControl(id, description, key, "KEYBOARD", onPress, onRelease, timeToHoldInMs);
        }

        /// <inheritdoc />
        public void RegisterMouseKeybind(string id, string description, string key, Action onPress = null, Action onRelease = null)
        {
            this.RegisterControl(id, description, key, "MOUSE_BUTTON", onPress, onRelease);
        }

        private void RegisterControl(string id, string description, string key, string keyMapper, Action onPress = null, Action onRelease = null, int timeToHoldInMs = 0)
        {
            this._controls.Add(new Control
            {
                Id = id,
                Description = description,
                Key = key,
                KeyMapper = keyMapper,
                OnPress = onPress,
                OnRelease = onRelease,
                TimeToHoldInMs = timeToHoldInMs,
            });
            this._eventHandlerDictionary[Events.Control.Trigger] += new Action<string, string>((triggeredId, type) =>
            {
                var control = this._controls.FirstOrDefault(x => x.Id == triggeredId);
                if (type == "press")
                {
                    control?.OnPress?.Invoke();
                }
                else
                {
                    control?.OnRelease?.Invoke();
                }
            });
        }

        private void OnGetAll(NetworkCallbackDelegate networkCallbackDelegate)
        {
            networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(this._controls.Select(x => x.ToClientResponse())));
        }
    }
}