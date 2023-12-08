using System.Collections.ObjectModel;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides utility methods to handle translation files.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the available localizations.
    /// </summary>
    public ReadOnlyDictionary<string, string> AvailableLocalizations { get; }

    /// <summary>
    /// Gets the localization with the given key.
    /// </summary>
    /// <param name="key">The locale.</param>
    /// <returns>The serialized localization.</returns>
    public string GetLocalization(string key);
}