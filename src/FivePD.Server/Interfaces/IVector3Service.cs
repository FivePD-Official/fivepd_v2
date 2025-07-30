using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for Vector3 related tasks.
/// </summary>
public interface IVector3Service
{
    /// <summary>
    /// Gets a random location offset from the specified entity with heading.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="offsetMin">The minimum offset.</param>
    /// <param name="offsetMax">The maximum offset.</param>
    /// <returns>A serialized json with location and heading.</returns>
    public Task<Vector3> GetRandomLocationAsync(Entity entity, int offsetMin, int offsetMax);

    /// <summary>
    /// Gets a random location on roadside with heading.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="offsetMin">The minimum offset.</param>
    /// <param name="offsetMax">The maximum offset.</param>
    /// <returns>A serialized json with location and heading.</returns>
    public Task<Vector3WithHeading> GetPointOnRoadSideAsync(Entity entity, int offsetMin, int offsetMax);

    /// <summary>
    /// Gets the forward vector of the specified player.
    /// </summary>
    /// <param name="player">The player whose forward vector is to be calculated.</param>
    /// <returns>A Vector3 representing the forward direction of the player.</returns>
    public Task<Vector3> GetPlayerForwardVectorAsync(Player player);
}