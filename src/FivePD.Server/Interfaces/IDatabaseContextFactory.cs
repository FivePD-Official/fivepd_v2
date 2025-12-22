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

/// <summary>
/// A factory for creating scoped database contexts.
/// Use this to safely access the database from singleton services.
/// </summary>
public interface IDatabaseContextFactory
{
    /// <summary>
    /// Creates a new scope and resolves a database context within it.
    /// </summary>
    /// <returns>A new <see cref="IDatabaseContextScope"/>.</returns>
    IDatabaseContextScope CreateScope();
}
