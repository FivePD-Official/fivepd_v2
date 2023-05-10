using FivePD.Common.Localization;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides methods to request translations with their keys.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the currently set localization.
        /// </summary>
        public Localization CurrentLocalization { get; }

        /// <summary>
        /// Register all localization related event handlers.
        /// </summary>
        public void RegisterEventHandlers();

        /// <summary>
        /// Loads the default localization file.
        /// </summary>
        public void LoadDefaultLocalization();

        /// <summary>
        /// Loads the passed in localization.
        /// </summary>
        /// <param name="locale">The localization's key.</param>
        public void LoadLocalization(string locale);
    }
}