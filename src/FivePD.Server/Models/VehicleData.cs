using System.Collections.Generic;
using System.Linq;
using FivePD.Common.ConfigModels;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a vehicle history.
/// </summary>
public class VehicleData
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the owner ped history.
    /// </summary>
    public int? PedDataId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the vehicle the ped is owning.
    /// </summary>
    public PedData PedData { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's license plate.
    /// </summary>
    public string LicensePlate { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's color.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's brand.
    /// </summary>
    public string Brand { get; set; }

    /// <summary>
    /// Gets or sets the items of the vehicle.
    /// </summary>
    public List<Item> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets the documents of the vehicle.
    /// </summary>
    public List<VehicleDocument> Documents { get; set; } = new();

    /// <summary>
    /// Checks if the vehicle has a document.
    /// </summary>
    /// <param name="vehicleDocumentType">The document type to check.</param>
    /// <returns>True if the vehicle has the document, false otherwise.</returns>
    public bool HasDocument(VehicleDocumentType vehicleDocumentType) => this.Documents.Any(x => x.Type == vehicleDocumentType);

    /// <summary>
    /// Gets the document of the specified type.
    /// </summary>
    /// <param name="vehicleDocumentType">The document type to get.</param>
    /// <returns>The license.</returns>
    public VehicleDocument GetDocument(VehicleDocumentType vehicleDocumentType) => this.Documents.FirstOrDefault(x => x.Type == vehicleDocumentType);
}