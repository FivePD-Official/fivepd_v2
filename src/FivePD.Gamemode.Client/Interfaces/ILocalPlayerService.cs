using CitizenFX.Core;
using FivePD.Common.Models;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides access to the client's player.
    /// </summary>
    public interface ILocalPlayerService
    {
        /// <summary>
        /// Gets the local <see cref="FPlayer"/>.
        /// </summary>
        public FPlayer Player { get; }
    }
}