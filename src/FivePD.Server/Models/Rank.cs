using System.Collections.Generic;

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
    /// Gets or sets a value indicating whether this rank is global or per department.
    /// </summary>
    public bool Global { get; set; }

    /// <summary>
    /// Gets or sets the required minimum experience points for the progression system.
    /// </summary>
    public int RequiredXp { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the permissions of this rank.
    /// </summary>
    public Permissions Permission { get; set; }

    /// <summary>
    /// Gets or sets the users of this ranks.
    /// </summary>
    public IList<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the DepartmentRanks.
    /// </summary>
    public IList<DepartmentRank> DepartmentRanks { get; set; }

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
        public bool SetDivision { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has the ability
        /// to change everything in a department.
        /// </summary>
        public bool DepartmentManager { get; set; }
    }
}