using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable

namespace FivePD.Server.Migrations
{
    public partial class ped_vehicle_history_to_data2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_vehicle_data_fivepd_ped_data_PedHistoryId",
                table: "fivepd_vehicle_data");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_vehicle_data_PedHistoryId",
                table: "fivepd_vehicle_data");

            migrationBuilder.DropColumn(
                name: "PedHistoryId",
                table: "fivepd_vehicle_data");

            migrationBuilder.DropColumn(
                name: "VehicleHistoryId",
                table: "fivepd_ped_data");

            migrationBuilder.AddColumn<int>(
                name: "PedDataId",
                table: "fivepd_vehicle_data",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleDataId",
                table: "fivepd_ped_data",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_data_PedDataId",
                table: "fivepd_vehicle_data",
                column: "PedDataId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_vehicle_data_fivepd_ped_data_PedDataId",
                table: "fivepd_vehicle_data",
                column: "PedDataId",
                principalTable: "fivepd_ped_data",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_vehicle_data_fivepd_ped_data_PedDataId",
                table: "fivepd_vehicle_data");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_vehicle_data_PedDataId",
                table: "fivepd_vehicle_data");

            migrationBuilder.DropColumn(
                name: "PedDataId",
                table: "fivepd_vehicle_data");

            migrationBuilder.DropColumn(
                name: "VehicleDataId",
                table: "fivepd_ped_data");

            migrationBuilder.AddColumn<int>(
                name: "PedHistoryId",
                table: "fivepd_vehicle_data",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleHistoryId",
                table: "fivepd_ped_data",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_data_PedHistoryId",
                table: "fivepd_vehicle_data",
                column: "PedHistoryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_vehicle_data_fivepd_ped_data_PedHistoryId",
                table: "fivepd_vehicle_data",
                column: "PedHistoryId",
                principalTable: "fivepd_ped_data",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
