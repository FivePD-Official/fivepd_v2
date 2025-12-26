using System.Collections.Generic;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides methods for probability-based calculations and outcome selection.
/// </summary>
public interface IProbabilityService
{
    /// <summary>
    /// Selects an outcome based on cumulative probabilities.
    /// </summary>
    /// <typeparam name="T">The type of the outcome.</typeparam>
    /// <param name="probabilities">A list of tuples where each tuple contains a probability (0-100) and its corresponding outcome.</param>
    /// <returns>The selected outcome based on the cumulative probabilities.</returns>
    public T SelectOutcomeBasedOnCumulativeChance<T>(List<(int Probability, T Outcome)> probabilities);
}