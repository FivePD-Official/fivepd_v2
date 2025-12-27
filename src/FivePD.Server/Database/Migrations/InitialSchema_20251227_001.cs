using System.Data;
using Dapper;

namespace FivePD.Server.Database.Migrations;

/// <summary>
/// Initial schema creation migration.
/// </summary>
public class InitialSchema_20251227_001 : IMigration
{
    public string Id => "20251227_001";
    public string Description => "Creates initial database schema for FivePD";

    public void Up(IDbConnection connection)
    {
        // Users table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                AccessFivePD TINYINT(1) NOT NULL DEFAULT 0,
                PreferredLanguage VARCHAR(10),
                FirstLogin TINYINT(1) NOT NULL DEFAULT 1,
                DismissedWelcomeMessage TINYINT(1) NOT NULL DEFAULT 0,
                Name VARCHAR(255),
                IdentifierLicense VARCHAR(255),
                IdentifierDiscord VARCHAR(255),
                IdentifierSteam VARCHAR(255),
                IdentifierXbl VARCHAR(255),
                IdentifierLive VARCHAR(255),
                Xp INT NOT NULL DEFAULT 0,
                AudioVolume DOUBLE NOT NULL DEFAULT 0.5,
                Role INT NOT NULL DEFAULT 0,
                Callsign VARCHAR(50),
                CallsignFormatId INT,
                DepartmentId INT,
                DivisionId INT,
                RankId INT,
                FixedRank TINYINT(1) NOT NULL DEFAULT 0,
                INDEX idx_users_license (IdentifierLicense),
                INDEX idx_users_discord (IdentifierDiscord),
                INDEX idx_users_steam (IdentifierSteam)
            )");

        // Messages table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_messages (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Sender VARCHAR(255) NOT NULL,
                Subject VARCHAR(255) NOT NULL,
                Content TEXT NOT NULL,
                SentAt DATETIME NOT NULL,
                Read TINYINT(1) NOT NULL DEFAULT 0,
                UserId INT NOT NULL,
                INDEX idx_messages_user (UserId),
                INDEX idx_messages_sent (SentAt)
            )");

        // Departments table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_departments (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Callsign VARCHAR(50) NOT NULL,
                MaxUsers INT NOT NULL DEFAULT 0,
                UseGlobalRanks TINYINT(1) NOT NULL DEFAULT 1,
                CallsignFormat TEXT,
                DefaultRankId INT,
                INDEX idx_departments_name (Name)
            )");

        // Ranks table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_ranks (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                RequiredXp INT NOT NULL DEFAULT 0,
                Global TINYINT(1) NOT NULL DEFAULT 0,
                Permission TEXT
            )");

        // Divisions table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_divisions (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                DepartmentId INT NOT NULL,
                INDEX idx_divisions_dept (DepartmentId)
            )");

        // Vehicle configs table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_vehicle_configs (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Model VARCHAR(255) NOT NULL,
                Options TEXT,
                Extras TEXT
            )");

        // Loadout configs table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_loadout_configs (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL
            )");

        // Weapons table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_weapons (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Model VARCHAR(255) NOT NULL
            )");

        // Weapon components table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_weapon_components (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                HashKey VARCHAR(255) NOT NULL
            )");

        // Join tables
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_loadout_weapons (
                LoadoutConfigId INT NOT NULL,
                WeaponId INT NOT NULL,
                PRIMARY KEY (LoadoutConfigId, WeaponId)
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_weapon_weapon_components (
                WeaponId INT NOT NULL,
                WeaponComponentId INT NOT NULL,
                PRIMARY KEY (WeaponId, WeaponComponentId)
            )");
    }

    public void Down(IDbConnection connection)
    {
        // Drop all tables created in this migration
        connection.Execute("DROP TABLE IF EXISTS fivepd_weapon_weapon_components");
        connection.Execute("DROP TABLE IF EXISTS fivepd_loadout_weapons");
        connection.Execute("DROP TABLE IF EXISTS fivepd_weapon_components");
        connection.Execute("DROP TABLE IF EXISTS fivepd_weapons");
        connection.Execute("DROP TABLE IF EXISTS fivepd_loadout_configs");
        connection.Execute("DROP TABLE IF EXISTS fivepd_vehicle_configs");
        connection.Execute("DROP TABLE IF EXISTS fivepd_divisions");
        connection.Execute("DROP TABLE IF EXISTS fivepd_ranks");
        connection.Execute("DROP TABLE IF EXISTS fivepd_departments");
        connection.Execute("DROP TABLE IF EXISTS fivepd_messages");
        connection.Execute("DROP TABLE IF EXISTS fivepd_users");
    }
}