using System;
using FivePD.Server.Utilities;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a license.
/// </summary>
public class PedLicense
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Id { get; set; } = IdGenerator.Generate();

    /// <summary>
    /// Gets or sets the entity which this value belongs to.
    /// </summary>
    public PedHistory PedHistory { get; set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public PedLicenseType Type { get; set; }

    /// <summary>
    /// Gets or sets the date of issue.
    /// </summary>
    public DateTime IssueDate { get; set; }

    /// <summary>
    /// Gets or sets the expiration date.
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Gets a value indicating whether the license is valid.
    /// </summary>
    public bool Valid => DateTime.Now < this.ExpirationDate;
}