using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models;

/// <summary>
/// Represents the 'divisions' table.
/// </summary>
[Table("fivepd_divisions")]
public class Division
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
    /// Gets or sets the department of this division.
    /// </summary>
    public Department Department { get; set; }

    /// <summary>
    /// Gets or sets the users of this division.
    /// </summary>
    public IList<User> Users { get; set; }
}