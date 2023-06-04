using FivePD.Gamemode.Common.Models;

namespace FivePD.Gamemode.Client.Interfaces;

/// <summary>
/// Provides access to the client's player.
/// </summary>
internal interface ILocalPlayerService
{
    /// <summary>
    /// Gets the local <see cref="FPlayer"/>.
    /// </summary>
    public FPlayer Player { get; }
}