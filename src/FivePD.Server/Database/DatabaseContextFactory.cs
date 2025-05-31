using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FivePD.Server.Database;

/// <summary>
/// Design-time context factory used for generating database migrations.
/// Provides a database context with default connection settings.
/// </summary>
internal class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    /// <inheritdoc/>
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseMySql("server=;port=;database=;user=;password=;");

        return new DatabaseContext(optionsBuilder.Options);
    }
}