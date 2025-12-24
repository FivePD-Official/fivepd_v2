using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Common.Builders;
using FivePD.Common.ConfigModels;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Server.Models;
using FivePD.Server.Utilities;

namespace FivePD.Server.EntityWrappers.VehicleWrapper;

#pragma warning disable

public class FVehicleBuilder : CommonFVehicleBuilder
{
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
    /// Gets the VehicleHistory id.
    /// </summary>
    public int VehicleDataId { get; internal set; }

    /// <summary>
    /// Gets the PedHistory id.
    /// </summary>
    public int OwnerPedHistoryId { get; internal set; }

    /// <summary>
    /// Gets the documents of the vehicle.
    /// </summary>
    public List<VehicleDocument> Documents { get; } = new();

    public FVehicleBuilder WithVehicleData(VehicleData vehicleData)
    {
        this.VehicleDataId = vehicleData.Id;
        return this;
    }

    public FVehicleBuilder WithPedHistory(PedData pedData)
    {
        this.OwnerPedHistoryId = pedData?.Id ?? 0;
        return this;
    }

    public FVehicleBuilder WithItems(IEnumerable<Item> items)
    {
        this.Items = items
            .Where(item => item.Location.ToUpper() == "VEHICLE" || item.Location.ToUpper() == "ALL")
            .OrderBy(_ => Guid.NewGuid())
            .Take(Random.Next(0, 6))
            .ToList();
        return this;
    }

    public FVehicleBuilder WithDocument(VehicleDocument document)
    {
        this.Documents.Add(document);
        return this;
    }

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

            documents.Add(new VehicleDocument
            {
                Type = config.Key,
                IssueDate = issueDate,
                ExpirationDate = expirationDate,
                Status = expired ? VehicleDocumentStatus.Expired : VehicleDocumentStatus.Valid,
            });
        }

        this.WithDocuments(documents);

        return this;
    }

    public FVehicle Build()
    {
        return new FVehicle(this);
    }
}