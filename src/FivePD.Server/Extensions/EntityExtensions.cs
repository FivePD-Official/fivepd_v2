using System;
using CitizenFX.Core;

namespace FivePD.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Entity"/>.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Gets the forward vector of the given entity.
    /// </summary>
    /// <param name="entity">An entity.</param>
    /// <returns>The forward vector of the entity.</returns>
    /// TODO: fix, the returned forward vector does not seem to be correct
    public static Vector3 ForwardVector(this Entity entity)
    {
#pragma warning disable
        // const float rad = (float)Math.PI / 180f;
        // var yaw = entity.Heading * rad;
        //
        // var x = (float)Math.Cos(yaw);
        // var y = (float)Math.Sin(yaw);
        //
        // return new Vector3(x, y, 0f);
        
        Vector3 rotationRadians = new Vector3(MathUtil.DegreesToRadians(entity.Rotation.X), MathUtil.DegreesToRadians(entity.Rotation.Y), MathUtil.DegreesToRadians(entity.Rotation.Z));
        Vector3 forwardVector = new Vector3(-(float)Math.Sin(rotationRadians.Y), (float)Math.Cos(rotationRadians.Y), 0);
        forwardVector.Normalize();

        return forwardVector;

        // return new Vector3(0.0f, -1.0f, 0.0f);

        // forward.x =  cos(pitch) * sin(yaw);
        // forward.y = -sin(pitch);
        // forward.z =  cos(pitch) * cos(yaw);

        // var x = (float)(Math.Cos(entity.Rotation.X) * Math.Sin(entity.Rotation.Z));
        // var y = (float)(-Math.Sin(entity.Rotation.X));
        // var z = (float)(Math.Cos(entity.Rotation.X) * Math.Cos(entity.Rotation.Z));
        //
        // return new Vector3(x,y,z);
    }
}