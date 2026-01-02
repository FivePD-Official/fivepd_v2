using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Common.Enums;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for PedData CRUD operations.
/// </summary>
public class PedDataRepository : BaseRepository<PedData, PedDataUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PedDataRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public PedDataRepository(IDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.PedData)
    {
    }

    /// <inheritdoc />
    public override PedData Add(PedData pedData)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                pedData.Gender,
                pedData.Firstname,
                pedData.Lastname,
                pedData.Birthdate,
                pedData.AlcoholLevel,
                pedData.Drugs,
                pedData.Items,
                pedData.Warrants,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        pedData.Id = id;
        return pedData;
    }

    /// <inheritdoc />
    public override void Update(PedData pedData, Action<PedDataUpdate> updateAction)
    {
        var update = new PedDataUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, this.IdColumn, pedData.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Retrieves a random <see cref="PedData"/> entry from the repository that matches the specified gender.
    /// </summary>
    /// <param name="gender">The gender to match when retrieving a random <see cref="PedData"/> entry.</param>
    /// <returns>A random <see cref="PedData"/> entry with the specified gender, or null if no match is found.</returns>
    public PedData GetRandomWithMatchingGender(FGender gender)
    {
        var b = this.BaseQueryBuilder().OrderByRandom().WhereEquals("pd.Gender", gender);
        return this.GetAllWithDynamicRelated(b, new PedDataLoadOptions { IncludeLicenses = true }, 0).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves a ped data entity by its ID, including dynamically related data based on the specified options.
    /// </summary>
    /// <param name="id">The unique identifier of the ped data entity to retrieve.</param>
    /// <param name="options">The data load options specifying which related data to include dynamically.</param>
    /// <returns>The ped data entity with the specified ID, including the dynamically related data, or null if not found.</returns>
    public PedData FindByIdWithDynamicRelated(int id, PedDataLoadOptions options)
    {
        var b = this.BaseQueryBuilder();
        return this.GetAllWithDynamicRelated(b, options, id).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves all <see cref="PedData"/> with dynamically loaded related data based on the specified options.
    /// </summary>
    /// <param name="options">The options specifying which related data to include during the retrieval.</param>
    /// <returns>A list of <see cref="PedData"/> with related data dynamically loaded based on the specified options.</returns>
    public List<PedData> GetAllWithDynamicRelated(PedDataLoadOptions options)
    {
        var b = this.BaseQueryBuilder();
        return this.GetAllWithDynamicRelated(b, options, 0);
    }

    /// <summary>
    /// Adds a license to the specified ped data.
    /// </summary>
    /// <param name="pedData">The ped data to add the license to.</param>
    /// <param name="license">The license to add.</param>
    /// <returns>The added license with updated ID.</returns>
    public PedLicense AddLicense(PedData pedData, PedLicense license)
    {
        var builderResult = SqlBuilder.Into(TableNames.PedLicenses)
            .Insert(new
            {
                license.Id,
                PedDataId = pedData.Id,
                license.Type,
                license.Status,
                license.IssueDate,
                license.ExpirationDate,
            })
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
        return license;
    }

    /// <summary>
    /// Removes a license from the specified ped data.
    /// </summary>
    /// <param name="pedData">The ped data to remove the license from.</param>
    /// <param name="licenseId">The ID of the license to remove.</param>
    public void RemoveLicense(PedData pedData, string licenseId)
    {
        var builderResult = SqlBuilder.From(TableNames.PedLicenses)
            .Delete()
            .WhereEquals("Id", licenseId)
            .WhereEquals("PedDataId", pedData.Id)
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets a license by its ID.
    /// </summary>
    /// <param name="licenseId">The ID of the license to get.</param>
    /// <returns>The license if found, null otherwise.</returns>
    public PedLicense GetLicense(string licenseId)
    {
        var builderResult = SqlBuilder.From(TableNames.PedLicenses)
            .WhereEquals("Id", licenseId)
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.QueryFirstOrDefault<PedLicense>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all licenses for the specified ped data.
    /// </summary>
    /// <param name="pedData">The ped data to get licenses for.</param>
    /// <returns>All licenses for the ped data.</returns>
    public IEnumerable<PedLicense> GetLicenses(PedData pedData)
    {
        var builderResult = SqlBuilder.From(TableNames.PedLicenses)
            .WhereEquals("PedDataId", pedData.Id)
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<PedLicense>(builderResult.Sql, builderResult.Parameters);
    }

    private IQueryBuilder BaseQueryBuilder()
    {
        return SqlBuilder
            .From($"{TableNames.PedData} pd")
            .Select("pd.*", typeof(PedData));
    }

    /// <summary>
    /// Retrieves a list of PedData records, along with dynamically loaded related data, based on the specified query builder, load options, and identifier.
    /// </summary>
    /// <param name="builder">The query builder used to define the base query structure.</param>
    /// <param name="options">The options specifying which related data to include (e.g., licenses, vehicle data).</param>
    /// <param name="id">The unique identifier of the record to retrieve. Use 0 to retrieve all records.</param>
    /// <returns>A list of <see cref="PedData"/> objects with the specified related data included.</returns>
    private List<PedData> GetAllWithDynamicRelated(IQueryBuilder builder, PedDataLoadOptions options, int id)
    {
        builder
            .If(options.IncludeLicenses, b => b.Select("pl.*", typeof(PedLicense)).LeftJoin(TableNames.PedLicenses, "pl", "pd.Id = pl.PedDataId"))
            .If(options.IncludeVehicleData, b => b.Select("v.*", typeof(VehicleData)).LeftJoin(TableNames.VehicleData, "v", "pd.Id = v.PedDataId"))
            .If(id > 0, b => b.WhereEquals("pd.Id", id));

        var builderResult = builder.Build();
        return this.QueryMultiMap<PedData, int>(
            builderResult,
            (entity, rowReader) =>
            {
                var license = rowReader.Read<PedLicense>(options.IncludeLicenses);
                if (license is not null && !entity.HasLicense(license.Type))
                {
                    entity.Licenses.Add(license);
                }

                var vehicleData = rowReader.Read<VehicleData>(options.IncludeVehicleData);
                if (vehicleData is not null && entity.VehicleData.All(x => x.Id != vehicleData.Id))
                {
                    entity.VehicleData.Add(vehicleData);
                }

                return entity;
            },
            x => x.Id);
    }

#pragma warning disable
    public class PedDataLoadOptions
    {
        public bool IncludeLicenses { get; set; }

        public bool IncludeVehicleData { get; set; }
    }
#pragma warning restore
}