using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable

namespace FivePD.Server.Migrations
{
    public partial class ped_vehicle_data_indexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                table: "fivepd_vehicle_data",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "fivepd_vehicle_data",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "fivepd_vehicle_data",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Lastname",
                table: "fivepd_ped_data",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Firstname",
                table: "fivepd_ped_data",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_data_Brand",
                table: "fivepd_vehicle_data",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_data_Color",
                table: "fivepd_vehicle_data",
                column: "Color");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_data_LicensePlate",
                table: "fivepd_vehicle_data",
                column: "LicensePlate");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_ped_data_Birthdate",
                table: "fivepd_ped_data",
                column: "Birthdate");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_ped_data_Firstname",
                table: "fivepd_ped_data",
                column: "Firstname");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_ped_data_Lastname",
                table: "fivepd_ped_data",
                column: "Lastname");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_fivepd_vehicle_data_Brand",
                table: "fivepd_vehicle_data");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_vehicle_data_Color",
                table: "fivepd_vehicle_data");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_vehicle_data_LicensePlate",
                table: "fivepd_vehicle_data");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_ped_data_Birthdate",
                table: "fivepd_ped_data");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_ped_data_Firstname",
                table: "fivepd_ped_data");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_ped_data_Lastname",
                table: "fivepd_ped_data");

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                table: "fivepd_vehicle_data",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "fivepd_vehicle_data",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "fivepd_vehicle_data",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Lastname",
                table: "fivepd_ped_data",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Firstname",
                table: "fivepd_ped_data",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
