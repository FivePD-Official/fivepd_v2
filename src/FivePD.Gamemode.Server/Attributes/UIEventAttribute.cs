using System;

namespace FivePD.Gamemode.Server.Attributes;

/// <summary>
/// Used to bind methods to events.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal class UIEventAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UIEventAttribute"/> class.
    /// </summary>
    /// <param name="eventName">Name of the event..</param>
    /// <param name="withPlayer">Determines whether the event will be invoked with the Player or not.</param>
    public UIEventAttribute(string eventName, bool withPlayer = false)
    {
        this.Event = eventName;
        this.WithPlayer = withPlayer;
    }

    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    public string Event { get; }

    /// <summary>
    /// Gets a value indicating whether the event should be invoked
    /// with the Player object as the first argument.
    /// </summary>
    public bool WithPlayer { get; }
}