using System;

#pragma warning disable

namespace FivePD.Gamemode.Server.Models;

[Flags]
public enum UserRole
{
    None = 0,
    Admin = 1,
    Developer = 2
}

/// <summary>
/// Represents the 'users' table.
/// </summary>
public class User
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
    /// Gets or sets the license key.
    /// </summary>
    public string License { get; set; }

    /// <summary>
    /// Gets or sets the role of the user.
    /// </summary>
    public int Role { get; set; } = (int)UserRole.None;

    /// <summary>
    /// Gets or sets the callsign.
    /// </summary>
    public string Callsign { get; set; }

    /// <summary>
    /// Gets or sets the id of the department.
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the department which the user is joined to.
    /// </summary>
    public Department Department { get; set; }

    /// <summary>
    /// Gets or sets the id of the division.
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// Gets or sets the division which the user is joined to.
    /// </summary>
    public Division Division { get; set; }

    /// <summary>
    /// Gets or sets the id of the rank.
    /// </summary>
    public int? RankId { get; set; }

    /// <summary>
    /// Gets or sets the rank.
    /// </summary>
    public Rank Rank { get; set; }

    public bool HasRole(UserRole role)
    {
        return (this.Role & (int)role) == (int)role;
    }
}