using System.Collections.Generic;
using FivePD.Gamemode.Server.Models.Responses;

namespace FivePD.Gamemode.Server.Models;

/// <summary>
/// Represents the 'departments' table.
/// </summary>
public class Department
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the callsign.
    /// </summary>
    public string Callsign { get; set; }

    /// <summary>
    /// Gets or sets the callsign format.
    /// </summary>
    public string CallsignFormat { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of users.
    /// </summary>
    public int MaxUsers { get; set; }

    /// <summary>
    /// Gets or sets the default rank's id.
    /// </summary>
    public int? DefaultRankId { get; set; }

    /// <summary>
    /// Gets or sets the default rank which will be
    /// assigned to new members upon joining.
    /// </summary>
    public Rank DefaultRank { get; set; }

    /// <summary>
    /// Gets or sets the users of this department.
    /// </summary>
    public IEnumerable<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the divisions of this department.
    /// </summary>
    public IEnumerable<Division> Divisions { get; set; }

    /// <summary>
    /// Gets or sets the ranks of this department.
    /// </summary>
    public IEnumerable<Rank> Ranks { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="DepartmentResponse"/>.
    /// </summary>
    /// <returns>The created response object.</returns>
    internal DepartmentResponse ToResponse()
    {
        return new DepartmentResponse(this);
    }
}