using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for playing various sounds in the UI.
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Plays a sound.
    /// </summary>
    /// <param name="player">The player to play the sound for.</param>
    /// <param name="audioFiles">The sound effect's name to play.</param>
    public void Play(FPlayer player, AudioFiles audioFiles);

    /// <summary>
    /// Plays a sound.
    /// </summary>
    /// <param name="player">The player to play the sound for.</param>
    /// <param name="soundFileName">The name of the sound file to play.</param>
    public void Play(FPlayer player, string soundFileName);

    /// <summary>
    /// Reads out a text.
    /// </summary>
    /// <param name="player">The player to play the sound for.</param>
    /// <param name="text">The text to read out.</param>
    public void Speak(FPlayer player, string text);
}