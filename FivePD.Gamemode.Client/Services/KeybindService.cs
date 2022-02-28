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

                Cfx.API.RegisterKeyMapping($"+{attr.Id}", "description", "keyboard", attr.Key);
                Cfx.API.RegisterCommand(
                    $"+{attr.Id}",
                    new Action(() =>
                    {
                        Cfx.API.CancelEvent();
                        method.Invoke(obj, null);
                    }),
                    false);
                Cfx.API.RegisterCommand(
                    $"-{attr.Id}",
                    new Action(() =>
                    {
                        Cfx.API.CancelEvent();
                    }),
                    false);
            }
        }
    }
}