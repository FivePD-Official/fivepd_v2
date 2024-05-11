using System.ComponentModel;

namespace FivePD.Common.Enums;

#pragma warning disable

/// <summary>
/// Contains the available sound effect names.
/// </summary>
public enum Sound
{
    [Description("mic_click.mp3")] MicClick,

    [Description("beep.mp3")] Beep,

    [Description("boop_boop.mp3")] BoopBoop,
}