using System;
using FivePD.Server.Utilities;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a vehicle document.
/// </summary>
public class VehicleDocument
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Id { get; set; } = IdGenerator.Generate();

    /// <summary>
    /// Gets or sets the entity which this value belongs to.
    /// </summary>
    public VehicleData VehicleData { get; set; }

    /// <summary>
    /// Gets or sets the status of the vehicle document.
    /// </summary>
    public VehicleDocumentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the type of the vehicle document.
    /// </summary>
    public VehicleDocumentType Type { get; set; }

    /// <summary>
    /// Gets or sets the issue date of the vehicle document.
    /// </summary>
    public DateTime IssueDate { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the vehicle document.
    /// </summary>
    public DateTime ExpirationDate { get; set; }
}