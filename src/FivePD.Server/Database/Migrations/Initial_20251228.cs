using System.Collections.Generic;
using System.Data;
using System.Linq;
using CitizenFX.Core;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Database.Sql.Migration;

namespace FivePD.Server.Database.Migrations;

#pragma warning disable

public class CreateUsersTable_20251228 : IMigration
{
    public string Id => "20251228_001";
    public string Description => "Create user-related tables";

    public void Up(IDbConnection connection, DatabaseType databaseType)
    {
        new TableBuilder(connection, databaseType, TableNames.UserCallsigns)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("FormatValues", x => x.Text())
            .Execute();
        
        new TableBuilder(connection, databaseType, TableNames.Departments)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Name", x => x.Varchar(255).NotNull())
            .Column("Callsign", x => x.Varchar(50).NotNull())
            .Column("UseGlobalRanks", x => x.Boolean().NotNull().Default(1))
            .Column("CallsignFormat", x => x.Text())
            .Column("MaxUsers", x => x.Integer().NotNull().Default(0))
            .Column("DefaultRankId", x => x.Integer())
            .Index("idx_departments_name", "Name")
            .Execute();
  
        new TableBuilder(connection, databaseType, TableNames.Ranks)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Global", x => x.Boolean().NotNull().Default(0))
            .Column("RequiredXp", x => x.Integer().NotNull().Default(0))
            .Column("Name", x => x.Varchar(255).NotNull())
            .Column("Permission", x => x.Text())
            .Execute();
             
        new TableBuilder(connection, databaseType, TableNames.Divisions)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Name", x => x.Varchar(255).NotNull())
            .Column("DepartmentId", x => x.Integer())
            .Index("idx_divisions_dept", "DepartmentId")
            .ForeignKey("DepartmentId", x => x.References(TableNames.Departments, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.DepartmentRanks)
            .Column("DepartmentId", x => x.Integer().NotNull())
            .Column("RankId", x => x.Integer().NotNull())
            .ForeignKey("DepartmentId", x => x.References(TableNames.Departments, "Id").OnDeleteCascade())
            .ForeignKey("RankId", x => x.References(TableNames.Ranks, "Id").OnDeleteCascade())
            .PrimaryKey("DepartmentId", "RankId")
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.DepartmentAreaCoordinates)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("DepartmentId", x => x.Integer())
            .Column("X", x => x.Float().NotNull())
            .Column("Y", x => x.Float().NotNull())
            .Index("idx_dept_area_coords_dept", "DepartmentId")
            .ForeignKey("DepartmentId", x => x.References(TableNames.Departments, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.IncidentHistories)
            .Column("Id", x => x.Varchar(255).PrimaryKey())
            .Column("IncidentId", x => x.Varchar(255).NotNull())
            .Column("Title", x => x.Varchar(255).NotNull())
            .Column("Description", x => x.Text())
            .Column("Location", x => x.Varchar(255))
            .Column("Priority", x => x.Integer().NotNull())
            .Column("Type", x => x.Integer().NotNull())
            .Column("GeneratedAt", x => x.DateTime().NotNull())
            .Index("idx_incident_histories_incident_id", "IncidentId")
            .Index("idx_incident_histories_generated_at", "GeneratedAt")
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.Users)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("AccessFivePD", x => x.Boolean().NotNull().Default(1))
            .Column("PreferredLanguage", x => x.Varchar(10))
            .Column("FirstLogin", x => x.Boolean().NotNull().Default(1))
            .Column("DismissedWelcomeMessage", x => x.Boolean().NotNull().Default(0))
            .Column("Name", x => x.Varchar(255))
            .Column("IdentifierLicense", x => x.Varchar(255))
            .Column("IdentifierDiscord", x => x.Varchar(255))
            .Column("IdentifierSteam", x => x.Varchar(255))
            .Column("IdentifierXbl", x => x.Varchar(255))
            .Column("IdentifierLive", x => x.Varchar(255))
            .Column("Xp", x => x.Integer().NotNull().Default(0))
            .Column("AudioVolume", x => x.Double().NotNull().Default(0.5))
            .Column("Role", x => x.Integer().NotNull().Default(0))
            .Column("Callsign", x => x.Varchar(50))
            .Column("UserCallsignFormatId", x => x.Integer())
            .Column("DepartmentId", x => x.Integer())
            .Column("DivisionId", x => x.Integer())
            .Column("RankId", x => x.Integer())
            .Column("FixedRank", x => x.Boolean().NotNull().Default(0))
            .Index("idx_users_license", "IdentifierLicense")
            .Index("idx_users_discord", "IdentifierDiscord")
            .Index("idx_users_steam", "IdentifierSteam")
            .Index("idx_users_dept", "DepartmentId")
            .Index("idx_users_division", "DivisionId")
            .Index("idx_users_rank", "RankId")
            .ForeignKey("UserCallsignFormatId", x => x.References(TableNames.UserCallsigns, "Id").OnDeleteSetNull())
            .ForeignKey("DepartmentId", x => x.References(TableNames.Departments, "Id").OnDeleteSetNull())
            .ForeignKey("DivisionId", x => x.References(TableNames.Divisions, "Id").OnDeleteSetNull())
            .ForeignKey("RankId", x => x.References(TableNames.Ranks, "Id").OnDeleteSetNull())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.Messages)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Sender", x => x.Varchar(255).NotNull())
            .Column("Subject", x => x.Varchar(255).NotNull())
            .Column("Content", x => x.Text().NotNull())
            .Column("SentAt", x => x.DateTime().NotNull())
            .Column("HasRead", x => x.Boolean().NotNull().Default(0))
            .Column("UserId", x => x.Integer())
            .Index("idx_messages_user", "UserId")
            .ForeignKey("UserId", x => x.References(TableNames.Users, "Id").OnDeleteSetNull())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.IncidentHistoryUsers)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("UserId", x => x.Integer().NotNull())
            .Column("IncidentHistoryId", x => x.Varchar(255).NotNull())
            .Index("idx_incident_history_users_user", "UserId")
            .Index("idx_incident_history_users_incident", "IncidentHistoryId")
            .ForeignKey("UserId", x => x.References(TableNames.Users, "Id").OnDeleteCascade())
            .ForeignKey("IncidentHistoryId", x => x.References(TableNames.IncidentHistories, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.PedData)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Gender", x => x.Integer().NotNull())
            .Column("Firstname", x => x.Varchar(255).NotNull())
            .Column("Lastname", x => x.Varchar(255).NotNull())
            .Column("Birthdate", x => x.DateTime().NotNull())
            .Column("Items", x => x.Text())
            .Column("Warrants", x => x.Text())
            .Column("AlcoholLevel", x => x.Double().NotNull())
            .Column("Drugs", x => x.Text())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.PedLicenses)
            .Column("Id", x => x.Varchar(255).PrimaryKey())
            .Column("PedDataId", x => x.Integer().NotNull())
            .Column("Type", x => x.Integer().NotNull())
            .Column("Status", x => x.Integer().NotNull())
            .Column("IssueDate", x => x.DateTime().NotNull())
            .Column("ExpirationDate", x => x.DateTime().NotNull())
            .Index("idx_ped_licenses_ped", "PedDataId")
            .ForeignKey("PedDataId", x => x.References(TableNames.PedData, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.VehicleData)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("PedDataId", x => x.Integer())
            .Column("LicensePlate", x => x.Varchar(50).NotNull())
            .Column("Color", x => x.Varchar(50))
            .Column("Brand", x => x.Varchar(255))
            .Column("Items", x => x.Text())
            .Index("idx_vehicle_data_ped", "PedDataId")
            .ForeignKey("PedDataId", x => x.References(TableNames.PedData, "Id").OnDeleteSetNull())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.VehicleDocuments)
            .Column("Id", x => x.Varchar(255).PrimaryKey())
            .Column("VehicleDataId", x => x.Integer().NotNull())
            .Column("Type", x => x.Integer().NotNull())
            .Column("Status", x => x.Integer().NotNull())
            .Column("IssueDate", x => x.DateTime().NotNull())
            .Column("ExpirationDate", x => x.DateTime().NotNull())
            .Index("idx_vehicle_documents_vehicle", "VehicleDataId")
            .ForeignKey("VehicleDataId", x => x.References(TableNames.VehicleData, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.VehicleConfigs)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Type", x => x.Integer().NotNull())
            .Column("Name", x => x.Varchar(255).NotNull())
            .Column("Model", x => x.Varchar(255).NotNull())
            .Column("Livery", x => x.Integer().NotNull().Default(0))
            .Column("Extras", x => x.Text())
            .Column("Options", x => x.Text())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.CalloutDetails)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Active", x => x.Boolean().NotNull().Default(0))
            .Column("TypeName", x => x.Varchar(255).NotNull())
            .Column("AssemblyName", x => x.Varchar(255).NotNull())
            .Column("RulesetType", x => x.Integer().NotNull())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.CalloutDetailsDepartments)
            .Column("CalloutDetailsId", x => x.Integer().NotNull())
            .Column("DepartmentId", x => x.Integer().NotNull())
            .PrimaryKey("CalloutDetailsId", "DepartmentId")
            .ForeignKey("CalloutDetailsId", x => x.References(TableNames.CalloutDetails, "Id").OnDeleteCascade())
            .ForeignKey("DepartmentId", x => x.References(TableNames.Departments, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.VariableDataSets)
            .Column("Id", x => x.Varchar(255).PrimaryKey())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.VariableDataEntities)
            .Column("Id", x => x.Varchar(255).PrimaryKey())
            .Column("SetId", x => x.Varchar(255).NotNull())
            .Index("idx_variable_data_entities_set", "SetId")
            .ForeignKey("SetId", x => x.References(TableNames.VariableDataSets, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.VariableDataAttributes)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("SetId", x => x.Varchar(255).NotNull())
            .Column("DataType", x => x.Integer().NotNull())
            .Column("AddonId", x => x.Varchar(255).NotNull())
            .Column("Name", x => x.Varchar(255).NotNull())
            .Column("Order", x => x.Integer().NotNull())
            .Column("Required", x => x.Boolean().NotNull().Default(0))
            .Column("Hidden", x => x.Boolean().NotNull().Default(0))
            .Column("Readonly", x => x.Boolean().NotNull().Default(0))
            .Column("ShowInGrid", x => x.Boolean().NotNull().Default(0))
            .Column("Placeholder", x => x.Varchar(255))
            .Column("Label", x => x.Varchar(255))
            .Column("HelpLabel", x => x.Varchar(255))
            .Column("Options", x => x.Text())
            .Index("idx_variable_data_attributes_set", "SetId")
            .ForeignKey("SetId", x => x.References(TableNames.VariableDataSets, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.VariableDataValues)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("AttributeId", x => x.Integer().NotNull())
            .Column("EntityId", x => x.Varchar(255).NotNull())
            .Column("Value", x => x.Text())
            .Index("idx_variable_data_values_attribute", "AttributeId")
            .Index("idx_variable_data_values_entity", "EntityId")
            .ForeignKey("AttributeId", x => x.References(TableNames.VariableDataAttributes, "Id").OnDeleteCascade())
            .ForeignKey("EntityId", x => x.References(TableNames.VariableDataEntities, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.Locations)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Name", x => x.Varchar(255).NotNull())
            .Column("Type", x => x.Integer().NotNull())
            .Column("Position", x => x.Text().NotNull())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.AssistanceServiceTypeConfigs)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("TypeName", x => x.Varchar(255).NotNull())
            .Column("AssemblyName", x => x.Varchar(255).NotNull())
            .Column("Active", x => x.Boolean().NotNull().Default(0))
            .Column("Enabled", x => x.Boolean().NotNull().Default(0))
            .Column("SpawnType", x => x.Varchar(50).NotNull().Default("AUTO"))
            .Column("ManualSpawnLocations", x => x.Text())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.AssistanceServiceStations)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Name", x => x.Varchar(255).NotNull())
            .Column("Position", x => x.Text().NotNull())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.AssistanceServiceStationTypeConfigs)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("AssistanceServiceStationId", x => x.Integer().NotNull())
            .Column("TypeConfigId", x => x.Integer().NotNull())
            .Column("Capacity", x => x.Integer().NotNull())
            .Column("ActiveCapacity", x => x.Integer().NotNull())
            .Index("idx_assistance_station_type_configs_type", "TypeConfigId")
            .Index("idx_assistance_station", "AssistanceServiceStationId")
            .ForeignKey("TypeConfigId", x => x.References(TableNames.AssistanceServiceTypeConfigs, "Id").OnDeleteCascade())
            .ForeignKey("AssistanceServiceStationId", x => x.References(TableNames.AssistanceServiceStations, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.Weapons)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Name", x => x.Varchar(255).NotNull())
            .Column("Model", x => x.Varchar(255).NotNull())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.WeaponComponents)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Name", x => x.Varchar(255).NotNull())
            .Column("HashKey", x => x.Varchar(255).NotNull())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.LoadoutConfigs)
            .Column("Id", x => x.Integer().AutoIncrement().PrimaryKey())
            .Column("Name", x => x.Varchar(255).NotNull())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.LoadoutWeapon)
            .Column("LoadoutConfigId", x => x.Integer().NotNull())
            .Column("WeaponId", x => x.Integer().NotNull())
            .PrimaryKey("LoadoutConfigId", "WeaponId")
            .ForeignKey("LoadoutConfigId", x => x.References(TableNames.LoadoutConfigs, "Id").OnDeleteCascade())
            .ForeignKey("WeaponId", x => x.References(TableNames.Weapons, "Id").OnDeleteCascade())
            .Execute();

        new TableBuilder(connection, databaseType, TableNames.WeaponWeaponComponent)
            .Column("WeaponId", x => x.Integer().NotNull())
            .Column("WeaponComponentId", x => x.Integer().NotNull())
            .PrimaryKey("WeaponId", "WeaponComponentId")
            .ForeignKey("WeaponId", x => x.References(TableNames.Weapons, "Id").OnDeleteCascade())
            .ForeignKey("WeaponComponentId", x => x.References(TableNames.WeaponComponents, "Id").OnDeleteCascade())
            .Execute();
    }

    public void Down(IDbConnection connection, DatabaseType databaseType)
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