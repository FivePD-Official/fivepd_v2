using System;
using System.Data;

namespace FivePD.Server.Database;

#pragma warning disable

public interface IDatabaseConnectionScope : IDisposable
{
    /// <summary>
    /// Gets the database connection for Dapper operations.
    /// </summary>
    IDbConnection Connection { get; }
}