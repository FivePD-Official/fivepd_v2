using System;
using System.Collections;
using System.Collections.Generic;

namespace FivePD.Server.Models;

/// <summary>
/// Used to get/set vehicle configuration.
/// </summary>
public class VehicleConfig
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the type of vehicle.
    /// </summary>
    #pragma warning disable
    public enum Type
    {
        Police = 0,
        Ambulance = 1,
        TowTruck = 2,
    }
    #pragma warning restore

    /// <summary>
    /// Gets or sets the model.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// Gets or sets the livery.
    /// </summary>
    public int Livery { get; set; }

    /// <summary>
    /// Gets or sets the extras.
    /// </summary>
    public IList<int> Extras { get; set; }

    /// <summary>
    /// Gets or sets the options by type.
    /// </summary>
    public Dictionary<string, object> Options { get; set; }
}