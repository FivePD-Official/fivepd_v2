using System;
using CitizenFX.Core;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Used to register keybindings.
/// </summary>
public interface IControlService
{
    /// <summary>
    /// Registers a method to be ran on keybind press.
    /// </summary>
    /// <param name="id">The id of this keybind.</param>
    /// <param name="key">The key of this keybind.</param>
    /// <param name="title">The description of this keybind.</param>
    /// <param name="onPress">The method that'll be invoked on press.</param>
    /// <param name="onRelease">The method that'll be invoked on release.</param>
    public void RegisterKeybind(string id, string key, string title, Action<Player> onPress = null, Action<Player> onRelease = null);

    /// <summary>
    /// Registers a method to be ran on keybind press. A momentary keybind
    /// also supports a release method to be ran on key release.
    /// </summary>
    /// <param name="id">The id of this keybind.</param>
    /// <param name="key">The key of this keybind.</param>
    /// <param name="title">The description of this keybind.</param>
    /// <param name="timeToHoldInMs">The time to hold in ms.</param>
    /// <param name="onPress">The method that'll be invoked on press.</param>
    /// <param name="onRelease">The method that'll be invoked on release.</param>
    public void RegisterMomentaryKeybind(string id, string key, string title, int timeToHoldInMs = 0, Action<Player> onPress = null, Action<Player> onRelease = null);

    /// <summary>
    /// Registers a method to be ran on keybind press.
    /// </summary>
    /// <param name="id">The id of this keybind.</param>
    /// <param name="key">The key of this keybind.</param>
    /// <param name="title">The description of this keybind.</param>
    /// <param name="onPress">The method that'll be invoked on press.</param>
    /// <param name="onRelease">The method that'll be invoked on release.</param>
    public void RegisterMouseKeybind(string id, string key, string title, Action<Player> onPress = null, Action<Player> onRelease = null);
}