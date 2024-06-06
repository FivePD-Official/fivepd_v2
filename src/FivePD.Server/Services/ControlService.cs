using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using Serilog;

#pragma warning disable

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IControlService" />
[Obfuscate]
public class ControlService : IControlService, IOnInit
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
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Control.GetAll] += this.OnGetAll;
        this._eventHandlerDictionary[Events.Control.Trigger] += this.OnControlTrigger;
    }

    /// <inheritdoc />
    public void RegisterKeybind(
        string id,
        string key,
        string title,
        Action<FPlayer> onPress = null,
        Action<FPlayer> onRelease = null)
    {
        this.RegisterControl(id, key, title, "KEYBOARD", onPress, onRelease);
    }

    /// <inheritdoc />
    public void RegisterMomentaryKeybind(
        string id,
        string key,
        string title,
        int timeToHoldInMs = 0,
        Action<FPlayer> onPress = null,
        Action<FPlayer> onRelease = null)
    {
        this.RegisterControl(id, key, title, "KEYBOARD", onPress, onRelease, timeToHoldInMs);
    }

    /// <inheritdoc />
    public void RegisterMouseKeybind(
        string id,
        string key,
        string title,
        Action<FPlayer> onPress = null,
        Action<FPlayer> onRelease = null)
    {
        this.RegisterControl(id, key, title, "MOUSE_BUTTON", onPress, onRelease);
    }

    private void RegisterControl(
        string id,
        string key,
        string title,
        string keyMapper,
        Action<FPlayer> onPress = null,
        Action<FPlayer> onRelease = null,
        int timeToHoldInMs = 0)
    {
        this._controls.Add(new Control
        {
            Id = id,
            Title = title,
            Key = key,
            KeyMapper = keyMapper,
            OnPress = onPress,
            OnRelease = onRelease,
            TimeToHoldInMs = timeToHoldInMs,
        });
    }

    private void OnControlTrigger([FromSource] Player player, string triggeredId, string type)
    {
        var control = this._controls.FirstOrDefault(x => x.Id == triggeredId);
        if (control is null)
        {
            return;
        }

        var fPlayer = FPlayer.FromPlayer(player);
        if (fPlayer is null)
        {
            return;
        }

        if (type == "press")
        {
            control.OnPress?.Invoke(fPlayer);
        }
        else
        {
            control.OnRelease?.Invoke(fPlayer);
        }
    }

    private void OnGetAll(NetworkCallbackDelegate networkCallbackDelegate)
    {
        networkCallbackDelegate.InvokeWithJson(this._controls.Select(control => new ControlsDTO.ClientControlDTO
        {
            Id = control.Id,
            Title = control.Title,
            Key = control.Key,
            KeyMapper = control.KeyMapper,
            TimeToHoldInMs = control.TimeToHoldInMs,
        }));
    }

    private sealed record Control
    {
        public string Id { get; init; }

        public string Title { get; init; }

        public string Key { get; init; }

        public string KeyMapper { get; init; }

        public Action<FPlayer> OnPress { get; init; } = null;

        public Action<FPlayer> OnRelease { get; init; } = null;

        public int TimeToHoldInMs { get; init; } = 0;
    }
}