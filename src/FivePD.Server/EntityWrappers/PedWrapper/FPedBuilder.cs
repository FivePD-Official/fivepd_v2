using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Common.Builders;
using FivePD.Common.ConfigModels;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Utilities;

namespace FivePD.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Builder class for creating FPed instances with configurable properties.
/// Provides a fluent interface for setting up ped characteristics, licenses, items, and other attributes.
/// </summary>
public class FPedBuilder : CommonFPedBuilder
{
    private readonly IRandomService _randomService = ServiceLocator.GetService<IRandomService>();
    private readonly IProbabilityService _probabilityService = ServiceLocator.GetService<IProbabilityService>();

#pragma warning disable
    /// <summary>
    /// Configuration options for the <see cref="WithWarrants(WithWarrantsOptions)"/> method.
    /// </summary>
    /// <param name="Warrants">The list of warrant types to potentially assign.</param>
    /// <param name="IncludeAll">When true, includes all warrants regardless of probability. Should only be true when ped history is loaded.</param>
    /// <param name="ChanceForAnyWarrant">The probability (0-100) that the ped will have any warrant at all.</param>
    /// <param name="MaxWarrants">The maximum number of warrants to assign.</param>
    public readonly record struct WithWarrantsOptions(List<string> Warrants, bool IncludeAll, int ChanceForAnyWarrant, int MaxWarrants);
#pragma warning restore

    /// <summary>
    /// Initializes a new instance of the <see cref="FPedBuilder"/> class.
    /// </summary>
    /// <param name="fPedBuilder">The base CommonFPedBuilder to initialize from.</param>
    public FPedBuilder(CommonFPedBuilder fPedBuilder)
        : base(fPedBuilder.NetworkId)
    {
        this.OwningVehicleNetworkId = fPedBuilder.NetworkId;
        this.Birthdate = fPedBuilder.Birthdate;
        this.Firstname = fPedBuilder.Firstname;
        this.Lastname = fPedBuilder.Lastname;
        this.Gender = fPedBuilder.Gender;
        this.Items = fPedBuilder.Items;
        this.Warrants = fPedBuilder.Warrants;
    }

    /// <summary>
    /// Gets the PedHistory database identifier.
    /// </summary>
    public int PedDataId { get; internal set; }

    /// <summary>
    /// Gets the collection of licenses assigned to this ped.
    /// </summary>
    public List<PedLicense> Licenses { get; private set; } = new();

    /// <summary>
    /// Gets the collection of drugs found on this ped.
    /// </summary>
    public List<Drug> Drugs { get; private set; } = new();

    /// <summary>
    /// Sets the PedData identifier for this ped.
    /// </summary>
    /// <param name="pedData">The PedData entity containing the database identifier.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithPedData(PedData pedData)
    {
        this.PedDataId = pedData.Id;
        return this;
    }

    /// <summary>
    /// Sets the network ID of the vehicle owned by this ped.
    /// </summary>
    /// <param name="vehicleNetworkId">The network identifier of the vehicle.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithOwningVehicleNetworkId(int vehicleNetworkId)
    {
        this.OwningVehicleNetworkId = vehicleNetworkId;
        return this;
    }

    /// <summary>
    /// Sets a specific first name for the ped.
    /// </summary>
    /// <param name="name">The first name to assign.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithFirstname(string name)
    {
        this.Firstname = name;
        return this;
    }

    /// <summary>
    /// Sets a random first name for the ped from the provided list.
    /// </summary>
    /// <param name="names">The list of possible first names to choose from.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithFirstname(List<string> names)
    {
        this.Firstname = names[this._randomService.Next(0, names.Count)].Trim();
        return this;
    }

    /// <summary>
    /// Sets a specific last name for the ped.
    /// </summary>
    /// <param name="name">The last name to assign.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithLastname(string name)
    {
        this.Lastname = name;
        return this;
    }

    /// <summary>
    /// Sets a random last name for the ped from the provided list.
    /// </summary>
    /// <param name="names">The list of possible last names to choose from.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithLastname(List<string> names)
    {
        this.Lastname = names[this._randomService.Next(0, names.Count)].Trim();
        return this;
    }

    /// <summary>
    /// Sets a random birthdate for the ped between 1960 and 2004.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithBirthdate()
    {
        this.Birthdate = DateGenerator.GetRandomDate(1960, 2004);
        return this;
    }

    /// <summary>
    /// Sets a specific birthdate for the ped.
    /// </summary>
    /// <param name="birthdate">The birthdate to assign.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithBirthdate(DateTime birthdate)
    {
        this.Birthdate = birthdate;
        return this;
    }

    /// <summary>
    /// Selects and assigns a random set of items to the ped.
    /// </summary>
    /// <param name="items">The collection of possible items to choose from.</param>
    /// <param name="includeAll">When true, includes all items. When false, selects a random subset (0-5 items).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithItems(IEnumerable<Item> items, bool includeAll = false)
    {
        var filteredItems = items.Where(item => item.Location.ToUpper() == "PED" || item.Location.ToUpper() == "ALL");
        var selectedItems = filteredItems.TakeRandomSubset(includeAll, 5).ToList();
        this.Items.AddRange(selectedItems);
        return this;
    }

    /// <summary>
    /// Assigns warrants to the ped based on the specified configuration options.
    /// </summary>
    /// <param name="options">Configuration options controlling warrant assignment.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithWarrants(WithWarrantsOptions options)
    {
        this.Warrants ??= new();

        if (options.IncludeAll)
        {
            this.Warrants.AddRange(options.Warrants);
            return this;
        }

        if (this._randomService.GetRandomChance > options.ChanceForAnyWarrant)
        {
            return this;
        }

        var maxWarrants = options.Warrants.Count < options.MaxWarrants ? options.Warrants.Count : options.MaxWarrants;
        var warrants = options.Warrants
            .OrderBy(_ => Guid.NewGuid())
            .Take(this._randomService.Next(0, maxWarrants))
            .ToList();

        this.Warrants.AddRange(warrants);

        return this;
    }

    /// <summary>
    /// Adds a single license to the ped if they don't already have a license of the same type.
    /// </summary>
    /// <param name="pedLicense">The license to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithLicense(PedLicense pedLicense)
    {
        if (this.Licenses.Any(x => x.Type == pedLicense.Type))
        {
            return this;
        }

        this.Licenses.Add(pedLicense);

        return this;
    }

    /// <summary>
    /// Adds multiple licenses to the ped, skipping any license types the ped already has.
    /// </summary>
    /// <param name="licenses">The collection of licenses to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithLicenses(IEnumerable<PedLicense> licenses)
    {
        foreach (var license in licenses)
        {
            if (this.Licenses.Any(x => x.Type == license.Type))
            {
                continue;
            }

            this.Licenses.Add(license);
        }

        return this;
    }

    /// <summary>
    /// Sets a specific alcohol level for the ped.
    /// </summary>
    /// <param name="alcoholLevel">The alcohol level to set (blood alcohol concentration).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithAlcoholLevel(double alcoholLevel)
    {
        this.AlcoholLevel = alcoholLevel;
        return this;
    }

    /// <summary>
    /// Sets a random alcohol level for the ped based on the provided configuration.
    /// </summary>
    /// <param name="alcoholConfig">Configuration defining alcohol level probabilities and limits.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithAlcoholLevel(AlcoholConfig alcoholConfig)
    {
        if (this._randomService.GetRandomChance > alcoholConfig.ChanceToHave)
        {
            return this;
        }

        if (this._randomService.GetRandomChance <= alcoholConfig.ChanceForOverLegalLimit)
        {
            var minValue = alcoholConfig.LegalLimit;
            const double maxValue = 0.2;

            this.AlcoholLevel = (this._randomService.Random.NextDouble() * (maxValue - minValue)) + minValue;
        }

        return this;
    }

    /// <summary>
    /// Adds a collection of drugs to the ped's inventory.
    /// </summary>
    /// <param name="drugs">The drugs to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithDrugs(IEnumerable<Drug> drugs)
    {
        this.Drugs ??= new();
        this.Drugs.AddRange(drugs ?? new List<Drug>());
        return this;
    }

    /// <summary>
    /// Randomly adds drugs to the ped based on the provided configuration.
    /// </summary>
    /// <param name="drugsConfig">The configuration defining drug probabilities.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FPedBuilder WithDrugs(List<DrugConfig> drugsConfig)
    {
        this.Drugs ??= new();

        foreach (var drug in drugsConfig.Where(drug => this._randomService.GetRandomChance <= drug.ChanceToHave))
        {
            this.Drugs.Add(new Drug
            {
                Name = drug.Name,
            });
        }

        return this;
    }

    /// <summary>
    /// Randomly assigns licenses to the ped based on the provided configuration.
    /// Supports four license statuses: Valid, Expired, Suspended, and Revoked.
    /// </summary>
    /// <param name="pedLicensesConfig">The configuration defining license types and their probabilities.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Status determination uses cumulative probability logic:
    /// - ChanceForExpired, ChanceForSuspended, and ChanceForRevoked are cumulative
    /// - The first matching status in the sequence is assigned
    /// - If no status matches, the license is Valid.
    /// </remarks>
    public FPedBuilder WithRandomLicenses(PedLicensesConfig pedLicensesConfig)
    {
        var licenses = new List<PedLicense>();

        var licenseConfigs = new Dictionary<PedLicenseType, PedLicensesConfig.PedLicenseConfig>
        {
            [PedLicenseType.Driving] = pedLicensesConfig.Driving,
            [PedLicenseType.Fishing] = pedLicensesConfig.Fishing,
            [PedLicenseType.Hunting] = pedLicensesConfig.Hunting,
            [PedLicenseType.Ccw] = pedLicensesConfig.Ccw,
        };

        foreach (var config in licenseConfigs)
        {
            if (this._randomService.GetRandomChance >= config.Value.ChanceToHave)
            {
                continue;
            }

            var probabilities = new List<(int Probability, PedLicenseStatus Outcome)>
            {
                (config.Value.ChanceForExpired, PedLicenseStatus.Expired),
                (config.Value.ChanceForSuspended, PedLicenseStatus.Suspended),
                (config.Value.ChanceForRevoked, PedLicenseStatus.Revoked),
                (100 - config.Value.ChanceForExpired - config.Value.ChanceForSuspended - config.Value.ChanceForRevoked, PedLicenseStatus.Valid),
            };
            var status = this._probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

            var isExpired = status == PedLicenseStatus.Expired;
            var (issueDate, expirationDate) = DateGenerator.GeneratePedLicenseAndVehicleDocumentDates(isExpired);

            licenses.Add(new PedLicense
            {
                Type = config.Key,
                IssueDate = issueDate,
                ExpirationDate = expirationDate,
                Status = status,
            });
        }

        this.WithLicenses(licenses);

        return this;
    }

    /// <summary>
    /// Creates a new <see cref="FPed"/> instance using all configured properties.
    /// </summary>
    /// <returns>A new FPed instance with all assigned properties.</returns>
    public FPed Build()
    {
        return new FPed(this);
    }
}