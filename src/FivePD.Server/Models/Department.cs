using System.Collections.Generic;
using FivePD.Server.Models.DTO;

namespace FivePD.Server.Models;

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
    /// Gets or sets a value indicating whether this department uses global or custom ranks.
    /// </summary>
    public bool UseGlobalRanks { get; set; }

    /// <summary>
    /// Gets or sets the callsign format.
    /// </summary>
    public List<CallsignFormatItem> CallsignFormat { get; set; } = new();

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
    public List<User> Users { get; set; } = new();

    /// <summary>
    /// Gets or sets the divisions of this department.
    /// </summary>
    public List<Division> Divisions { get; set; } = new();

    /// <summary>
    /// Gets or sets the callout details of this department.
    /// </summary>
    public List<CalloutDetailsDepartment> CalloutDetailsDepartments { get; set; } = new();

    /// <summary>
    /// Gets or sets the area coordinates of this department.
    /// </summary>
    public List<DepartmentAreaCoordinate> AreaCoordinates { get; set; } = new();

    /// <summary>
    /// Gets or sets the DepartmentRanks.
    /// </summary>
    public List<DepartmentRank> DepartmentRanks { get; set; } = new();
}