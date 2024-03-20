using System;
using System.Collections;
using System.Collections.Generic;

namespace FivePD.Server.Models;

/// <summary>
/// Used to get/set vehicle configuration.
/// </summary>
/// <typeparam name="TOptions">The type of the Options field..</typeparam>
public class VehicleConfig<TOptions>
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the type of vehicle.
    /// </summary>
    public VehicleConfigType Type { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

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
    public TOptions Options { get; set; }
}