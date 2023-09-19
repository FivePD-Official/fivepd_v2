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

#pragma warning disable

public class FPedBuilder : CommonFPedBuilder
{
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
#pragma warning disable SA1313
    public readonly record struct WithWarrantsOptions(List<string> Warrants, bool IncludeAll, int ChanceForAnyWarrant, int MaxWarrants);
#pragma warning restore SA1313
    
    public FPedBuilder(CommonFPedBuilder fPedBuilder)
        : base(fPedBuilder.NetworkId)
    {
        this.Birthdate = fPedBuilder.Birthdate;
        this.Firstname = fPedBuilder.Firstname;
        this.Lastname = fPedBuilder.Lastname;
        this.Gender = fPedBuilder.Gender;
        this.Items = fPedBuilder.Items;
        this.Warrants = fPedBuilder.Warrants;
    }

    /// <summary>
    /// Gets the licenses of the ped.
    /// </summary>
    public List<PedLicense> Licenses { get; private set; } = new();

    /// <summary>
    /// Sets the firstname of the ped.
    /// </summary>
    /// <param name="name">The name to set.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public FPedBuilder WithFirstname(string name)
    {
        this.Firstname = name;
        return this;
    }

    /// <summary>
    /// Sets a random firstname for the ped.
    /// </summary>
    /// <param name="names">The names to choose from.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public FPedBuilder WithFirstname(List<string> names)
    {
        this.Firstname = names[Random.Next(names.Count)].Trim();
        return this;
    }

    /// <summary>
    /// Sets the lastname of the ped.
    /// </summary>
    /// <param name="name">The name to set.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public FPedBuilder WithLastname(string name)
    {
        this.Lastname = name;
        return this;
    }

    /// <summary>
    /// Sets a random lastname for the ped.
    /// </summary>
    /// <param name="names">The names to choose from.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public FPedBuilder WithLastname(List<string> names)
    {
        this.Lastname = names[Random.Next(names.Count)].Trim();
        return this;
    }

    /// <summary>
    /// Sets a random birthdate for the ped.
    /// </summary>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public FPedBuilder WithBirthdate()
    {
        this.Birthdate = DateGenerator.GetRandomDate(1960, 2004);
        return this;
    }

    /// <summary>
    /// Sets the birthdate of the ped.
    /// </summary>
    /// <param name="birthdate">The birthdate to set.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public FPedBuilder WithBirthdate(DateTime birthdate)
    {
        this.Birthdate = birthdate;
        return this;
    }

    /// <summary>
    /// Selects and sets a random set of items for the ped.
    /// </summary>
    /// <param name="items">The items to choose from.</param>
    /// <param name="includeAll">Whether to include all specified items or select a random subset.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
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
    /// Sets the warrants according to the options for the ped.
    /// </summary>
    /// <param name="options">The options to use.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public FPedBuilder WithWarrants(WithWarrantsOptions options)
    {
        if (options.IncludeAll)
        {
            if (this.Warrants is null)
            {
                this.Warrants = new();
            }
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

        if (this.Warrants is null)
        {
            this.Warrants = new();
        }

        this.Warrants.AddRange(warrants);

        return this;
    }

    /// <summary>
    /// Adds a license to the ped.
    /// </summary>
    /// <param name="pedLicense">The license to add.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
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
    /// Adds multiple licenses to the ped.
    /// </summary>
    /// <param name="licenses">The licenses to add.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
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
    /// Adds multiple licenses to the ped.
    /// </summary>
    /// <param name="licensesConfig">The licenses config to use.</param>
    /// <returns>The <see cref="CommonFPedBuilder"/> instance.</returns>
    public FPedBuilder WithRandomLicenses(LicensesConfig licensesConfig)
    {
        var licenses = new List<PedLicense>();

        var asd = new Dictionary<PedLicenseType, LicensesConfig.LicenseConfig>
        {
            [PedLicenseType.Driving] = licensesConfig.Driving,
            [PedLicenseType.Fishing] = licensesConfig.Fishing,
            [PedLicenseType.Hunting] = licensesConfig.Hunting,
            [PedLicenseType.Ccw] = licensesConfig.Ccw,
        };

        foreach (var config in asd)
        {
            if (Random.Next(1, 101) >= config.Value.ChanceToHave)
            {
                continue;
            }

            var now = DateTime.Now;
            var issueDate = DateGenerator.GetRandomDate(now.Year - 10, now.Year);
            var expirationDate = Random.Next(1, 101) < config.Value.ChanceForExpired
                ? issueDate.AddYears(-Random.Next(1, 3))
                : issueDate.AddYears(Random.Next(1, 11));

            licenses.Add(new PedLicense
            {
                Type = config.Key,
                IssueDate = issueDate,
                ExpirationDate = expirationDate,
            });
        }

        this.WithLicenses(licenses);

        return this;
    }

    /// <summary>
    /// Initializes a new <see cref="CommonFPed"/> instance.
    /// </summary>
    /// <returns>Returns the newly created <see cref="CommonFPed"/> instance.</returns>
    public FPed Build()
    {
        return new FPed(this);
    }
}