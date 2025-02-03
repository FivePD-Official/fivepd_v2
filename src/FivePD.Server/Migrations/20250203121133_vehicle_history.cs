using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable

namespace FivePD.Server.Migrations
{
    public partial class vehicle_history : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VehicleHistoryId",
                table: "fivepd_ped_histories",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fivepd_vehicle_histories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PedHistoryId = table.Column<int>(nullable: true),
                    LicensePlate = table.Column<string>(nullable: true),
                    Color = table.Column<string>(nullable: true),
                    Brand = table.Column<string>(nullable: true),
                    Items = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_vehicle_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_vehicle_histories_fivepd_ped_histories_PedHistoryId",
                        column: x => x.PedHistoryId,
                        principalTable: "fivepd_ped_histories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_vehicle_documents",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    VehicleHistoryId = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    IssueDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_vehicle_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_vehicle_documents_fivepd_vehicle_histories_VehicleHis~",
                        column: x => x.VehicleHistoryId,
                        principalTable: "fivepd_vehicle_histories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_documents_VehicleHistoryId",
                table: "fivepd_vehicle_documents",
                column: "VehicleHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_histories_PedHistoryId",
                table: "fivepd_vehicle_histories",
                column: "PedHistoryId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fivepd_vehicle_documents");

            migrationBuilder.DropTable(
                name: "fivepd_vehicle_histories");

            migrationBuilder.DropColumn(
                name: "VehicleHistoryId",
                table: "fivepd_ped_histories");
        }
    }
}
