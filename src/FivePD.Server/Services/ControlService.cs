using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Utilities;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;

#pragma warning disable

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IControlService" />
public class ControlService : IControlService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly List<ControlsDTO> _controls = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlService"/> class.
    /// </summary>
    public ControlService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }
        
    /// <inheritdoc />
    void IServiceWithEvents.RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Control.GetAll] += new Action<NetworkCallbackDelegate>(this.OnGetAll);
    }

    /// <inheritdoc />
    public void RegisterKeybind(string id, string key, string title, Action<Player> onPress = null, Action<Player> onRelease = null)
    {
        this.RegisterControl(id, key, title, "KEYBOARD", onPress, onRelease);
    }

    /// <inheritdoc />
    public void RegisterMomentaryKeybind(string id, string key, string title, int timeToHoldInMs = 0, Action<Player> onPress = null, Action<Player> onRelease = null)
    {
        this.RegisterControl(id, key, title, "KEYBOARD", onPress, onRelease, timeToHoldInMs);
    }

    /// <inheritdoc />
    public void RegisterMouseKeybind(string id, string key, string title, Action<Player> onPress = null, Action<Player> onRelease = null)
    {
        this.RegisterControl(id, key, title, "MOUSE_BUTTON", onPress, onRelease);
    }

    private void RegisterControl(string id, string key, string title, string keyMapper, Action<Player> onPress = null, Action<Player> onRelease = null, int timeToHoldInMs = 0)
    {
        this._controls.Add(new ControlsDTO
        {
            Id = id,
            Title = title,
            Key = key,
            KeyMapper = keyMapper,
            OnPress = onPress,
            OnRelease = onRelease,
            TimeToHoldInMs = timeToHoldInMs,
        });
        this._eventHandlerDictionary[Events.Control.Trigger] += new Action<Player, string, string>(this.OnControlTrigger);
    }

    private void OnControlTrigger([FromSource] Player player, string triggeredId, string type)
    {
        var control = this._controls.FirstOrDefault(x => x.Id == triggeredId);
        if (type == "press")
        {
            control?.OnPress?.Invoke(player);
        }
        else
        {
            control?.OnRelease?.Invoke(player);
        }
    }

    private void OnGetAll(NetworkCallbackDelegate networkCallbackDelegate)
    {
        networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(this._controls.Select(x => x.ToClientDTO())));
    }
}