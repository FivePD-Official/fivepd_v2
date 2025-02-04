using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable

namespace FivePD.Server.Migrations
{
    public partial class ped_vehicle_history_to_data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_ped_licenses_fivepd_ped_histories_PedHistoryId",
                table: "fivepd_ped_licenses");

            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_vehicle_documents_fivepd_vehicle_histories_VehicleHis~",
                table: "fivepd_vehicle_documents");

            migrationBuilder.DropTable(
                name: "fivepd_vehicle_histories");

            migrationBuilder.DropTable(
                name: "fivepd_ped_histories");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_vehicle_documents_VehicleHistoryId",
                table: "fivepd_vehicle_documents");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_ped_licenses_PedHistoryId",
                table: "fivepd_ped_licenses");

            migrationBuilder.DropColumn(
                name: "VehicleHistoryId",
                table: "fivepd_vehicle_documents");

            migrationBuilder.DropColumn(
                name: "PedHistoryId",
                table: "fivepd_ped_licenses");

            migrationBuilder.AddColumn<int>(
                name: "VehicleDataId",
                table: "fivepd_vehicle_documents",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PedDataId",
                table: "fivepd_ped_licenses",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fivepd_ped_data",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VehicleHistoryId = table.Column<int>(nullable: true),
                    Gender = table.Column<int>(nullable: false),
                    Firstname = table.Column<string>(nullable: true),
                    Lastname = table.Column<string>(nullable: true),
                    Birthdate = table.Column<DateTime>(nullable: false),
                    Items = table.Column<string>(nullable: true),
                    Warrants = table.Column<string>(nullable: true),
                    AlcoholLevel = table.Column<double>(nullable: false),
                    Drugs = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_ped_data", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_vehicle_data",
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
                    table.PrimaryKey("PK_fivepd_vehicle_data", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_vehicle_data_fivepd_ped_data_PedHistoryId",
                        column: x => x.PedHistoryId,
                        principalTable: "fivepd_ped_data",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_documents_VehicleDataId",
                table: "fivepd_vehicle_documents",
                column: "VehicleDataId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_ped_licenses_PedDataId",
                table: "fivepd_ped_licenses",
                column: "PedDataId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_data_PedHistoryId",
                table: "fivepd_vehicle_data",
                column: "PedHistoryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_ped_licenses_fivepd_ped_data_PedDataId",
                table: "fivepd_ped_licenses",
                column: "PedDataId",
                principalTable: "fivepd_ped_data",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_vehicle_documents_fivepd_vehicle_data_VehicleDataId",
                table: "fivepd_vehicle_documents",
                column: "VehicleDataId",
                principalTable: "fivepd_vehicle_data",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_ped_licenses_fivepd_ped_data_PedDataId",
                table: "fivepd_ped_licenses");

            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_vehicle_documents_fivepd_vehicle_data_VehicleDataId",
                table: "fivepd_vehicle_documents");

            migrationBuilder.DropTable(
                name: "fivepd_vehicle_data");

            migrationBuilder.DropTable(
                name: "fivepd_ped_data");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_vehicle_documents_VehicleDataId",
                table: "fivepd_vehicle_documents");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_ped_licenses_PedDataId",
                table: "fivepd_ped_licenses");

            migrationBuilder.DropColumn(
                name: "VehicleDataId",
                table: "fivepd_vehicle_documents");

            migrationBuilder.DropColumn(
                name: "PedDataId",
                table: "fivepd_ped_licenses");

            migrationBuilder.AddColumn<int>(
                name: "VehicleHistoryId",
                table: "fivepd_vehicle_documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PedHistoryId",
                table: "fivepd_ped_licenses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fivepd_ped_histories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AlcoholLevel = table.Column<double>(type: "double", nullable: false),
                    Birthdate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Drugs = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Firstname = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Items = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Lastname = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    VehicleHistoryId = table.Column<int>(type: "int", nullable: true),
                    Warrants = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_ped_histories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_vehicle_histories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Brand = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Color = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Items = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    LicensePlate = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    PedHistoryId = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_documents_VehicleHistoryId",
                table: "fivepd_vehicle_documents",
                column: "VehicleHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_ped_licenses_PedHistoryId",
                table: "fivepd_ped_licenses",
                column: "PedHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_vehicle_histories_PedHistoryId",
                table: "fivepd_vehicle_histories",
                column: "PedHistoryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_ped_licenses_fivepd_ped_histories_PedHistoryId",
                table: "fivepd_ped_licenses",
                column: "PedHistoryId",
                principalTable: "fivepd_ped_histories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_vehicle_documents_fivepd_vehicle_histories_VehicleHis~",
                table: "fivepd_vehicle_documents",
                column: "VehicleHistoryId",
                principalTable: "fivepd_vehicle_histories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
