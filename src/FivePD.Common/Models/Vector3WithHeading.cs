using CitizenFX.Core;

namespace FivePD.Common.Models;

#pragma warning disable

public record struct Vector3WithHeading
{
    public Vector3 Location { get; init; }

    public float Heading { get; init; }
}