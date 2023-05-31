namespace FivePD.Common.ConfigModels;

/// <summary>
/// Contains the definition of an item.
/// </summary>
public sealed record Item
{
    /// <summary>
    /// Defines where the item can be found.
    /// </summary>
    public enum ItemLocation
    {
        /// <summary>
        /// The item can spawn both in vehicles and on peds.
        /// </summary>
        Everywhere,

        /// <summary>
        /// The item can only spawn in vehicles.
        /// </summary>
        Vehicle,

        /// <summary>
        /// The item can only spawn on peds.
        /// </summary>
        Ped,
    }

    /// <summary>
    /// Gets the title of this loadout.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item is illegal.
    /// </summary>
    public bool IsIllegal { get; init; }

    /// <summary>
    /// Gets where can the item spawn.
    /// </summary>
    public ItemLocation Location { get; init; } = ItemLocation.Everywhere;
}