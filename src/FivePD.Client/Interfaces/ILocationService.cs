using CitizenFX.Core;

namespace FivePD.Client.Interfaces;

/// <summary>
/// Provides location-related data.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets the local player's current location.
    /// </summary>
    /// <returns>The local player's location.</returns>
    public Vector3 GetPlayerLocation();
}