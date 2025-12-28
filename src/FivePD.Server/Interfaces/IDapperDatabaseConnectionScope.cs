using System;
using System.Data;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IDapperDatabaseConnectionScope : IDisposable
{
    /// <summary>
    /// Gets the database connection for Dapper operations.
    /// </summary>
    IDbConnection Connection { get; }
}