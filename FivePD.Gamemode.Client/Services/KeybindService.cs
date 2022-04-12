// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Attributes;
using FivePD.Gamemode.Client.Injection;
using FivePD.Gamemode.Client.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class KeybindService : IKeybindService
    {
        private readonly ITickService _tickService;
        private readonly List<HoldableKeybind> _actions = new List<HoldableKeybind>();

        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindService"/> class.
        /// </summary>
        /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
        public KeybindService(ITickService tickService)
        {
            this._tickService = tickService;
        }

        /// <inheritdoc />
        public void Register(ServiceProvider provider)
        {
            var methodInfos = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .ToList();

            var normalKeybinds = methodInfos
                .Where(x => x.GetCustomAttributes(typeof(KeybindAttribute), false).FirstOrDefault() != null)
                .GroupBy(x => x.GetCustomAttribute<KeybindAttribute>().Key);
            var holdableKeybinds = methodInfos
                .Where(x => x.GetCustomAttributes(typeof(HoldableKeybindAttribute), false).FirstOrDefault() != null)
                .GroupBy(x => x.GetCustomAttribute<HoldableKeybindAttribute>().Key);

            foreach (var methodInfoGroup in normalKeybinds)
            {
                var attribute = methodInfoGroup.ElementAt(0).GetCustomAttribute<KeybindAttribute>();
                var keybindActions = new List<Action>();

                foreach (var methodInfo in methodInfoGroup)
                {
                    var obj = provider.Resolve(methodInfo.DeclaringType);
                    keybindActions.Add(() => methodInfo.Invoke(obj, null));
                }

                this.RegisterKeybind(attribute.Id, attribute.Description, attribute.Key, () =>
                {
                    foreach (var action in keybindActions)
                    {
                        action.Invoke();
                    }
                });
            }

            foreach (var methodInfoGroup in holdableKeybinds)
            {
                var attribute = methodInfoGroup.ElementAt(0).GetCustomAttribute<HoldableKeybindAttribute>();
                var keybindActions = new List<Action>();

                foreach (var methodInfo in methodInfoGroup)
                {
                    var obj = provider.Resolve(methodInfo.DeclaringType);
                    keybindActions.Add(() => methodInfo.Invoke(obj, null));
                }

                this.RegisterHoldableKeybind(attribute.Id, attribute.Description, attribute.Key, attribute.TimeToHold, () =>
                {
                    foreach (var action in keybindActions)
                    {
                        action.Invoke();
                    }
                });
            }

            this._tickService.OnTick += this.WaitForHold;
        }

        /// <inheritdoc />
        public void RegisterKeybind(string id, string description, string key, Action onPress = null, Action onRelease = null)
        {
            this.InternalRegisterKeybind(id, description, key, "KEYBOARD", onPress, onRelease);
        }

        /// <inheritdoc />
        public void RegisterHoldableKeybind(string id, string description, string key, int timeToHold, Action onPress = null, Action onRelease = null)
        {
            HoldableKeybind holdableKeybind = new HoldableKeybind(onPress, timeToHold);

            void HoldableOnPress()
            {
                this._actions.Add(holdableKeybind);
            }

            void HoldableOnRelease()
            {
                holdableKeybind.ClearTimer();
                this._actions.Remove(holdableKeybind);
            }

            this.InternalRegisterKeybind(id, description, key, "KEYBOARD", HoldableOnPress, HoldableOnRelease);
        }

        /// <inheritdoc />
        public void RegisterMouseKeybind(string id, string description, string key, Action onPress = null, Action onRelease = null)
        {
            this.InternalRegisterKeybind(id, description, key, "MOUSE_BUTTON", onPress, onRelease);
        }

        private void InternalRegisterKeybind(string id, string description, string key, string keyMapper, Action onPress = null, Action onRelease = null)
        {
            Cfx.API.RegisterKeyMapping($"+{id}", description, keyMapper, key);
            Cfx.API.RegisterCommand(
                $"+{id}",
                new Action(() =>
                {
                    Cfx.API.CancelEvent();
                    onPress?.Invoke();
                }),
                false);
            Cfx.API.RegisterCommand(
                $"-{id}",
                new Action(() =>
                {
                    Cfx.API.CancelEvent();
                    onRelease?.Invoke();
                }),
                false);
        }

        private Task WaitForHold()
        {
            var toBeRemoved = new List<HoldableKeybind>();
            foreach (var item in this._actions)
            {
                item.IncrementTimer();
                if (item.Timer == item.TimeToHold)
                {
                    item.OnPress?.Invoke();
                    toBeRemoved.Add(item);
                }
            }

            foreach (var item in toBeRemoved)
            {
                item.ClearTimer();
                this._actions.Remove(item);
            }

            return Task.FromResult(0);
        }

        private sealed class HoldableKeybind
        {
            public HoldableKeybind(Action onPress, int timeToHold)
            {
                this.OnPress = onPress;
                this.TimeToHold = timeToHold;
            }

            public Action OnPress { get; }

            public int TimeToHold { get; }

            public int Timer { get; private set; }

            public void IncrementTimer()
            {
                this.Timer++;
            }

            public void ClearTimer()
            {
                this.Timer = 0;
            }
        }
    }
}