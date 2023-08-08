using System.Collections.Generic;

#pragma warning disable SA1402

namespace FivePD.Server.Models;

/// <summary>
/// Represents the 'divisions' table.
/// </summary>
public class Rank
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the required minimum experience points for the progression system.
    /// </summary>
    public int RequiredXp { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the department which it's attached to.
    /// </summary>
    public Department Department { get; set; }

    /// <summary>
    /// Gets or sets the permissions of this rank.
    /// </summary>
    public Permissions Permission { get; set; }

    /// <summary>
    /// Gets or sets the users of this ranks.
    /// </summary>
    public IEnumerable<User> Users { get; set; }

    /// <summary>
    /// Defines the actions a given <see cref="Rank"/> can do.
    /// </summary>
    public class Permissions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user has the ability
        /// to kick other players from the department.
        /// </summary>
        public bool Kick { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has the ability
        /// to set the rank of other players in the department.
        /// </summary>
        public bool SetRank { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has the ability
        /// to set the callsign of other players in the department.
        /// </summary>
        public bool SetCallsign { get; set; }
    }
}