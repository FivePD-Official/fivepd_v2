namespace FivePD.Server.Models;

/// <summary>
/// Many-to-Many joining table for Department and Rank.
/// </summary>
public class DepartmentRank
{
    /// <summary>
    /// Gets or sets the department id.
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the rank id.
    /// </summary>
    public int RankId { get; set; }

    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    public virtual Department Department { get; set; }

    /// <summary>
    /// Gets or sets the rank.
    /// </summary>
    public virtual Rank Rank { get; set; }
}