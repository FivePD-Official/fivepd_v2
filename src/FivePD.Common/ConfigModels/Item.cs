namespace FivePD.Common.ConfigModels;

/// <summary>
/// Contains the definition of an item.
/// </summary>
public sealed record Item
{
    /// <summary>
    /// Gets the title of this loadout.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item is illegal.
    /// </summary>
    public bool Illegal { get; init; }

    /// <summary>
    /// Gets where can the item spawn.
    /// </summary>
    public string Location { get; init; }
}