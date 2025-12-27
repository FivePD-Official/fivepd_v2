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
    /// Applies the migration.
    /// </summary>
    void Up(IDbConnection connection);

    /// <summary>
    /// Reverts the migration.
    /// </summary>
    void Down(IDbConnection connection);
}