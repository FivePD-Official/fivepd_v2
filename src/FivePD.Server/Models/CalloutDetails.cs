using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models;

/// <summary>
/// Represents the details of a callout.
/// </summary>
[Table("fivepd_callout_details")]
public class CalloutDetails
{
    /// <summary>
    /// Gets or sets the unique identifier for the callout details.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the callout is active.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// Gets or sets the name of the assembly.
    /// </summary>
    public string AssemblyName { get; set; }

    /// <summary>
    /// Gets or sets the ruleset type for the callout.
    /// </summary>
    public CalloutDetailsRulesetType RulesetType { get; set; }

    /// <summary>
    /// Gets or sets the departments associated with the callout.
    /// </summary>
    public IList<CalloutDetailsDepartment> CalloutDetailsDepartments { get; set; }
}