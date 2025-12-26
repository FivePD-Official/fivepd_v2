using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Common.Builders;
using FivePD.Common.ConfigModels;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Utilities;

namespace FivePD.Server.EntityWrappers.VehicleWrapper;

/// <summary>
/// Builder class for creating FVehicle instances with configurable properties.
/// Provides a fluent interface for setting up vehicle characteristics, documents, and other attributes.
/// </summary>
public class FVehicleBuilder : CommonFVehicleBuilder
{
    private readonly IRandomService _randomService = ServiceLocator.GetService<IRandomService>();
    private readonly IProbabilityService _probabilityService = ServiceLocator.GetService<IProbabilityService>();

    /// <summary>
    /// Initializes a new instance of the <see cref="FVehicleBuilder"/> class.
    /// </summary>
    /// <param name="fPedBuilder">The base CommonFVehicleBuilder to initialize from.</param>
    public FVehicleBuilder(CommonFVehicleBuilder fPedBuilder)
        : base(fPedBuilder.NetworkId)
    {
        this.OwnerNetworkId = fPedBuilder.OwnerNetworkId;
        this.PassengerNetworkIds = fPedBuilder.PassengerNetworkIds;
        this.LicensePlate = fPedBuilder.LicensePlate;
        this.Color = fPedBuilder.Color;
        this.Brand = fPedBuilder.Brand;
        this.Items = fPedBuilder.Items;
    }

    /// <summary>
    /// Gets the VehicleHistory database identifier.
    /// </summary>
    public int VehicleDataId { get; internal set; }

    /// <summary>
    /// Gets the PedHistory identifier of the vehicle's owner.
    /// </summary>
    public int OwnerPedHistoryId { get; internal set; }

    /// <summary>
    /// Gets the collection of documents assigned to this vehicle.
    /// </summary>
    public List<VehicleDocument> Documents { get; } = new();

    /// <summary>
    /// Sets the VehicleData identifier for this vehicle.
    /// </summary>
    /// <param name="vehicleData">The VehicleData entity containing the database identifier.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FVehicleBuilder WithVehicleData(VehicleData vehicleData)
    {
        this.VehicleDataId = vehicleData.Id;
        return this;
    }

    /// <summary>
    /// Sets the PedHistory identifier for the vehicle's owner.
    /// </summary>
    /// <param name="pedData">The PedData entity containing the owner's history identifier.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FVehicleBuilder WithPedHistory(PedData pedData)
    {
        this.OwnerPedHistoryId = pedData?.Id ?? 0;
        return this;
    }

    /// <summary>
    /// Assigns a random set of items to the vehicle.
    /// </summary>
    /// <param name="items">The collection of possible items to choose from.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FVehicleBuilder WithItems(IEnumerable<Item> items)
    {
        var filteredItems = items.Where(item => item.Location.ToUpper() == "VEHICLE" || item.Location.ToUpper() == "ALL");
        this.Items = filteredItems.TakeRandomSubset(false, 5).ToList();
        return this;
    }

    /// <summary>
    /// Adds a single document to the vehicle.
    /// </summary>
    /// <param name="document">The document to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FVehicleBuilder WithDocument(VehicleDocument document)
    {
        this.Documents.Add(document);
        return this;
    }

    /// <summary>
    /// Adds multiple documents to the vehicle, skipping any document types the vehicle already has.
    /// </summary>
    /// <param name="documents">The collection of documents to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FVehicleBuilder WithDocuments(IEnumerable<VehicleDocument> documents)
    {
        foreach (var license in documents)
        {
            if (this.Documents.Any(x => x.Type == license.Type))
            {
                continue;
            }

            this.Documents.Add(license);
        }

        return this;
    }

    /// <summary>
    /// Randomly assigns documents to the vehicle based on the provided configuration.
    /// Supports four document statuses: Valid, Expired, Suspended, and Revoked.
    /// </summary>
    /// <param name="vehicleDocumentsConfig">The configuration defining document types and their probabilities.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Status determination uses cumulative probability logic:
    /// - ChanceForExpired, ChanceForSuspended, and ChanceForRevoked are cumulative
    /// - The first matching status in the sequence is assigned
    /// - If no status matches, the document is Valid.
    /// </remarks>
    public FVehicleBuilder WithRandomDocuments(VehicleDocumentsConfig vehicleDocumentsConfig)
    {
        var documents = new List<VehicleDocument>();

        var documentConfigs = new Dictionary<VehicleDocumentType, VehicleDocumentsConfig.VehicleDocumentConfig>
        {
            [VehicleDocumentType.Registration] = vehicleDocumentsConfig.Registration,
            [VehicleDocumentType.Insurance] = vehicleDocumentsConfig.Insurance,
        };

        foreach (var config in documentConfigs)
        {
            if (this._randomService.GetRandomChance >= config.Value.ChanceToHave)
            {
                continue;
            }

            var probabilities = new List<(int Probability, VehicleDocumentStatus Outcome)>
            {
                (config.Value.ChanceForExpired, VehicleDocumentStatus.Expired),
                (config.Value.ChanceForSuspended, VehicleDocumentStatus.Suspended),
                (config.Value.ChanceForRevoked, VehicleDocumentStatus.Revoked),
                (100 - config.Value.ChanceForExpired - config.Value.ChanceForSuspended - config.Value.ChanceForRevoked, VehicleDocumentStatus.Valid),
            };
            var status = this._probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

            var isExpired = status == VehicleDocumentStatus.Expired;
            var (issueDate, expirationDate) = DateGenerator.GeneratePedLicenseAndVehicleDocumentDates(isExpired);

            documents.Add(new VehicleDocument
            {
                Type = config.Key,
                IssueDate = issueDate,
                ExpirationDate = expirationDate,
                Status = status,
            });
        }

        this.WithDocuments(documents);

        return this;
    }

    /// <summary>
    /// Creates a new <see cref="FVehicle"/> instance using all configured properties.
    /// </summary>
    /// <returns>A new FVehicle instance with all assigned properties.</returns>
    public new FVehicle Build()
    {
        return new FVehicle(this);
    }
}