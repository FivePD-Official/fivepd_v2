using System.Data;

namespace FivePD.Server.Database.Migrations;

/// <summary>
/// Interface for database migrations.
/// </summary>
public interface IMigration
{
    /// <summary>
    /// Gets the unique identifier for the migration (e.g., "20251227_001").
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the description of what this migration does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Applies the migration to bring the database schema to the desired state.
    /// </summary>
    /// <param name="connection">The database connection used to execute the migration.</param>
    void Up(IDbConnection connection);

    /// <summary>
    /// Reverts the applied migration for the database schema to its previous state.
    /// </summary>
    /// <param name="connection">The database connection used to execute the rollback.</param>
    void Down(IDbConnection connection);
}