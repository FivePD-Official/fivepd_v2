using System;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.Services;

#pragma warning disable

public class RandomService : IRandomService
{
    public Random Random { get; private set; } = new(Guid.NewGuid().GetHashCode());
}