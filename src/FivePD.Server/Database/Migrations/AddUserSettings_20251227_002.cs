using System.Data;
using Dapper;

namespace FivePD.Server.Database.Migrations;

/// <summary>
/// Example migration: Add user settings table.
/// </summary>
public class AddUserSettings_20251227_002 : IMigration
{
    public string Id => "20251227_002";
    public string Description => "Adds user settings table for preferences";

    public void Up(IDbConnection connection)
    {
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
        connection.Execute("DROP TABLE IF EXISTS fivepd_user_settings");
    }
}