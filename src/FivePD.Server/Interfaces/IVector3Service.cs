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
    /// <returns>A serialized json with location and heading.</returns>
    public Task<Vector3> GetRandomLocationAsync(Entity entity);

    /// <summary>
    /// Gets a random location on roadside with heading.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>A serialized json with location and heading.</returns>
    public Task<Vector3WithHeading> GetPointOnRoadSideAsync(Entity entity);
}