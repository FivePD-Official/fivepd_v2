using System.Collections.Generic;
using System.Threading.Tasks;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides methods to request translations with their keys.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets all available locales.
        /// </summary>
        public Dictionary<string, string> Locales { get; }

        /// <summary>
        /// Gets asd.
        /// </summary>
        public dynamic CurrentLocalization { get; }

        /// <summary>
        /// Register all localization related event handlers.
        /// </summary>
        public void RegisterEventHandlers();

        /// <summary>
        /// Finds and loads all translation files located in fivepd/Localization.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Dictionary<string, string>> LoadLocalizations();

        /// <summary>
        /// Loads the default localization file.
        /// </summary>
        public void LoadDefaultLocalization();

        /// <summary>
        /// Loads the passed in localization.
        /// </summary>
        /// <param name="locale">The localization's key.</param>
        public void LoadLocalization(string locale);

        /// <summary>
        /// Gets a localization value by its key.
        /// </summary>
        /// <param name="key">The key of the localization to get.</param>
        /// <returns>A localized value.</returns>
        public string Get(string key);
    }
}