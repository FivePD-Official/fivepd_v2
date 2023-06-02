using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FivePD.Gamemode.Server.Database;

/// <summary>
/// Design-time context factory used only for migration generation.
/// </summary>
public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    /// <inheritdoc/>
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseMySql("server=;port=;database=;user=;password=;");

        return new DatabaseContext(optionsBuilder.Options);
    }
}