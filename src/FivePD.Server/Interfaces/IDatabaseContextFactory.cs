namespace FivePD.Server.Interfaces;

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
