using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models;

/// <summary>
/// Represents an incident (callout or traffic stop) history.
/// </summary>
[Table("fivepd_incident_history_users")]
public class IncidentHistoryUser
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Gets or sets the entity which this value belongs to.
    /// </summary>
    public IncidentHistory IncidentHistory { get; set; }
}