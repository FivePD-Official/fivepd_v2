using System;
using System.ComponentModel.DataAnnotations.Schema;
using FivePD.Server.Utilities;

namespace FivePD.Server.Models;

#pragma warning disable

[Table("fivepd_vehicle_documents")]
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

    public VehicleDocumentStatus Status { get; set; }

    public VehicleDocumentType Type { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime ExpirationDate { get; set; }
}