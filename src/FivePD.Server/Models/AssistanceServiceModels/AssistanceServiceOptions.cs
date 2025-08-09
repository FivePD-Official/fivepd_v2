namespace FivePD.Server.Models.AssistanceServiceModels;

/// <summary>
/// Represents the options for an assistance service.
/// </summary>
public sealed record AssistanceServiceOptions
{
    /// <summary>
    /// Gets the label for the request.
    /// </summary>
    public string RequestLabel { get; init; }

    /// <summary>
    /// Gets the label for the cancellation.
    /// </summary>
    public string CancelLabel { get; init; }

    /// <summary>
    /// Gets the description of the assistance service.
    /// </summary>
    public string Description { get; init; }

#pragma warning disable S125

    // TODO why was this commented out? - public List<Vector3> SpawnLocations { get; init; } = new();
#pragma warning restore S125

    /// <summary>
    /// Gets the spawn heading for the assistance service.
    /// </summary>
    public float SpawnHeading { get; init; }
}