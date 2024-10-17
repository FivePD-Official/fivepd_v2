using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable

namespace FivePD.Server.Migrations
{
    public partial class user_audio_volume : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AudioVolume",
                table: "fivepd_users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioVolume",
                table: "fivepd_users");
        }
    }
}
