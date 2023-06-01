using System.Collections.Generic;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Provides utility methods to handle translation files.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets asd.
        /// </summary>
        public Dictionary<string, string> AvailableLocalizations { get; }

        /// <summary>
        /// Gets asd.
        /// </summary>
        /// <param name="key">The locale.</param>
        /// <returns>The s.</returns>
        public string GetLocalization(string key);
    }
}