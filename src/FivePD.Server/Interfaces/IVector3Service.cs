using System.Threading.Tasks;
using CitizenFX.Core;

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
    /// <returns>A serialized json with location and heading.</returns>
    public Task<string> GetRandomLocationAsync(Entity entity);

    /// <summary>
    /// Gets a random location on roadside with heading.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>A serialized json with location and heading.</returns>
    public Task<string> GetPointOnRoadSideAsync(Entity entity);
}