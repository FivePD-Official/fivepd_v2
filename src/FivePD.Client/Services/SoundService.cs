using FivePD.Client.Interfaces;
using FivePD.Client.Utilities;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Extensions;
using Newtonsoft.Json.Linq;

namespace FivePD.Client.Services;

/// <inheritdoc />
internal class SoundService : ISoundService
{
    private readonly IUIManagerService _uiService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundService"/> class.
    /// </summary>
    /// <param name="uiService">The <see cref="IUIManagerService"/> to use.</param>
    public SoundService(IUIManagerService uiService)
    {
        this._uiService = uiService;
    }

    /// <inheritdoc />
    public void Play(Sound sound)
    {
        this._uiService.SendMessage(new JObject
        {
            { "type", Events.NuiEventType.PlaySound },
            { "sound", sound.GetDescription() },
        });
    }

    /// <inheritdoc />
    public void Speak(string text)
    {
        this._uiService.SendMessage(new JObject
        {
            { "type", Events.NuiEventType.Speak },
            { "textToPlay", text },
        });
    }
}