using System;
using FivePD.Server.Database;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Represents a scope for a database context.
/// Ensures that the context is properly disposed of when the scope is finished.
/// </summary>
public interface IDatabaseContextScope : IDisposable
{
    /// <summary>
    /// Gets the database context associated with the scope.
    /// </summary>
    DatabaseContext Context { get; }
}