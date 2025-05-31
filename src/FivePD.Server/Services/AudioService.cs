using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class AudioService : IAudioService
{
    /// <inheritdoc />
    public void Play(FPlayer player, string soundFileName)
    {
        player.GetUnderlyingPlayer().TriggerEvent(Events.Audio.PlaySound, soundFileName);
    }

    /// <inheritdoc />
    public void Speak(FPlayer player, string text)
    {
        player.GetUnderlyingPlayer().TriggerEvent(Events.Audio.Speak, text);
    }
}