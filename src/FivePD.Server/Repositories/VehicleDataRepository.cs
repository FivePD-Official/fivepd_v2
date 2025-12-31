using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

#pragma warning disable

/// <summary>
/// Repository for VehicleData CRUD operations.
/// </summary>
public class VehicleDataRepository : BaseRepository<VehicleData, VehicleDataUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleDataRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public VehicleDataRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.VehicleData)
    {
    }

    /// <inheritdoc />
    public override VehicleData Add(VehicleData vehicleData)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                vehicleData.PedDataId,
                vehicleData.LicensePlate,
                vehicleData.Color,
                vehicleData.Brand,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        vehicleData.Id = id;
        return vehicleData;
    }

    /// <inheritdoc />
    public override void Update(VehicleData vehicleData, Action<VehicleDataUpdate> updateAction)
    {
        var update = new VehicleDataUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.PedDataId), update.PedDataId);
        builder.Add(nameof(update.LicensePlate), update.LicensePlate);
        builder.Add(nameof(update.Color), update.Color);
        builder.Add(nameof(update.Brand), update.Brand);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, vehicleData.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Finds a vehicle data by its ID, including related documents based on the specified options.
    /// </summary>
    /// <param name="id">The vehicle data ID.</param>
    /// <param name="options">The options for loading related entities.</param>
    /// <returns>The vehicle data with requested related entities if found, null otherwise.</returns>
    public VehicleData FindByIdWithDynamicRelated(int id, VehicleDataLoadOptions options)
    {
        var builder = SqlBuilder
            .From($"{TableNames.VehicleData} vd")
            .Select("vd.*", typeof(VehicleData))
            .If(options.IncludeDocuments,
                b => b.Select("doc.*", typeof(VehicleDocument)).LeftJoin(TableNames.VehicleDocuments, "doc", "vd.Id = doc.VehicleDataId"))
            .If(options.IncludePedData, b => b.Select("pd.*", typeof(PedData)).LeftJoin(TableNames.PedData, "pd", "vd.PedDataId = pd.Id"))
            .Where("vd.Id = @Id", new { Id = id });

        var result = builder.Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;

        return connection.Query(
            result.Sql,
            result.Types,
            objArray =>
            {
                var vehicleData = (VehicleData)objArray[0];
                var index = 1;
                if (options.IncludeDocuments)
                {
                    vehicleData.Documents = objArray[index++] as List<VehicleDocument>;
                }

                if (options.IncludePedData)
                {
                    vehicleData.PedData = objArray[index] as PedData;
                }

                return vehicleData;
            },
            result.Parameters,
            splitOn: result.SplitOn).FirstOrDefault();
    }

    /// <summary>
    /// Adds a document to the specified vehicle data.
    /// </summary>
    /// <param name="vehicleData">The vehicle data to add the document to.</param>
    /// <param name="document">The document to add.</param>
    /// <returns>The added document with updated ID.</returns>
    public VehicleDocument AddDocument(VehicleData vehicleData, VehicleDocument document)
    {
        var builderResult = SqlBuilder.Into(TableNames.VehicleDocuments)
            .Insert(new
            {
                document.Id,
                VehicleDataId = vehicleData.Id,
                document.Type,
                document.Status,
                document.IssueDate,
                document.ExpirationDate
            })
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
        return document;
    }

    /// <summary>
    /// Removes a document from the specified vehicle data.
    /// </summary>
    /// <param name="vehicleData">The vehicle data to remove the document from.</param>
    /// <param name="documentId">The ID of the document to remove.</param>
    public void RemoveDocument(VehicleData vehicleData, string documentId)
    {
        var builderResult = SqlBuilder.From(TableNames.VehicleDocuments)
            .Delete()
            .Where("Id = @DocumentId", new { DocumentId = documentId })
            .Where("VehicleDataId = @VehicleDataId", new { VehicleDataId = vehicleData.Id })
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets a document by its ID.
    /// </summary>
    /// <param name="documentId">The ID of the document to get.</param>
    /// <returns>The document if found, null otherwise.</returns>
    public VehicleDocument GetDocument(string documentId)
    {
        var builderResult = SqlBuilder.From(TableNames.VehicleDocuments)
            .Where("Id = @Id", new { Id = documentId })
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.QueryFirstOrDefault<VehicleDocument>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all documents for the specified vehicle data.
    /// </summary>
    /// <param name="vehicleData">The vehicle data to get documents for.</param>
    /// <returns>All documents for the vehicle data.</returns>
    public IEnumerable<VehicleDocument> GetDocuments(VehicleData vehicleData)
    {
        var builderResult = SqlBuilder.From(TableNames.VehicleDocuments)
            .Where("VehicleDataId = @VehicleDataId", new { VehicleDataId = vehicleData.Id })
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<VehicleDocument>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Options for loading related entities for a vehicle data.
    /// </summary>
    public class VehicleDataLoadOptions
    {
        public bool IncludeDocuments { get; set; }

        public bool IncludePedData { get; set; }
    }
}