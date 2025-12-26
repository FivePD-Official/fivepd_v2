using System.Collections.Generic;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class ProbabilityService : IProbabilityService
{
    private readonly IRandomService _randomService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProbabilityService"/> class.
    /// </summary>
    /// <param name="randomService">The random service for generating random values.</param>
    public ProbabilityService(IRandomService randomService)
    {
        this._randomService = randomService;
    }

    /// <inheritdoc />
    public T SelectOutcomeBasedOnCumulativeChance<T>(List<(int Probability, T Outcome)> probabilities)
    {
        var randomValue = this._randomService.GetRandomChance;
        var cumulativeProbability = 0;

        foreach (var (probability, outcome) in probabilities)
        {
            cumulativeProbability += probability;
            if (randomValue < cumulativeProbability)
            {
                return outcome;
            }
        }

        return default;
    }
}