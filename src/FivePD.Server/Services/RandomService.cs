using System;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class RandomService : IRandomService
{
    /// <inheritdoc />
    public Random Random { get; private set; } = new(Guid.NewGuid().GetHashCode());

    /// <inheritdoc />
    public int GetRandomChance => this.Random.Next(0, 100);

    /// <inheritdoc />
    public int Next(int minValue, int maxValue)
    {
        return this.Random.Next(minValue, maxValue);
    }
}