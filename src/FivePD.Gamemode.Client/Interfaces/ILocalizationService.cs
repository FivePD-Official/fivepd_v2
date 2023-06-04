using System;
using System.Collections.Generic;

namespace FivePD.Gamemode.Client.Interfaces;

/// <summary>
/// Provides methods to request translations with their keys.
/// </summary>
internal interface ILocalizationService
{
    /// <summary>
    /// Fires when the localization has been changed.
    /// </summary>
    public event EventHandler<string> LocalizationChanged;

    /// <summary>
    /// Gets all available locales.
    /// </summary>
    public Dictionary<string, string> Locales { get; }

    /// <summary>
    /// Gets the currently set localization.
    /// </summary>
    public dynamic CurrentLocalization { get; }

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