using System.Data;
using Dapper;
using FivePD.Server.Database;

namespace FivePD.Server.Database.Migrations;

#pragma warning disable

public class CreateUsersTable_20251228 : IMigration
{
    public string Id => "20251228_001";
    public string Description => "Create user-related tables";

    public void Up(IDbConnection connection)
    {
        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.UserCallsigns} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                `Values` TEXT
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.Departments} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Callsign VARCHAR(50) NOT NULL,
                UseGlobalRanks TINYINT(1) NOT NULL DEFAULT 1,
                CallsignFormat TEXT,
                MaxUsers INT NOT NULL DEFAULT 0,
                DefaultRankId INT,
                INDEX idx_departments_name (Name)
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.Ranks} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Global TINYINT(1) NOT NULL DEFAULT 0,
                RequiredXp INT NOT NULL DEFAULT 0,
                Name VARCHAR(255) NOT NULL,
                Permission TEXT
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.Divisions} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                DepartmentId INT,
                INDEX idx_divisions_dept (DepartmentId),
                FOREIGN KEY (DepartmentId) REFERENCES {TableNames.Departments}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.DepartmentRanks} (
                DepartmentId INT NOT NULL,
                RankId INT NOT NULL,
                PRIMARY KEY (DepartmentId, RankId),
                FOREIGN KEY (DepartmentId) REFERENCES {TableNames.Departments}(Id) ON DELETE CASCADE,
                FOREIGN KEY (RankId) REFERENCES {TableNames.Ranks}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.DepartmentAreaCoordinates} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                DepartmentId INT,
                X FLOAT NOT NULL,
                Y FLOAT NOT NULL,
                INDEX idx_dept_area_coords_dept (DepartmentId),
                FOREIGN KEY (DepartmentId) REFERENCES {TableNames.Departments}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.IncidentHistories} (
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

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.Users} (
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
                FOREIGN KEY (UserCallsignFormatId) REFERENCES {TableNames.UserCallsigns}(Id) ON DELETE SET NULL,
                FOREIGN KEY (DepartmentId) REFERENCES {TableNames.Departments}(Id) ON DELETE SET NULL,
                FOREIGN KEY (DivisionId) REFERENCES {TableNames.Divisions}(Id) ON DELETE SET NULL,
                FOREIGN KEY (RankId) REFERENCES {TableNames.Ranks}(Id) ON DELETE SET NULL
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.Messages} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Sender VARCHAR(255) NOT NULL,
                Subject VARCHAR(255) NOT NULL,
                Content TEXT NOT NULL,
                SentAt DATETIME NOT NULL,
                HasRead TINYINT(1) NOT NULL DEFAULT 0,
                UserId INT,
                INDEX idx_messages_user (UserId),
                FOREIGN KEY (UserId) REFERENCES {TableNames.Users}(Id) ON DELETE SET NULL
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.IncidentHistoryUsers} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                UserId INT NOT NULL,
                IncidentHistoryId VARCHAR(255) NOT NULL,
                INDEX idx_incident_history_users_user (UserId),
                INDEX idx_incident_history_users_incident (IncidentHistoryId),
                FOREIGN KEY (UserId) REFERENCES {TableNames.Users}(Id) ON DELETE CASCADE,
                FOREIGN KEY (IncidentHistoryId) REFERENCES {TableNames.IncidentHistories}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.PedData} (
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

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.PedLicenses} (
                Id VARCHAR(255) PRIMARY KEY,
                PedDataId INT NOT NULL,
                Type INT NOT NULL,
                Status INT NOT NULL,
                IssueDate DATETIME NOT NULL,
                ExpirationDate DATETIME NOT NULL,
                INDEX idx_ped_licenses_ped (PedDataId),
                FOREIGN KEY (PedDataId) REFERENCES {TableNames.PedData}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.VehicleData} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                PedDataId INT,
                LicensePlate VARCHAR(50) NOT NULL,
                Color VARCHAR(50),
                Brand VARCHAR(255),
                Items TEXT,
                INDEX idx_vehicle_data_ped (PedDataId),
                FOREIGN KEY (PedDataId) REFERENCES {TableNames.PedData}(Id) ON DELETE SET NULL
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.VehicleDocuments} (
                Id VARCHAR(255) PRIMARY KEY,
                VehicleDataId INT NOT NULL,
                Type INT NOT NULL,
                Status INT NOT NULL,
                IssueDate DATETIME NOT NULL,
                ExpirationDate DATETIME NOT NULL,
                INDEX idx_vehicle_documents_vehicle (VehicleDataId),
                FOREIGN KEY (VehicleDataId) REFERENCES {TableNames.VehicleData}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.VehicleConfigs} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Type INT NOT NULL,
                Name VARCHAR(255) NOT NULL,
                Model VARCHAR(255) NOT NULL,
                Livery INT NOT NULL DEFAULT 0,
                Extras TEXT,
                Options TEXT
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.CalloutDetails} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Active TINYINT(1) NOT NULL DEFAULT 0,
                TypeName VARCHAR(255) NOT NULL,
                AssemblyName VARCHAR(255) NOT NULL,
                RulesetType INT NOT NULL
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.CalloutDetailsDepartments} (
                CalloutDetailsId INT NOT NULL,
                DepartmentId INT NOT NULL,
                PRIMARY KEY (CalloutDetailsId, DepartmentId),
                FOREIGN KEY (CalloutDetailsId) REFERENCES {TableNames.CalloutDetails}(Id) ON DELETE CASCADE,
                FOREIGN KEY (DepartmentId) REFERENCES {TableNames.Departments}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.VariableDataSets} (
                Id VARCHAR(255) PRIMARY KEY
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.VariableDataEntities} (
                Id VARCHAR(255) PRIMARY KEY,
                SetId VARCHAR(255) NOT NULL,
                INDEX idx_variable_data_entities_set (SetId),
                FOREIGN KEY (SetId) REFERENCES {TableNames.VariableDataSets}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.VariableDataAttributes} (
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
                FOREIGN KEY (SetId) REFERENCES {TableNames.VariableDataSets}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.VariableDataValues} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                AttributeId INT NOT NULL,
                EntityId VARCHAR(255) NOT NULL,
                Value TEXT,
                INDEX idx_variable_data_values_attribute (AttributeId),
                INDEX idx_variable_data_values_entity (EntityId),
                FOREIGN KEY (AttributeId) REFERENCES {TableNames.VariableDataAttributes}(Id) ON DELETE CASCADE,
                FOREIGN KEY (EntityId) REFERENCES {TableNames.VariableDataEntities}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.Locations} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Type INT NOT NULL,
                X FLOAT NOT NULL,
                Y FLOAT NOT NULL,
                Z FLOAT NOT NULL
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.AssistanceServiceTypeConfigs} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                TypeName VARCHAR(255) NOT NULL,
                AssemblyName VARCHAR(255) NOT NULL,
                Active TINYINT(1) NOT NULL DEFAULT 0,
                Enabled TINYINT(1) NOT NULL DEFAULT 0,
                SpawnType VARCHAR(50) NOT NULL DEFAULT 'AUTO',
                ManualSpawnLocations TEXT
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.AssistanceServiceStations} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                X FLOAT NOT NULL,
                Y FLOAT NOT NULL,
                Z FLOAT NOT NULL
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.AssistanceServiceStationTypeConfigs} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                TypeConfigId INT NOT NULL,
                Capacity INT NOT NULL,
                ActiveCapacity INT NOT NULL,
                INDEX idx_assistance_station_type_configs_type (TypeConfigId),
                FOREIGN KEY (TypeConfigId) REFERENCES {TableNames.AssistanceServiceTypeConfigs}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.Weapons} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Model VARCHAR(255) NOT NULL
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.WeaponComponents} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                HashKey VARCHAR(255) NOT NULL
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.LoadoutConfigs} (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.LoadoutWeapon} (
                LoadoutConfigId INT NOT NULL,
                WeaponId INT NOT NULL,
                PRIMARY KEY (LoadoutConfigId, WeaponId),
                FOREIGN KEY (LoadoutConfigId) REFERENCES {TableNames.LoadoutConfigs}(Id) ON DELETE CASCADE,
                FOREIGN KEY (WeaponId) REFERENCES {TableNames.Weapons}(Id) ON DELETE CASCADE
            )");

        connection.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableNames.WeaponWeaponComponent} (
                WeaponId INT NOT NULL,
                WeaponComponentId INT NOT NULL,
                PRIMARY KEY (WeaponId, WeaponComponentId),
                FOREIGN KEY (WeaponId) REFERENCES {TableNames.Weapons}(Id) ON DELETE CASCADE,
                FOREIGN KEY (WeaponComponentId) REFERENCES {TableNames.WeaponComponents}(Id) ON DELETE CASCADE
            )");
    }

    public void Down(IDbConnection connection)
    {
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.WeaponWeaponComponent}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.LoadoutWeapon}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.LoadoutConfigs}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.WeaponComponents}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.Weapons}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.AssistanceServiceStationTypeConfigs}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.AssistanceServiceStations}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.AssistanceServiceTypeConfigs}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.Locations}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.VariableDataValues}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.VariableDataAttributes}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.VariableDataEntities}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.VariableDataSets}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.CalloutDetailsDepartments}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.CalloutDetails}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.VehicleConfigs}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.VehicleDocuments}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.VehicleData}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.PedLicenses}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.PedData}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.IncidentHistoryUsers}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.Messages}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.Users}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.IncidentHistories}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.DepartmentAreaCoordinates}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.DepartmentRanks}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.Divisions}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.Ranks}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.Departments}");
        connection.Execute($"DROP TABLE IF EXISTS {TableNames.UserCallsigns}");
    }
}