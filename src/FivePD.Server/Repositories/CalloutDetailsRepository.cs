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

    /// <summary>
    /// Attaches a department to a callout details.
    /// </summary>
    /// <param name="calloutDetails">The callout details.</param>
    /// <param name="department">The department.</param>
    public void AttachDepartment(CalloutDetails calloutDetails, Department department)
    {
        var builderResult = SqlBuilder
            .Into(TableNames.CalloutDetailsDepartments)
            .Insert(new
            {
                CalloutDetailsId = calloutDetails.Id,
                DepartmentId = department.Id,
            })
            .OnDuplicateKeyUpdate("CalloutDetailsId", "DepartmentId")
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Removes a department from a callout details.
    /// </summary>
    /// <param name="calloutDetails">The callout details.</param>
    /// <param name="department">The department.</param>
    public void RemoveDepartment(CalloutDetails calloutDetails, Department department)
    {
        var builderResult = SqlBuilder
            .From(TableNames.CalloutDetailsDepartments)
            .Delete()
            .WhereEquals("CalloutDetailsId", calloutDetails.Id)
            .WhereEquals("DepartmentId", department.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all departments attached to a callout details.
    /// </summary>
    /// <param name="calloutDetails">The callout details.</param>
    /// <returns>List of departments.</returns>
    public IEnumerable<Department> GetDepartments(CalloutDetails calloutDetails)
    {
        var builderResult = SqlBuilder.From($"{TableNames.Departments} d")
            .Select("d.*", typeof(Department))
            .InnerJoin(TableNames.CalloutDetailsDepartments, "cdd", "d.Id = cdd.DepartmentId", "DepartmentId")
            .WhereEquals("cdd.CalloutDetailsId", calloutDetails.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<Department>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Checks if a callout details has a specific department attached.
    /// </summary>
    /// <param name="calloutDetails">The callout details.</param>
    /// <param name="department">The department.</param>
    /// <returns>True if the department is attached, false otherwise.</returns>
    public bool HasDepartment(CalloutDetails calloutDetails, Department department)
    {
        var builderResult = SqlBuilder
            .From(TableNames.CalloutDetailsDepartments)
            .WhereEquals("CalloutDetailsId", calloutDetails.Id)
            .WhereEquals("DepartmentId", department.Id)
            .BuildCount();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters) > 0;
    }

    /// <summary>
    /// Finds a callout detail by its ID and loads related data dynamically based on the specified options.
    /// </summary>
    /// <param name="id">The unique identifier of the callout detail to find.</param>
    /// <param name="options">The load options specifying which related data to include.</param>
    /// <returns>The callout detail with the specified ID, including related data based on the provided options, or null if not found.</returns>
    public CalloutDetails FindByIdWithDynamicRelated(int id, CalloutDetailsLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, id).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves all <see cref="CalloutDetails"/> objects with optional related entities based on the specified load options.
    /// </summary>
    /// <param name="options">The load options that determine which related entities to include.</param>
    /// <returns>A list of <see cref="CalloutDetails"/> objects, optionally including related entities based on the load options.</returns>
    public List<CalloutDetails> GetAllWithDynamicRelated(CalloutDetailsLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, 0);
    }

    private List<CalloutDetails> GetAllWithDynamicRelated(CalloutDetailsLoadOptions options, int id)
    {
        var result = SqlBuilder.From($"{this.TableName} c")
            .Select("c.*", typeof(CalloutDetails))
            .If(options.IncludeCalloutDetailsDepartments, wb => wb
                .Select("cd.*", typeof(CalloutDetailsDepartment))
                .LeftJoin(TableNames.CalloutDetailsDepartments, "cd", "c.Id = cd.DepartmentId", "DepartmentId"))
            .If(id > 0, b => b.Where("c.Id = @Id", new { Id = id }))
            .Build();

        var lookup = new Dictionary<int, CalloutDetails>();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Query(
            result.Sql,
            result.Types,
            objArray =>
            {
                var calloutDetails = objArray[0] as CalloutDetails;
                var index = 1;

                if (calloutDetails is null)
                {
                    return null;
                }

                if (!lookup.TryGetValue(calloutDetails.Id, out var currentCalloutDetails))
                {
                    currentCalloutDetails = calloutDetails;
                    if (options.IncludeCalloutDetailsDepartments)
                    {
                        currentCalloutDetails.CalloutDetailsDepartments = new List<CalloutDetailsDepartment>();
                    }

                    lookup.Add(currentCalloutDetails.Id, currentCalloutDetails);
                }

                if (options.IncludeCalloutDetailsDepartments)
                {
                    var detailsDepartment = objArray[index] as CalloutDetailsDepartment;
                    if (detailsDepartment is not null &&
                        currentCalloutDetails.CalloutDetailsDepartments.All(x => x.DepartmentId != detailsDepartment.DepartmentId))
                    {
                        currentCalloutDetails.CalloutDetailsDepartments.Add(detailsDepartment);
                    }
                }

                return calloutDetails;
            },
            result.Parameters,
            splitOn: result.SplitOn);

        return lookup.Values.ToList();
    }

#pragma warning disable
    public class CalloutDetailsLoadOptions
    {
        public bool IncludeCalloutDetailsDepartments { get; set; }
    }
#pragma warning restore
}