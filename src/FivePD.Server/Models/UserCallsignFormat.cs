using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models;

#pragma warning disable

/// <summary>
/// Represents the table containing the callsign format items
/// which are used for generating the callsign of users.
/// </summary>
[Table("fivepd_user_callsigns")]
public class UserCallsignFormat
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the callsign.
    /// </summary>
    public List<string> Values { get; set; }
}