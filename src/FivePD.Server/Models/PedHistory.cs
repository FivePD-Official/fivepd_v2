using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FivePD.Common.ConfigModels;
using FivePD.Common.Enums;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a ped history.
/// </summary>
[Table("fivepd_ped_histories")]
public class PedHistory
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the vehicle the ped is owning.
    /// </summary>
    public int? VehicleHistoryId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the vehicle the ped is owning.
    /// </summary>
    public VehicleHistory VehicleHistory { get; set; }

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
    public IList<PedLicense> Licenses { get; set; }

    /// <summary>
    /// Gets or sets the alcohol level.
    /// </summary>
    public double AlcoholLevel { get; set; }

    /// <summary>
    /// Gets or sets the drugs.
    /// </summary>
    public List<Drug> Drugs { get; set; }

    /// <summary>
    /// Checks if the ped has a license.
    /// </summary>
    /// <param name="pedLicenseType">The license type to check.</param>
    /// <returns>True if the ped has the license, false otherwise.</returns>
    public bool HasLicense(PedLicenseType pedLicenseType) => this.Licenses.Any(x => x.Type == pedLicenseType);

    /// <summary>
    /// Gets the license of the specified type.
    /// </summary>
    /// <param name="pedLicenseType">The license type to get.</param>
    /// <returns>The license.</returns>
    public PedLicense GetLicense(PedLicenseType pedLicenseType) => this.Licenses.FirstOrDefault(x => x.Type == pedLicenseType);

    /// <summary>
    /// Gets a value indicating whether the ped's alcohol level is over the legal limit.
    /// </summary>
    /// <returns>True if the ped's alcohol level is over the legal limit, false otherwise.</returns>
    public bool AlcoholOverLegalLimit() => this.AlcoholLevel > ServiceLocator.GetService<IConfigService>().GeneralConfig.Peds.Alcohol.LegalLimit;
}