using FivePD.Gamemode.Client.Utilities;

namespace FivePD.Gamemode.Client.Interfaces;

/// <summary>
/// Responsible for playing various sounds in the NUI.
/// </summary>
public interface ISoundService
{
    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    /// <param name="sound">The sound effect's name to play.</param>
    public void Play(Sound sound);

    /// <summary>
    /// Reads out a text.
    /// </summary>
    /// <param name="text">The text to read out.</param>
    public void Speak(string text);
}