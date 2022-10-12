// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Common;
using FivePD.Common.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Utilities;
using Newtonsoft.Json.Linq;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class SoundService : ISoundService
    {
        private readonly IUIService _iuiService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundService"/> class.
        /// </summary>
        /// <param name="iuiService">The <see cref="IUIService"/> to use.</param>
        public SoundService(IUIService iuiService)
        {
            this._iuiService = iuiService;
        }

        /// <inheritdoc />
        public void Play(Sound sound)
        {
            this._iuiService.SendMessage(new JObject
            {
                { "type", Events.NuiEventType.PlaySound },
                { "sound", sound.GetDescription() },
            });
        }

        /// <inheritdoc />
        public void Speak(string text)
        {
            this._iuiService.SendMessage(new JObject
            {
                { "type", Events.NuiEventType.Speak },
                { "textToPlay", text },
            });
        }
    }
}