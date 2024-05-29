using System.Collections.Generic;
using FivePD.Server.Models.LoadoutModels;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides access to loadout config data.
/// </summary>
public interface ILoadoutConfigService
{
    /// <summary>
    /// Gets all loadout configs.
    /// </summary>
    /// <returns>A list of loadout configs.</returns>
    public List<LoadoutConfig> GetAll();
}