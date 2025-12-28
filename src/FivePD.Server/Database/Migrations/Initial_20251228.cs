using System.Data;
using Dapper;

namespace FivePD.Server.Database.Migrations;

#pragma warning disable

public class CreateUsersTable_20251228 : IMigration
{
    public string Id => "20251228_001";
    public string Description => "Create user-related tables";

    public void Up(IDbConnection connection)
    {
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_user_callsigns (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                `Values` TEXT
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_departments (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Callsign VARCHAR(50) NOT NULL,
                UseGlobalRanks TINYINT(1) NOT NULL DEFAULT 1,
                CallsignFormat TEXT,
                MaxUsers INT NOT NULL DEFAULT 0,
                DefaultRankId INT,
                INDEX idx_departments_name (Name)
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_ranks (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Global TINYINT(1) NOT NULL DEFAULT 0,
                RequiredXp INT NOT NULL DEFAULT 0,
                Name VARCHAR(255) NOT NULL,
                Permission TEXT
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_divisions (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                DepartmentId INT,
                INDEX idx_divisions_dept (DepartmentId),
                FOREIGN KEY (DepartmentId) REFERENCES fivepd_departments(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_department_rank (
                DepartmentId INT NOT NULL,
                RankId INT NOT NULL,
                PRIMARY KEY (DepartmentId, RankId),
                FOREIGN KEY (DepartmentId) REFERENCES fivepd_departments(Id) ON DELETE CASCADE,
                FOREIGN KEY (RankId) REFERENCES fivepd_ranks(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_department_area_coordinates (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                DepartmentId INT,
                X FLOAT NOT NULL,
                Y FLOAT NOT NULL,
                INDEX idx_dept_area_coords_dept (DepartmentId),
                FOREIGN KEY (DepartmentId) REFERENCES fivepd_departments(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_incident_histories (
                Id VARCHAR(255) PRIMARY KEY,
                IncidentId VARCHAR(255) NOT NULL,
                Title VARCHAR(255) NOT NULL,
                Description TEXT,
                Location VARCHAR(255),
                Priority INT NOT NULL,
                Type INT NOT NULL,
                GeneratedAt DATETIME NOT NULL,
                INDEX idx_incident_histories_incident_id (IncidentId),
                INDEX idx_incident_histories_generated_at (GeneratedAt)
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                AccessFivePD TINYINT(1) NOT NULL DEFAULT 1,
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
                UserCallsignFormatId INT,
                DepartmentId INT,
                DivisionId INT,
                RankId INT,
                FixedRank TINYINT(1) NOT NULL DEFAULT 0,
                INDEX idx_users_license (IdentifierLicense),
                INDEX idx_users_discord (IdentifierDiscord),
                INDEX idx_users_steam (IdentifierSteam),
                INDEX idx_users_dept (DepartmentId),
                INDEX idx_users_division (DivisionId),
                INDEX idx_users_rank (RankId),
                FOREIGN KEY (UserCallsignFormatId) REFERENCES fivepd_user_callsigns(Id) ON DELETE SET NULL,
                FOREIGN KEY (DepartmentId) REFERENCES fivepd_departments(Id) ON DELETE SET NULL,
                FOREIGN KEY (DivisionId) REFERENCES fivepd_divisions(Id) ON DELETE SET NULL,
                FOREIGN KEY (RankId) REFERENCES fivepd_ranks(Id) ON DELETE SET NULL
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_messages (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Sender VARCHAR(255) NOT NULL,
                Subject VARCHAR(255) NOT NULL,
                Content TEXT NOT NULL,
                SentAt DATETIME NOT NULL,
                `Read` TINYINT(1) NOT NULL DEFAULT 0,
                UserId INT,
                INDEX idx_messages_user (UserId),
                FOREIGN KEY (UserId) REFERENCES fivepd_users(Id) ON DELETE SET NULL
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_incident_history_users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                UserId INT NOT NULL,
                IncidentHistoryId VARCHAR(255) NOT NULL,
                INDEX idx_incident_history_users_user (UserId),
                INDEX idx_incident_history_users_incident (IncidentHistoryId),
                FOREIGN KEY (UserId) REFERENCES fivepd_users(Id) ON DELETE CASCADE,
                FOREIGN KEY (IncidentHistoryId) REFERENCES fivepd_incident_histories(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_ped_data (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Gender INT NOT NULL,
                Firstname VARCHAR(255) NOT NULL,
                Lastname VARCHAR(255) NOT NULL,
                Birthdate DATETIME NOT NULL,
                Items TEXT,
                Warrants TEXT,
                AlcoholLevel DOUBLE NOT NULL,
                Drugs TEXT
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_ped_licenses (
                Id VARCHAR(255) PRIMARY KEY,
                PedDataId INT NOT NULL,
                Type INT NOT NULL,
                Status INT NOT NULL,
                IssueDate DATETIME NOT NULL,
                ExpirationDate DATETIME NOT NULL,
                INDEX idx_ped_licenses_ped (PedDataId),
                FOREIGN KEY (PedDataId) REFERENCES fivepd_ped_data(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_vehicle_data (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                PedDataId INT,
                LicensePlate VARCHAR(50) NOT NULL,
                Color VARCHAR(50),
                Brand VARCHAR(255),
                Items TEXT,
                INDEX idx_vehicle_data_ped (PedDataId),
                FOREIGN KEY (PedDataId) REFERENCES fivepd_ped_data(Id) ON DELETE SET NULL
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_vehicle_documents (
                Id VARCHAR(255) PRIMARY KEY,
                VehicleDataId INT NOT NULL,
                Type INT NOT NULL,
                Status INT NOT NULL,
                IssueDate DATETIME NOT NULL,
                ExpirationDate DATETIME NOT NULL,
                INDEX idx_vehicle_documents_vehicle (VehicleDataId),
                FOREIGN KEY (VehicleDataId) REFERENCES fivepd_vehicle_data(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_vehicle_configs (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Type INT NOT NULL,
                Name VARCHAR(255) NOT NULL,
                Model VARCHAR(255) NOT NULL,
                Livery INT NOT NULL DEFAULT 0,
                Extras TEXT,
                Options TEXT
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_callout_details (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Active TINYINT(1) NOT NULL DEFAULT 0,
                TypeName VARCHAR(255) NOT NULL,
                AssemblyName VARCHAR(255) NOT NULL,
                RulesetType INT NOT NULL
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_callout_details_departments (
                CalloutDetailsId INT NOT NULL,
                DepartmentId INT NOT NULL,
                PRIMARY KEY (CalloutDetailsId, DepartmentId),
                FOREIGN KEY (CalloutDetailsId) REFERENCES fivepd_callout_details(Id) ON DELETE CASCADE,
                FOREIGN KEY (DepartmentId) REFERENCES fivepd_departments(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_variable_data_sets (
                Id VARCHAR(255) PRIMARY KEY
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_variable_data_entities (
                Id VARCHAR(255) PRIMARY KEY,
                SetId VARCHAR(255) NOT NULL,
                INDEX idx_variable_data_entities_set (SetId),
                FOREIGN KEY (SetId) REFERENCES fivepd_variable_data_sets(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_variable_data_attributes (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                SetId VARCHAR(255) NOT NULL,
                DataType INT NOT NULL,
                AddonId VARCHAR(255) NOT NULL,
                Name VARCHAR(255) NOT NULL,
                `Order` INT NOT NULL,
                Required TINYINT(1) NOT NULL DEFAULT 0,
                Hidden TINYINT(1) NOT NULL DEFAULT 0,
                Readonly TINYINT(1) NOT NULL DEFAULT 0,
                ShowInGrid TINYINT(1) NOT NULL DEFAULT 0,
                Placeholder VARCHAR(255),
                Label VARCHAR(255),
                HelpLabel VARCHAR(255),
                Options TEXT,
                INDEX idx_variable_data_attributes_set (SetId),
                FOREIGN KEY (SetId) REFERENCES fivepd_variable_data_sets(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_variable_data_values (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                AttributeId INT NOT NULL,
                EntityId VARCHAR(255) NOT NULL,
                Value TEXT,
                INDEX idx_variable_data_values_attribute (AttributeId),
                INDEX idx_variable_data_values_entity (EntityId),
                FOREIGN KEY (AttributeId) REFERENCES fivepd_variable_data_attributes(Id) ON DELETE CASCADE,
                FOREIGN KEY (EntityId) REFERENCES fivepd_variable_data_entities(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_locations (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Type INT NOT NULL,
                X FLOAT NOT NULL,
                Y FLOAT NOT NULL,
                Z FLOAT NOT NULL
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_assistance_service_type_configs (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                TypeName VARCHAR(255) NOT NULL,
                AssemblyName VARCHAR(255) NOT NULL,
                Active TINYINT(1) NOT NULL DEFAULT 0,
                Enabled TINYINT(1) NOT NULL DEFAULT 0,
                SpawnType VARCHAR(50) NOT NULL DEFAULT 'AUTO',
                ManualSpawnLocations TEXT
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_assistance_service_stations (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                X FLOAT NOT NULL,
                Y FLOAT NOT NULL,
                Z FLOAT NOT NULL
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_assistance_service_station_type_configs (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                TypeConfigId INT NOT NULL,
                Capacity INT NOT NULL,
                ActiveCapacity INT NOT NULL,
                INDEX idx_assistance_station_type_configs_type (TypeConfigId),
                FOREIGN KEY (TypeConfigId) REFERENCES fivepd_assistance_service_type_configs(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_weapons (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Model VARCHAR(255) NOT NULL
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_weapon_components (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                HashKey VARCHAR(255) NOT NULL
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_loadout_configs (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_loadout_weapon (
                LoadoutConfigId INT NOT NULL,
                WeaponId INT NOT NULL,
                PRIMARY KEY (LoadoutConfigId, WeaponId),
                FOREIGN KEY (LoadoutConfigId) REFERENCES fivepd_loadout_configs(Id) ON DELETE CASCADE,
                FOREIGN KEY (WeaponId) REFERENCES fivepd_weapons(Id) ON DELETE CASCADE
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS fivepd_weapon_weaponComponent (
                WeaponId INT NOT NULL,
                WeaponComponentId INT NOT NULL,
                PRIMARY KEY (WeaponId, WeaponComponentId),
                FOREIGN KEY (WeaponId) REFERENCES fivepd_weapons(Id) ON DELETE CASCADE,
                FOREIGN KEY (WeaponComponentId) REFERENCES fivepd_weapon_components(Id) ON DELETE CASCADE
            )");
    }

    public void Down(IDbConnection connection)
    {
        connection.Execute("DROP TABLE IF EXISTS fivepd_weapon_weaponComponent");
        connection.Execute("DROP TABLE IF EXISTS fivepd_loadout_weapon");
        connection.Execute("DROP TABLE IF EXISTS fivepd_loadout_configs");
        connection.Execute("DROP TABLE IF EXISTS fivepd_weapon_components");
        connection.Execute("DROP TABLE IF EXISTS fivepd_weapons");
        connection.Execute("DROP TABLE IF EXISTS fivepd_assistance_service_station_type_configs");
        connection.Execute("DROP TABLE IF EXISTS fivepd_assistance_service_stations");
        connection.Execute("DROP TABLE IF EXISTS fivepd_assistance_service_type_configs");
        connection.Execute("DROP TABLE IF EXISTS fivepd_locations");
        connection.Execute("DROP TABLE IF EXISTS fivepd_variable_data_values");
        connection.Execute("DROP TABLE IF EXISTS fivepd_variable_data_attributes");
        connection.Execute("DROP TABLE IF EXISTS fivepd_variable_data_entities");
        connection.Execute("DROP TABLE IF EXISTS fivepd_variable_data_sets");
        connection.Execute("DROP TABLE IF EXISTS fivepd_callout_details_departments");
        connection.Execute("DROP TABLE IF EXISTS fivepd_callout_details");
        connection.Execute("DROP TABLE IF EXISTS fivepd_vehicle_configs");
        connection.Execute("DROP TABLE IF EXISTS fivepd_vehicle_documents");
        connection.Execute("DROP TABLE IF EXISTS fivepd_vehicle_data");
        connection.Execute("DROP TABLE IF EXISTS fivepd_ped_licenses");
        connection.Execute("DROP TABLE IF EXISTS fivepd_ped_data");
        connection.Execute("DROP TABLE IF EXISTS fivepd_incident_history_users");
        connection.Execute("DROP TABLE IF EXISTS fivepd_messages");
        connection.Execute("DROP TABLE IF EXISTS fivepd_users");
        connection.Execute("DROP TABLE IF EXISTS fivepd_incident_histories");
        connection.Execute("DROP TABLE IF EXISTS fivepd_department_area_coordinates");
        connection.Execute("DROP TABLE IF EXISTS fivepd_department_rank");
        connection.Execute("DROP TABLE IF EXISTS fivepd_divisions");
        connection.Execute("DROP TABLE IF EXISTS fivepd_ranks");
        connection.Execute("DROP TABLE IF EXISTS fivepd_departments");
        connection.Execute("DROP TABLE IF EXISTS fivepd_user_callsigns");
    }
}