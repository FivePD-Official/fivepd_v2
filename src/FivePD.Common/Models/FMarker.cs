using CitizenFX.Core;
using FivePD.Gamemode.Client.Enums;

namespace FivePD.Common.Models;

/// <summary>
/// Represents a marker.
/// Contains all parameters that can be passed to the native <c>CitizenFX.Core.Native.API.DrawMarker</c> function.
/// </summary>
public sealed record Marker
{
    /// <summary>
    /// Represents a marker color.
    /// </summary>
    /// <param name="Red">The red component.</param>
    /// <param name="Green">The green component.</param>
    /// <param name="Blue">The blue component.</param>
    /// <param name="Alpha">The alpha component.</param>
#pragma warning disable SA1313
    public record struct MarkerColor(int Red, int Green, int Blue, int Alpha);
#pragma warning restore SA1313

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public FMarkerType Type { get; set; }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets the direction.
    /// </summary>
    public Vector3 Direction { get; set; }

    /// <summary>
    /// Gets or sets the rotation.
    /// </summary>
    public Vector3 Rotation { get; set; }

    /// <summary>
    /// Gets or sets the scale.
    /// </summary>
    public Vector3 Scale { get; set; }

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    public MarkerColor Color { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the marker should bob up and down.
    /// </summary>
    public bool BobUpAndDown { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the marker should face the camera.
    /// </summary>
    public bool FaceCamera { get; set; }

    /// <summary>
    /// Gets or sets the p19 parameter.
    /// </summary>
    public int P19 { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the marker should rotate or not.
    /// </summary>
    public bool Rotate { get; set; }

    /// <summary>
    /// Gets or sets the texture dictionary.
    /// </summary>
    public string TextureDict { get; set; }

    /// <summary>
    /// Gets or sets the texture name.
    /// </summary>
    public string TextureName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the marker should be drawn on entities.
    /// </summary>
    public bool DrawOnEnts { get; set; }
}