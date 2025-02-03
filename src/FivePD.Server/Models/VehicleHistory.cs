using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FivePD.Common.ConfigModels;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a vehicle history.
/// </summary>
[Table("fivepd_vehicle_histories")]
public class VehicleHistory
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the owner ped history.
    /// </summary>
    public int? PedHistoryId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the vehicle the ped is owning.
    /// </summary>
    public PedHistory PedHistory { get; set; }

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
    public List<Item> Items { get; set; }

    /// <summary>
    /// Gets or sets the documents of the vehicle.
    /// </summary>
    public IList<VehicleDocument> Documents { get; set; }
}