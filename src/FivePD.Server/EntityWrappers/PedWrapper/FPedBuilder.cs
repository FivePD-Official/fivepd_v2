using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Common.Builders;
using FivePD.Common.ConfigModels;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.Models;
using FivePD.Server.Models;
using FivePD.Server.Utilities;

namespace FivePD.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Builder class for creating FPed instances with configurable properties.
/// </summary>
public class FPedBuilder : CommonFPedBuilder
{
#pragma warning disable CS1574, CS1584, CS1581, CS1580
#pragma warning disable SA1313
    /// <summary>
    /// Options for the <see cref="CommonFPedBuilder.WithWarrants(WithWarrantsOptions)"/> method.
    /// </summary>
    /// <param name="Warrants">The warrants to use.</param>
    /// <param name="IncludeAll">
    /// Specifies whether to include all warrants. It should be true only when ped history is loaded.
    /// Does not respect <see cref="ChanceForAnyWarrant"/> and <see cref="MaxWarrants"/>.
    /// </param>
    /// <param name="ChanceForAnyWarrant">The chance for any warrant to be included.</param>
    /// <param name="MaxWarrants">The maximum number of warrants to include.</param>
    public readonly record struct WithWarrantsOptions(List<string> Warrants, bool IncludeAll, int ChanceForAnyWarrant, int MaxWarrants);
#pragma warning restore SA1313
#pragma warning restore CS1574, CS1584, CS1581, CS1580

    /// <summary>
    /// Initializes a new instance of the <see cref="FPedBuilder"/> class.
    /// </summary>
    /// <param name="fPedBuilder">The base builder to initialize from.</param>
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
    /// Gets the PedHistory id.
    /// </summary>
    public int PedDataId { get; internal set; }

    /// <summary>
    /// Gets the licenses of the ped.
    /// </summary>
    public List<PedLicense> Licenses { get; private set; } = new();

    /// <summary>
    /// Gets the drugs of the ped.
    /// </summary>
    public List<Drug> Drugs { get; private set; } = new();

    /// <summary>
    /// Sets the PedData for this ped.
    /// </summary>
    /// <param name="pedData">The PedData to associate with this ped.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithPedData(PedData pedData)
    {
        this.PedDataId = pedData.Id;
        return this;
    }

    /// <summary>
    /// Sets the id of the vehicle the ped is owning.
    /// </summary>
    /// <param name="vehicleNetworkId">The network ID of the vehicle to assign.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithOwningVehicleNetworkId(int vehicleNetworkId)
    {
        this.OwningVehicleNetworkId = vehicleNetworkId;
        return this;
    }

    /// <summary>
    /// Sets the firstname of the ped.
    /// </summary>
    /// <param name="name">The first name to set.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithFirstname(string name)
    {
        this.Firstname = name;
        return this;
    }

    /// <summary>
    /// Sets a random firstname for the ped from a provided list.
    /// </summary>
    /// <param name="names">The list of possible first names to choose from.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithFirstname(List<string> names)
    {
        this.Firstname = names[Random.Next(names.Count)].Trim();
        return this;
    }

    /// <summary>
    /// Sets the lastname of the ped.
    /// </summary>
    /// <param name="name">The last name to set.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithLastname(string name)
    {
        this.Lastname = name;
        return this;
    }

    /// <summary>
    /// Sets a random lastname for the ped from a provided list.
    /// </summary>
    /// <param name="names">The list of possible last names to choose from.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithLastname(List<string> names)
    {
        this.Lastname = names[Random.Next(names.Count)].Trim();
        return this;
    }

    /// <summary>
    /// Sets a random birthdate for the ped between 1960 and 2004.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithBirthdate()
    {
        this.Birthdate = DateGenerator.GetRandomDate(1960, 2004);
        return this;
    }

    /// <summary>
    /// Sets the birthdate of the ped.
    /// </summary>
    /// <param name="birthdate">The birthdate to set.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithBirthdate(DateTime birthdate)
    {
        this.Birthdate = birthdate;
        return this;
    }

    /// <summary>
    /// Selects and sets a random set of items for the ped.
    /// </summary>
    /// <param name="items">The collection of possible items to choose from.</param>
    /// <param name="includeAll">When true, includes all items. When false, selects a random subset.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithItems(IEnumerable<Item> items, bool includeAll = false)
    {
        var filteredItems = items.Where(item => item.Location.ToUpper() == "PED" || item.Location.ToUpper() == "ALL");

        if (!includeAll)
        {
            filteredItems = filteredItems
                .OrderBy(_ => Guid.NewGuid())
                .Take(Random.Next(0, 6));
        }

        this.Items.AddRange(filteredItems);
        return this;
    }

    /// <summary>
    /// Sets the warrants for the ped according to the specified options.
    /// </summary>
    /// <param name="options">Configuration options for warrant assignment.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithWarrants(WithWarrantsOptions options)
    {
        this.Warrants ??= new();

        if (options.IncludeAll)
        {
            this.Warrants.AddRange(options.Warrants);
            return this;
        }

        if (Random.Next(0, 101) > options.ChanceForAnyWarrant)
        {
            return this;
        }

        var maxWarrants = options.Warrants.Count < options.MaxWarrants ? options.Warrants.Count : options.MaxWarrants;
        var warrants = options.Warrants
            .OrderBy(_ => Guid.NewGuid())
            .Take(Random.Next(0, maxWarrants))
            .ToList();

        this.Warrants.AddRange(warrants);

        return this;
    }

    /// <summary>
    /// Adds a single license to the ped if they don't already have one of the same type.
    /// </summary>
    /// <param name="pedLicense">The license to add.</param>
    /// <returns>The current builder instance.</returns>
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
    /// <returns>The current builder instance.</returns>
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
    /// <param name="alcoholLevel">The alcohol level to set.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithAlcoholLevel(double alcoholLevel)
    {
        this.AlcoholLevel = alcoholLevel;
        return this;
    }

    /// <summary>
    /// Sets a random alcohol level for the ped based on the provided configuration.
    /// </summary>
    /// <param name="alcoholConfig">The configuration defining alcohol level probabilities and limits.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithAlcoholLevel(AlcoholConfig alcoholConfig)
    {
        if (Random.Next(0, 100) > alcoholConfig.ChanceToHave)
        {
            return this;
        }

        if (Random.Next(0, 100) <= alcoholConfig.ChanceForOverLegalLimit)
        {
            var minValue = alcoholConfig.LegalLimit;
            const double maxValue = 0.2;

            this.AlcoholLevel = (Random.NextDouble() * (maxValue - minValue)) + minValue;
        }

        return this;
    }

    /// <summary>
    /// Adds a collection of drugs to the ped's inventory.
    /// </summary>
    /// <param name="drugs">The drugs to add.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithDrugs(IEnumerable<Drug> drugs)
    {
        this.Drugs ??= new();
        this.Drugs.AddRange(drugs ?? new List<Drug>());
        return this;
    }

    /// <summary>
    /// Randomly adds drugs to the ped based on the provided configurations.
    /// </summary>
    /// <param name="drugsConfig">The configuration defining drug probabilities.</param>
    /// <returns>The current builder instance.</returns>
    public FPedBuilder WithDrugs(List<DrugConfig> drugsConfig)
    {
        this.Drugs ??= new();

        foreach (var drug in drugsConfig.Where(drug => Random.Next(0, 100) <= drug.ChanceToHave))
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
    /// </summary>
    /// <param name="pedLicensesConfig">The configuration defining license types and probabilities.</param>
    /// <returns>The current builder instance.</returns>
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
            if (Random.Next(0, 100) >= config.Value.ChanceToHave)
            {
                continue;
            }

            var now = DateTime.Now;
            var expired = Random.Next(0, 100) < config.Value.ChanceForExpired;
            DateTime issueDate;
            DateTime expirationDate;
            if (expired)
            {
                issueDate = DateGenerator.GetRandomDate(now.Year - 10, now.Year - 1);
                expirationDate = issueDate.AddYears(Random.Next(1, Math.Max(2, now.Year - issueDate.Year)));
                if (expirationDate >= now)
                {
                    expirationDate = now.AddDays(-Random.Next(1, 365));
                }
            }
            else
            {
                issueDate = DateGenerator.GetRandomDate(now.Year - 5, now.Year);
                expirationDate = issueDate.AddYears(Random.Next(1, 11));
                if (expirationDate <= now)
                {
                    expirationDate = now.AddYears(Random.Next(1, 11));
                }
            }

            licenses.Add(new PedLicense
            {
                Type = config.Key,
                IssueDate = issueDate,
                ExpirationDate = expirationDate,
                Status = expired ? PedLicenseStatus.Expired : PedLicenseStatus.Valid,
            });
        }

        this.WithLicenses(licenses);

        return this;
    }

    /// <summary>
    /// Creates a new <see cref="FPed"/> instance using the configured properties.
    /// </summary>
    /// <returns>A new FPed instance.</returns>
    public FPed Build()
    {
        return new FPed(this);
    }
}