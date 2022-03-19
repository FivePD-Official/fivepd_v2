// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Linq;
using System.Reflection;
using FivePD.Gamemode.Client.Attributes;
using FivePD.Gamemode.Client.Injection;
using FivePD.Gamemode.Client.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class KeybindService : IKeybindService
    {
        /// <inheritdoc />
        public void Register(ServiceProvider provider)
        {
            var methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(KeybindAttribute), false).FirstOrDefault() != null);

            foreach (var method in methods)
            {
                var obj = provider.Resolve(method.DeclaringType);

                var attr = method.GetCustomAttribute<KeybindAttribute>();

                this.RegisterKeybind(attr.Id, attr.Description, attr.Key, () => method.Invoke(obj, null));
            }
        }

        /// <inheritdoc />
        public void RegisterKeybind(string id, string description, string key, Action onPress = null, Action onRelease = null)
        {
            this.InternalRegisterKeybind(id, description, key, "KEYBOARD", onPress, onRelease);
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
    }
}