using System.Collections.Generic;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.Enums;

namespace FivePD.Common.Models;

/// <summary>
/// Represents a notification which is displayed via the NUI outside the MDT.
/// </summary>
public record GameNotificationOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameNotificationOptions"/> class.
    /// </summary>
    public GameNotificationOptions()
    {
        this.Title = string.Empty;
        this.Message = string.Empty;
        this.ImageSrc = string.Empty;
        this.Keys = new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets the title of the notification.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets the message of the notification.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the parameters to be replaced in the message on the client.
    /// </summary>
    public Dictionary<string, string> MessageParams { get; init; }

    /// <summary>
    /// Gets the image source of the notification.
    /// </summary>
    public string ImageSrc { get; init; }

    /// <summary>
    /// Gets the timeout of the notification.
    /// </summary>
    public int Timeout { get; init; } = 5000;

    /// <summary>
    /// Gets the keys to display in the notification.
    /// </summary>
    public Dictionary<string, string> Keys { get; init; }

    /// <summary>
    /// Gets a value indicating whether a sound should be played when this notification appears.
    /// </summary>
    public string PlayAudio { get; init; }
}