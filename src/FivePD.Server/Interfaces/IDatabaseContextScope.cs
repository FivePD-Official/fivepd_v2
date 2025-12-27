using System;
using System.Data;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Represents a scope for database operations.
/// Ensures that the database connection is properly disposed of when the scope is finished.
/// </summary>
public interface IDatabaseContextScope : IDisposable
{
    /// <summary>
    /// Gets the database connection for Dapper operations.
    /// </summary>
    IDbConnection Connection { get; }
}