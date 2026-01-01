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

        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, this.IdColumn, vehicleData.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Retrieves a single <see cref="VehicleData"/> record by its identifier, including dynamically loaded related data based on specified options.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle data to retrieve.</param>
    /// <param name="options">Specifies the dynamic load options for related data.</param>
    /// <returns>A <see cref="VehicleData"/> instance if found; otherwise, null.</returns>
    public VehicleData FindByIdWithDynamicRelated(int id, VehicleDataLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, id).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves all vehicle data records with dynamically loaded related data based on the specified options.
    /// </summary>
    /// <param name="options">The options for loading related data, such as including documents and/or associated pedestrian data.</param>
    /// <returns>A list of <see cref="VehicleData"/> instances with related data included as specified by the options.</returns>
    public List<VehicleData> GetAllWithDynamicRelated(VehicleDataLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, 0);
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
                document.ExpirationDate,
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
            .WhereEquals("Id", documentId)
            .WhereEquals("VehicleDataId", vehicleData.Id)
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
            .WhereEquals("Id", documentId)
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
            .WhereEquals("VehicleDataId", vehicleData.Id)
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<VehicleDocument>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Retrieves a list of vehicle data with optionally included related dynamic data.
    /// </summary>
    /// <param name="options">Specifies the related data to include, such as documents or pedestrian data.</param>
    /// <param name="id">The ID of the vehicle to filter the results. If 0 or less, no filtering by ID is applied.</param>
    /// <returns>A list of vehicle data with the related dynamic data included as specified in the options.</returns>
    private List<VehicleData> GetAllWithDynamicRelated(VehicleDataLoadOptions options, int id)
    {
        var builderResult = SqlBuilder
            .From($"{TableNames.VehicleData} vd")
            .Select("vd.*", typeof(VehicleData))
            .If(
                options.IncludeDocuments,
                b => b.Select("doc.*", typeof(VehicleDocument)).LeftJoin(TableNames.VehicleDocuments, "doc", "vd.Id = doc.VehicleDataId"))
            .If(options.IncludePedData, b => b.Select("pd.*", typeof(PedData)).LeftJoin(TableNames.PedData, "pd", "vd.PedDataId = pd.Id"))
            .If(id > 0, b => b.WhereEquals("vd.Id", id))
            .Build();

        return this.QueryMultiMap<VehicleData, int>(
            builderResult,
            (entity, rowReader) =>
            {
                var document = rowReader.Read<VehicleDocument>(options.IncludeDocuments);
                if (document is not null && !entity.HasDocument(document.Type))
                {
                    entity.Documents.Add(document);
                }

                var pedData = rowReader.Read<PedData>(options.IncludePedData);
                if (pedData is not null && entity.PedData is null)
                {
                    entity.PedData = pedData;
                }

                return entity;
            },
            x => x.Id);
    }

#pragma warning disable
    public class VehicleDataLoadOptions
    {
        public bool IncludeDocuments { get; set; }

        public bool IncludePedData { get; set; }
    }
#pragma warning restore
}