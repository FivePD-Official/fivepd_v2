# Dapper Migration System Guide

## Overview
This guide explains how to use the Dapper migration system, which provides automatic migration application similar to EF Core.

## How It Works

### 1. Automatic Migration Application
When your application starts, the `DapperDatabaseHandler.EnsureCorrectState()` method automatically:
- Creates the `__Migrations` tracking table if it doesn't exist
- Scans for all `IMigration` implementations in your assembly
- Applies any pending migrations in order
- Tracks applied migrations in the `__Migrations` table

### 2. Migration Tracking
The system maintains a `__Migrations` table:
```sql
CREATE TABLE __Migrations (
    Id VARCHAR(100) PRIMARY KEY,
    Description VARCHAR(255) NOT NULL,
    AppliedAt DATETIME NOT NULL
);
```

## Creating New Migrations

### Step 1: Create Migration Class
Create a new class in `src/FivePD.Server/Database/Migrations/`:

```csharp
using System.Data;
using Dapper;

namespace FivePD.Server.Database.Migrations;

public class AddUserSettings_20251227_002 : IMigration
{
    public string Id => "20251227_002";
    public string Description => "Adds user settings table for preferences";

    public void Up(IDbConnection connection)
    {
        // Create new table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_user_settings (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                UserId INT NOT NULL,
                SettingKey VARCHAR(100) NOT NULL,
                SettingValue TEXT,
                UNIQUE KEY unique_user_setting (UserId, SettingKey),
                INDEX idx_settings_user (UserId)
            )");
    }

    public void Down(IDbConnection connection)
    {
        // Revert migration
        connection.Execute("DROP TABLE IF EXISTS fivepd_user_settings");
    }
}
```

### Step 2: Naming Convention
Use the format: `Description_YYYYMMDD_XXX`
- `Description`: Brief description of what the migration does
- `YYYYMMDD`: Date (helps with ordering)
- `XXX`: Sequential number (001, 002, etc.)

Examples:
- `InitialSchema_20251227_001`
- `AddUserSettings_20251227_002`
- `AddMessagePriority_20251227_003`

### Step 3: Migration Order
Migrations are applied in **alphabetical order** by their `Id`. Ensure your IDs reflect the correct order.

## Using Migrations

### Automatic Application (Recommended)
```csharp
// In your startup code
DapperDatabaseHandler.SetParameters(serviceProvider, logger);
DapperDatabaseHandler.EnsureCorrectState(); // Applies all pending migrations
DapperDatabaseHandler.SeedData();
```

### Manual Migration Management

#### List Applied Migrations
```csharp
DapperDatabaseHandler.ListMigrations();
```

Output:
```
Applied Migrations:
  - 20251227_001: Creates initial database schema for FivePD (Applied: 2025-12-27 20:10:00)
  - 20251227_002: Adds user settings table for preferences (Applied: 2025-12-27 20:10:05)
```

#### Revert a Migration
```csharp
DapperDatabaseHandler.RevertMigration("20251227_002");
```

## Migration Examples

### Adding a Column
```csharp
public class AddUserEmail_20251227_003 : IMigration
{
    public string Id => "20251227_003";
    public string Description => "Adds email column to users table";

    public void Up(IDbConnection connection)
    {
        // MySQL/MariaDB
        connection.Execute("ALTER TABLE fivepd_users ADD COLUMN Email VARCHAR(255)");
        
        // SQLite (if needed)
        // connection.Execute("ALTER TABLE fivepd_users ADD COLUMN Email TEXT");
    }

    public void Down(IDbConnection connection)
    {
        // MySQL/MariaDB
        connection.Execute("ALTER TABLE fivepd_users DROP COLUMN Email");
        
        // SQLite doesn't support DROP COLUMN, recreate table instead
    }
}
```

### Creating Index
```csharp
public class AddUserEmailIndex_20251227_004 : IMigration
{
    public string Id => "20251227_004";
    public string Description => "Adds index on user email";

    public void Up(IDbConnection connection)
    {
        connection.Execute("CREATE INDEX idx_users_email ON fivepd_users(Email)");
    }

    public void Down(IDbConnection connection)
    {
        connection.Execute("DROP INDEX idx_users_email ON fivepd_users");
    }
}
```

### Data Migration
```csharp
public class MigrateUserRoles_20251227_005 : IMigration
{
    public string Id => "20251227_005";
    public string Description => "Migrates user roles to new format";

    public void Up(IDbConnection connection)
    {
        // Update existing data
        connection.Execute(@"
            UPDATE fivepd_users 
            SET Role = Role * 2 
            WHERE Role < 100");
    }

    public void Down(IDbConnection connection)
    {
        // Revert data changes
        connection.Execute(@"
            UPDATE fivepd_users 
            SET Role = Role / 2 
            WHERE Role < 200");
    }
}
```

## Database Provider Differences

### MySQL/MariaDB
```csharp
// Creating tables
connection.Execute(@"
    CREATE TABLE IF NOT EXISTS my_table (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        Name VARCHAR(255) NOT NULL,
        INDEX idx_name (Name)
    )");

// Adding columns
connection.Execute("ALTER TABLE my_table ADD COLUMN NewColumn VARCHAR(100)");

// Creating indexes
connection.Execute("CREATE INDEX idx_new_column ON my_table(NewColumn)");
```

### SQLite
```csharp
// Creating tables
connection.Execute(@"
    CREATE TABLE IF NOT EXISTS my_table (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL
    )");

// SQLite doesn't support adding columns with constraints in ALTER TABLE
// For complex changes, recreate table:
connection.Execute(@"
    CREATE TABLE my_table_new (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL,
        NewColumn TEXT
    );
    INSERT INTO my_table_new (Id, Name) SELECT Id, Name FROM my_table;
    DROP TABLE my_table;
    ALTER TABLE my_table_new RENAME TO my_table;");

// Creating indexes
connection.Execute("CREATE INDEX idx_name ON my_table(Name)");
```

## Best Practices

### 1. Always Provide Down Method
```csharp
public void Down(IDbConnection connection)
{
    // Always implement revert logic
    // This allows migration rollback if needed
}
```

### 2. Test Migrations
```csharp
// Test in development
public void TestMigration()
{
    using var connection = new MySqlConnection("your-test-connection");
    var migration = new MyMigration_20251227_001();
    
    // Apply
    migration.Up(connection);
    
    // Verify
    var result = connection.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM my_table");
    
    // Revert
    migration.Down(connection);
}
```

### 3. Keep Migrations Small
- One logical change per migration
- Easier to understand and debug
- Easier to revert if needed

### 4. Use Transactions
```csharp
public void Up(IDbConnection connection)
{
    using var transaction = connection.BeginTransaction();
    try
    {
        connection.Execute("CREATE TABLE ...", transaction: transaction);
        connection.Execute("INSERT INTO ...", transaction: transaction);
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

### 5. Handle Multiple Providers
```csharp
public void Up(IDbConnection connection)
{
    if (connection is System.Data.SQLite.SQLiteConnection)
    {
        // SQLite-specific SQL
    }
    else
    {
        // MySQL/MariaDB SQL
    }
}
```

## Troubleshooting

### Migration Already Applied
If you see "Migration already applied, skipping":
- Check `__Migrations` table
- Migration was already run
- To re-run, remove from table or create new migration

### Migration Failed
1. Check logs for specific error
2. Verify database connection
3. Check SQL syntax for your provider
4. Manually run `Down()` method to clean up
5. Fix migration and create new one

### Connection Issues
```csharp
// Verify connection string
var factory = new DapperConnectionFactory(configService);
using var scope = factory.CreateScope();
var connection = scope.Connection;

// Test connection
connection.Open();
var test = connection.ExecuteScalar("SELECT 1");
```

## Migration History Query

```sql
-- View all applied migrations
SELECT * FROM __Migrations ORDER BY AppliedAt;

-- View last migration
SELECT * FROM __Migrations ORDER BY AppliedAt DESC LIMIT 1;

-- Check if specific migration applied
SELECT * FROM __Migrations WHERE Id = '20251227_001';
```

## Integration with Existing Code

### Update Startup
```csharp
// In Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add Dapper services
    services.AddSingleton<IDatabaseContextFactory, DapperConnectionFactory>();
    services.AddSingleton<MessageRepository>();
    services.AddSingleton<DatabaseMigrationManager>();
    services.AddSingleton<DapperDatabaseHandler>();
}

public void Configure(IApplicationBuilder app, IHostEnvironment env)
{
    // Apply migrations on startup
    using var scope = app.ApplicationServices.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfigService>();
    
    DapperDatabaseHandler.SetParameters(scope.ServiceProvider, logger);
    DapperDatabaseHandler.EnsureCorrectState();
    DapperDatabaseHandler.SeedData();
}
```

### Command Line Tools
```csharp
// Add to your CLI or admin panel
public class MigrationCommands
{
    public void List()
    {
        DapperDatabaseHandler.ListMigrations();
    }

    public void Revert(string migrationId)
    {
        DapperDatabaseHandler.RevertMigration(migrationId);
    }

    public void Status()
    {
        var applied = DapperDatabaseHandler.GetAppliedMigrations();
        Console.WriteLine($"Applied: {applied.Count}");
        foreach (var m in applied)
        {
            Console.WriteLine($"  {m.Id}: {m.Description}");
        }
    }
}
```

This system provides EF Core-like migration functionality with Dapper's performance benefits!