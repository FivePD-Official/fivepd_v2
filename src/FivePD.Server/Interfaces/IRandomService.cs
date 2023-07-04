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
}