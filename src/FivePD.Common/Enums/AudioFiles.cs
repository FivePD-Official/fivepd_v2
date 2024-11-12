using System.ComponentModel;

namespace FivePD.Common.Enums;

#pragma warning disable

/// <summary>
/// Contains the available sound effect names.
/// </summary>
public enum AudioFiles
{
    [Description("mic_click.mp3")] MicClick,

    [Description("beep.mp3")] Beep,

    [Description("boop_boop.mp3")] BoopBoop,
    
    [Description("game_notification.mp3")] GameNotification, // src: https://pixabay.com/sound-effects/search/notification/
}