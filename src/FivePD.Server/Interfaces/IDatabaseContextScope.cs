using System;
using System.Data;
using FivePD.Server.Database;

namespace FivePD.Server.Interfaces;

#pragma warning disable

/// <summary>
/// Represents a scope for database operations.
/// Ensures that the database connection is properly disposed of when the scope is finished.
/// </summary>
public interface IDatabaseContextScope : IDisposable
{
    DatabaseContext Context { get; }
}