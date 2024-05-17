using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;

namespace FivePD.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Vector3"/>.
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    /// Checks whether the given point is inside the given polygon or not.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="points">The points of the polygon.</param>
    /// <returns>A boolean representing whether the point is inside the polygon or not.</returns>
    public static bool IsInsidePolygon(this Vector3 point, IEnumerable<Vector3> points)
    {
        var enumerable = points.ToList();
        var count = 0;

        for (int i = 0, j = enumerable.Count - 1; i < enumerable.Count; j = i++)
        {
            if (enumerable[i].Y > point.Y != enumerable[j].Y > point.Y &&
                point.X < ((enumerable[j].X - enumerable[i].X) * (point.Y - enumerable[i].Y) / (enumerable[j].Y - enumerable[i].Y)) + enumerable[i].X)
            {
                count++;
            }
        }

        return count % 2 == 1;
    }
}