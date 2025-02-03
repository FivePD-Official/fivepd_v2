using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable

namespace FivePD.Server.Migrations
{
    public partial class vehicle_history3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_vehicle_documents_fivepd_vehicle_histories_VehicleHis~",
                table: "fivepd_vehicle_documents");

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_vehicle_documents_fivepd_vehicle_histories_VehicleHis~",
                table: "fivepd_vehicle_documents",
                column: "VehicleHistoryId",
                principalTable: "fivepd_vehicle_histories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_vehicle_documents_fivepd_vehicle_histories_VehicleHis~",
                table: "fivepd_vehicle_documents");

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_vehicle_documents_fivepd_vehicle_histories_VehicleHis~",
                table: "fivepd_vehicle_documents",
                column: "VehicleHistoryId",
                principalTable: "fivepd_vehicle_histories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
