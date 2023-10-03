using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Handles interactions between entities.
/// </summary>
public interface IInteractionService
{
}

/// <summary>
/// Handles interactions between entities.
/// </summary>
internal interface IInternalInteractionService
{
    /// <summary>
    /// Makes the <see cref="Ped"/> follow the <see cref="Player"/>.
    /// </summary>
    /// <param name="player">The player to follow.</param>
    /// <param name="ped">The ped.</param>
    /// <param name="follow">Whether to follow or not.</param>
    void TogglePedFollowPlayer(FPlayer player, Ped ped, bool follow);
}