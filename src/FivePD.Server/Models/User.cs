using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FivePD.Common.Enums;

namespace FivePD.Server.Models;

/// <summary>
/// Represents the 'users' table.
/// </summary>
[Table("fivepd_users")]
public class User
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this player has access to use FivePD or not.
    /// </summary>
    public bool AccessFivePD { get; set; }

    /// <summary>
    /// Gets or sets the preferred language of the user.
    /// </summary>
    public string PreferredLanguage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is playing for the first time.
    /// </summary>
    public bool FirstLogin { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has dismissed the welcome message.
    /// </summary>
    public bool DismissedWelcomeMessage { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the License identifier.
    /// </summary>
    public string IdentifierLicense { get; set; }

    /// <summary>
    /// Gets or sets the Discord identifier.
    /// </summary>
    public string IdentifierDiscord { get; set; }

    /// <summary>
    /// Gets or sets the Steam identifier.
    /// </summary>
    public string IdentifierSteam { get; set; }

    /// <summary>
    /// Gets or sets the Xbl identifier.
    /// </summary>
    public string IdentifierXbl { get; set; }

    /// <summary>
    /// Gets or sets the Xbox live identifier.
    /// </summary>
    public string IdentifierLive { get; set; }

    /// <summary>
    /// Gets or sets the experience points.
    /// </summary>
    public int Xp { get; set; }

    /// <summary>
    /// Gets or sets the role of the user.
    /// </summary>
    public int Role { get; set; } = (int)UserRole.None;

    /// <summary>
    /// Gets or sets the callsign.
    /// </summary>
    public string Callsign { get; set; }

    /// <summary>
    /// Gets or sets the id of the user callsign.
    /// </summary>
    public int? UserCallsignFormatId { get; set; }

    /// <summary>
    /// Gets or sets the user callsign.
    /// </summary>
    public UserCallsignFormat CallsignFormat { get; set; }

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

    /// <summary>
    /// Gets or sets a value indicating whether the rank can be increased or decreased by the progression system.
    /// Enable this if you want to keep the rank of the user.
    /// </summary>
    public bool FixedRank { get; set; }

    /// <summary>
    /// Gets or sets the messages sent to the user.
    /// </summary>
    public IList<Message> Messages { get; set; }

    /// <summary>
    /// Gets whether the user has the specified role.
    /// </summary>
    /// <param name="role">The role you want to check.</param>
    /// <returns>A boolean.</returns>
    public bool HasRole(UserRole role)
    {
        return (this.Role & (int)role) == (int)role;
    }
}