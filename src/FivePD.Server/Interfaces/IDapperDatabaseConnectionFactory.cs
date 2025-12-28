namespace FivePD.Server.Interfaces;

/// <summary>
/// A factory for creating scoped database contexts.
/// Use this to safely access the database from singleton services.
/// </summary>
public interface IDapperDatabaseConnectionFactory
{
    /// <summary>
    /// Gets the connection string used to establish a database connection.
    /// This property provides the necessary configuration details
    /// for connecting to the underlying database.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Creates a new scope and resolves a database context within it.
    /// </summary>
    /// <returns>A new <see cref="IDapperDatabaseConnectionScope"/>.</returns>
    IDapperDatabaseConnectionScope CreateScope();
}
