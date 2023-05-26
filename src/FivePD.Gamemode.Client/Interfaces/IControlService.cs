using System;
using FivePD.Gamemode.Client.Injection;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Used to register keybindings.
    /// </summary>
    public interface IControlService
    {
        /// <summary>
        /// Finds all methods with the Keybind attribute
        /// and register them.
        /// </summary>
        /// <param name="provider">The <see cref="ServiceProvider"/> to be used.</param>>
        public void Register(ServiceProvider provider);

        /// <summary>
        /// Registers a method to be ran on keybind press.
        /// </summary>
        /// <param name="id">The id of this keybind.</param>
        /// <param name="key">The key of this keybind.</param>
        /// <param name="description">The description of this keybind.</param>
        /// <param name="onPress">The method that'll be invoked on press.</param>
        /// <param name="onRelease">The method that'll be invoked on release.</param>
        public void RegisterKeybind(string id, string key, string description, Action onPress = null, Action onRelease = null);

        /// <summary>
        /// Registers a method to be ran on keybind press. A momentary keybind
        /// also supports a release method to be ran on key release.
        /// </summary>
        /// <param name="id">The id of this keybind.</param>
        /// <param name="key">The key of this keybind.</param>
        /// <param name="description">The description of this keybind.</param>
        /// <param name="timeToHoldInMs">Determines for how long the key should be pressed to trigger the action.</param>
        /// <param name="onPress">The method that'll be invoked on press.</param>
        /// <param name="onRelease">The method that'll be invoked on release.</param>
        /// <param name="canInvokeOnPress">The onPress method will only be invoked if this method returns true.</param>
        public void RegisterMomentaryKeybind(string id, string key, string description, int timeToHoldInMs, Action onPress = null, Action onRelease = null, Func<bool> canInvokeOnPress = null);

        /// <summary>
        /// Registers a method to be ran on keybind press.
        /// </summary>
        /// <param name="id">The id of this keybind.</param>
        /// <param name="key">The key of this keybind.</param>
        /// <param name="description">The description of this keybind.</param>
        /// <param name="onPress">The method that'll be invoked on press.</param>
        /// <param name="onRelease">The method that'll be invoked on release.</param>
        public void RegisterMouseKeybind(string id, string key, string description, Action onPress = null, Action onRelease = null);
    }
}