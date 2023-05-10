using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Exceptions;
using FivePD.Common.Extensions;
using FivePD.Common.Localization;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class LocalizationService : ILocalizationService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IFileHandlerService _fileHandlerService;
        private readonly ILoggerService _loggerService;
        private readonly IUIService _uiService;
        private readonly string _defaultLocale = "en";

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="fileHandlerService">The <see cref="IFileHandlerService" /> to use.</param>
        /// <param name="loggerService">The <see cref="ILoggerService" /> to use.</param>
        /// <param name="uiService">The <see cref="IUIService" /> to use.</param>
        public LocalizationService(EventHandlerDictionary eventHandlerDictionary, IFileHandlerService fileHandlerService, ILoggerService loggerService, IUIService uiService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._fileHandlerService = fileHandlerService;
            this._loggerService = loggerService;
            this._uiService = uiService;
        }

        /// <inheritdoc />
        public Localization CurrentLocalization { get; private set; }

        /// <inheritdoc />
        public void RegisterEventHandlers()
        {
            this._eventHandlerDictionary.AddUIEvent(Events.Localization.GetLocalizationFromNui, new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                cb.Invoke(this.CurrentLocalization);
            }));

            this._eventHandlerDictionary[Events.UI.Player.LocalizationChange] += new Action<string>(data => this._uiService.SendMessage(data));
        }

        /// <inheritdoc />
        public void LoadDefaultLocalization()
        {
            this.LoadLocalization(this._defaultLocale);
        }

        /// <inheritdoc />
        public void LoadLocalization(string locale)
        {
            var path = $"/Localization/{locale}.json";
            var file = this._fileHandlerService.LoadFile<Localization>(path, false);

            if (file.Item2 is null)
            {
                this.CurrentLocalization = file.Item1;
                return;
            }

            var exceptionMessage = file.Item2 switch
            {
                EmptyConfigException _ => this.CurrentLocalization.Warning.EmptyConfigException,
                _ => this.CurrentLocalization.Warning.ConfigException,
            };

            this._loggerService.Warn(exceptionMessage.ReplaceParams(
                new Dictionary<string, string>
                {
                    { "path", $"\"{path}\"" },
                }));
        }
    }
}