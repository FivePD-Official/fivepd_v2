using FivePD.Common.ConfigModels.GeneralConfigs;

namespace FivePD.Client.Interfaces;

/// <summary>
/// Responsible for playing various sounds in the NUI.
/// </summary>
internal interface IAudioService
{
    /// <summary>
    /// Plays a sound.
    /// </summary>
    /// <param name="soundFileName">The name of the sound file to play.</param>
    public void Play(string soundFileName);

    /// <summary>
    /// Reads out a text.
    /// </summary>
    /// <param name="text">The text to read out.</param>
    public void Speak(string text);
}