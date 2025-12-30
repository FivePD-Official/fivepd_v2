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
/// Repository for PedData CRUD operations.
/// </summary>
public class PedDataRepository : BaseRepository<PedData, PedDataUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PedDataRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public PedDataRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
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

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Gender), update.Gender);
        builder.Add(nameof(update.Firstname), update.Firstname);
        builder.Add(nameof(update.Lastname), update.Lastname);
        builder.Add(nameof(update.Birthdate), update.Birthdate);
        builder.Add(nameof(update.AlcoholLevel), update.AlcoholLevel);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, pedData.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Finds a ped data by its ID, including related licenses based on the specified options.
    /// </summary>
    /// <param name="id">The ped data ID.</param>
    /// <param name="options">The options for loading related entities.</param>
    /// <returns>The ped data with requested related entities if found, null otherwise.</returns>
    public PedData FindByIdWithDynamicRelated(int id, PedDataLoadOptions options)
    {
        var builder = SqlBuilder
            .From($"{TableNames.PedData} pd")
            .Select("pd.*", typeof(PedData))
            .If(options.IncludeLicenses, b => b.Select("pl.*", typeof(PedLicense)).LeftJoin(TableNames.PedLicenses, "pl", "pd.Id = pl.PedDataId"))
            .Where("pd.Id = @Id", new { Id = id });

        var result = builder.Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;

        var pedData = connection.Query(
            result.Sql,
            result.Types,
            objArray =>
            {
                var pedData = (PedData)objArray[0];
                var index = 1;
                if (options.IncludeLicenses)
                {
                    pedData.Licenses = objArray.Skip(index).OfType<PedLicense>().ToList();
                }

                return pedData;
            },
            result.Parameters,
            splitOn: result.SplitOn).FirstOrDefault();
        // var b = SqlBuilder
        //     .From(TableNames.VehicleData)
        //     .Where("PedDataId = @PedDataId", new {PedDataId = id })
        //     .Build();
        // var vehicleDatas = connection.Query<VehicleData>(b.Sql, b.Parameters).ToList();
        // pedData.VehicleData = vehicleDatas;
        return pedData;
    }

    /// <summary>
    /// Adds a license to the specified ped data.
    /// </summary>
    /// <param name="pedData">The ped data to add the license to.</param>
    /// <param name="license">The license to add.</param>
    /// <returns>The added license with updated ID.</returns>
    public PedLicense AddLicense(PedData pedData, PedLicense license)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(
            $@"INSERT INTO {TableNames.PedLicenses} (Id, PedDataId, Type, Status, IssueDate, ExpirationDate)
            VALUES (@Id, @PedDataId, @Type, @Status, @IssueDate, @ExpirationDate);",
            new
            {
                Id = license.Id,
                PedDataId = pedData.Id,
                Type = license.Type,
                Status = license.Status,
                IssueDate = license.IssueDate,
                ExpirationDate = license.ExpirationDate
            });
        return license;
    }

    /// <summary>
    /// Removes a license from the specified ped data.
    /// </summary>
    /// <param name="pedData">The ped data to remove the license from.</param>
    /// <param name="licenseId">The ID of the license to remove.</param>
    public void RemoveLicense(PedData pedData, string licenseId)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(
            $@"DELETE FROM {TableNames.PedLicenses} 
            WHERE Id = @LicenseId AND PedDataId = @PedDataId",
            new { LicenseId = licenseId, PedDataId = pedData.Id });
    }

    /// <summary>
    /// Gets a license by its ID.
    /// </summary>
    /// <param name="licenseId">The ID of the license to get.</param>
    /// <returns>The license if found, null otherwise.</returns>
    public PedLicense GetLicense(string licenseId)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.QueryFirstOrDefault<PedLicense>(
            $@"SELECT * FROM {TableNames.PedLicenses} WHERE Id = @Id",
            new { Id = licenseId });
    }

    /// <summary>
    /// Gets all licenses for the specified ped data.
    /// </summary>
    /// <param name="pedData">The ped data to get licenses for.</param>
    /// <returns>All licenses for the ped data.</returns>
    public IEnumerable<PedLicense> GetLicenses(PedData pedData)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<PedLicense>(
            $@"SELECT * FROM {TableNames.PedLicenses} WHERE PedDataId = @PedDataId",
            new { PedDataId = pedData.Id });
    }

    /// <summary>
    /// Options for loading related entities for a ped data.
    /// </summary>
    public class PedDataLoadOptions
    {
        public bool IncludeLicenses { get; set; }

        public bool IncludeVehicleDatas { get; set; }
    }
}