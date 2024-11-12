using System;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Extensions;
using Newtonsoft.Json.Linq;

namespace FivePD.Client.Services;

/// <inheritdoc cref="IAudioService" />
internal class AudioService : IAudioService, IOnInit
{
    private readonly IUIManagerService _uiService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioService"/> class.
    /// </summary>
    /// <param name="uiService">The <see cref="IUIManagerService"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public AudioService(IUIManagerService uiService, EventHandlerDictionary eventHandlerDictionary)
    {
        this._uiService = uiService;
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary[Events.Audio.PlaySound] += new Action<string>(this.Play);
        this._eventHandlerDictionary[Events.Audio.Speak] += new Action<string>(this.Speak);
    }

    /// <inheritdoc />
    public void Play(AudioFiles audioFiles)
    {
        this.Play(audioFiles.GetDescription());
    }

    /// <inheritdoc />
    public void Play(string soundFileName)
    {
        this._uiService.SendMessage(new JObject
        {
            { "Event", Events.Audio.PlaySound },
            { "Sound", soundFileName },
        });
    }

    /// <inheritdoc />
    public void Speak(string text)
    {
        this._uiService.SendMessage(new JObject
        {
            { "Event", Events.Audio.Speak },
            { "Text", text },
        });
    }
}