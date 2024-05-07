using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for Vector3 related tasks.
/// </summary>
public interface IVector3Service
{
    /// <summary>
    /// Gets a random location offset from the specified entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>A random Vector3.</returns>
    public Task<Vector3> GetRandomLocationAsync(Entity entity);

    /// <summary>
    /// Gets a random location on roadside from vector3 coordinates.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="p3">Always set to 1.</param>
    /// <returns>A random roadside Vector3.</returns>
    public Task<Vector3> GetPointOnRoadSideAsync(Entity entity, int p3);
}