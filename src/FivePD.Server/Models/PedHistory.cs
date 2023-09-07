using System;
using System.Collections.Generic;
using FivePD.Common.ConfigModels;
using FivePD.Common.Enums;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a ped history.
/// </summary>
public class PedHistory
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    public FGender Gender { get; set; }

    /// <summary>
    /// Gets or sets the firstname.
    /// </summary>
    public string Firstname { get; set; }

    /// <summary>
    /// Gets or sets the lastname.
    /// </summary>
    public string Lastname { get; set; }

    /// <summary>
    /// Gets or sets the birthdate.
    /// </summary>
    public DateTime Birthdate { get; set; }

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    public List<Item> Items { get; set; }

    /// <summary>
    /// Gets or sets the warrants.
    /// </summary>
    public List<string> Warrants { get; set; }

    /// <summary>
    /// Gets or sets the licenses.
    /// </summary>
    public IList<License> Licenses { get; set; }
}