using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Extensions;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class AudioService : IAudioService
{
    /// <inheritdoc />
    public void Play(FPlayer player, AudioFiles audioFiles)
    {
        this.Play(player, audioFiles.GetDescription());
    }

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