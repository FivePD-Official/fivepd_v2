using System;

namespace FivePD.Server.Attributes;

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
    public UIEventAttribute(string eventName, bool withPlayer = false) // TODO: remove withPlayer
    {
        this.Event = eventName;
    }

    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    public string Event { get; }
}