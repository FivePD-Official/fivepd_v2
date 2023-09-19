using System;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class RandomService : IRandomService
{
    /// <inheritdoc />
    public Random Random { get; private set; } = new(Guid.NewGuid().GetHashCode());

    /// <inheritdoc />
    public int GetRandomChance => this.Random.Next(0, 101);
}