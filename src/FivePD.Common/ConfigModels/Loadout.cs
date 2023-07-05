using System.Collections.Generic;

namespace FivePD.Common.ConfigModels;

/// <summary>
/// Contains the definition of a loadout.
/// </summary>
public sealed record Loadout
{
    /// <summary>
    /// Gets the title of this loadout.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets an array of weapons in this loadout.
    /// </summary>
    public required List<Weapon> Weapons { get; init; } = new();

    /// <summary>
    /// A loadout weapon.
    /// </summary>
    public sealed record Weapon
    {
        /// <summary>
        /// Gets the key of this weapon.
        /// </summary>
        public required string Key { get; init; }

        /// <summary>
        /// Gets the components of this weapon.
        /// </summary>
        public List<string> ComponentKeys { get; init; } = new();

        /// <summary>
        /// Gets the ammunition count of this weapon.
        /// </summary>
        public int Ammunition { get; init; }
    }
}