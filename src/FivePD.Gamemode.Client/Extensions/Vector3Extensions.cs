using CitizenFX.Core;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Extensions;

/// <summary>
/// Holds extension methods for CitizenFX.Core.Client.Vector3.
/// </summary>
internal static class Vector3Extensions
{
    /// <summary>
    /// Gets the nearest street's name to the given position.
    /// </summary>
    /// <param name="position">A <see cref="Vector3"/> position.</param>
    /// <returns>A string representing the street's name.</returns>
    public static string GetStreetName(this Vector3 position)
    {
        var streetHash = default(uint);
        var crossingRoadHash = default(uint);
        Cfx.API.GetStreetNameAtCoord(position.X, position.Y, position.Z, ref streetHash, ref crossingRoadHash);

        var streetName = Cfx.API.GetStreetNameFromHashKey(streetHash);

        return streetName;
    }
}