using System.Collections.Generic;

namespace FivePD.Server.Models;

/// <summary>
/// Represents the table containing the callsign format items
/// which are used for generating the callsign of users.
/// </summary>
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