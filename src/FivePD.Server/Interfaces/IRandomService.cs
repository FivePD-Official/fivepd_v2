using System;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides methods for generating random numbers.
/// It's recommended to use this service instead of <see cref="Random"/> directly.
/// </summary>
public interface IRandomService
{
    /// <summary>
    /// Gets a random number generator.
    /// </summary>
    public Random Random { get; }

    /// <summary>
    /// Gets a random number between 0 and 99 (inclusive).
    /// </summary>
    public int GetRandomChance { get; }

    /// <summary>
    /// Gets a random number between the minimum and maximum values.
    /// Internally uses Random.Next.
    /// </summary>
    /// <param name="minValue">The minimum number.</param>
    /// <param name="maxValue">The maximum number.</param>
    /// <returns>A number between the specified numbers.</returns>
    public int Next(int minValue, int maxValue);
}