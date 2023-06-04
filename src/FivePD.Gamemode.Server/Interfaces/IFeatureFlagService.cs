using System;
using System.Linq.Expressions;
using FivePD.Gamemode.Common.ConfigModels;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Provides utility methods for feature configuration.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Gets the value of the selected property from the "features.json" config file.
    /// </summary>
    /// <param name="selector">The selector for a property.</param>
    /// <returns>A boolean.</returns>
    bool IsFeatureEnabled(Expression<Func<FeaturesConfig, bool>> selector);
}