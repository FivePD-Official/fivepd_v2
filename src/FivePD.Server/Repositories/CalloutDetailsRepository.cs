using System;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for CalloutDetails CRUD operations.
/// </summary>
public class CalloutDetailsRepository : BaseRepository<CalloutDetails, CalloutDetailsUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutDetailsRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public CalloutDetailsRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.CalloutDetails)
    {
    }

    /// <inheritdoc />
    public override CalloutDetails Add(CalloutDetails calloutDetails)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                calloutDetails.Active,
                calloutDetails.TypeName,
                calloutDetails.AssemblyName,
                calloutDetails.RulesetType,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        calloutDetails.Id = id;
        return calloutDetails;
    }

    /// <inheritdoc />
    public override void Update(CalloutDetails calloutDetails, Action<CalloutDetailsUpdate> updateAction)
    {
        var update = new CalloutDetailsUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Active), update.Active);
        builder.Add(nameof(update.TypeName), update.TypeName);
        builder.Add(nameof(update.AssemblyName), update.AssemblyName);
        builder.Add(nameof(update.RulesetType), update.RulesetType);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, calloutDetails.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }
}