using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models;

/// <summary>
/// Represents the relationship between a callout and a department.
/// </summary>
[Table("fivepd_callout_details_departments")]
public class CalloutDetailsDepartment
{
    /// <summary>
    /// Gets or sets the identifier of the callout details.
    /// </summary>
    public int CalloutDetailsId { get; set; }

    /// <summary>
    /// Gets or sets the callout details.
    /// </summary>
    public CalloutDetails CalloutDetails { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the department.
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    public Department Department { get; set; }
}