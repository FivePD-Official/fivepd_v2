using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable

namespace FivePD.Server.Migrations
{
    public partial class user_callsign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_ranks_fivepd_departments_DepartmentId",
                table: "fivepd_ranks");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_ranks_DepartmentId",
                table: "fivepd_ranks");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "fivepd_ranks");

            migrationBuilder.AddColumn<int>(
                name: "UserCallsignFormatId",
                table: "fivepd_users",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fivepd_user_callsigns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Values = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_user_callsigns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_users_UserCallsignFormatId",
                table: "fivepd_users",
                column: "UserCallsignFormatId");

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_users_fivepd_user_callsigns_UserCallsignFormatId",
                table: "fivepd_users",
                column: "UserCallsignFormatId",
                principalTable: "fivepd_user_callsigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_users_fivepd_user_callsigns_UserCallsignFormatId",
                table: "fivepd_users");

            migrationBuilder.DropTable(
                name: "fivepd_user_callsigns");

            migrationBuilder.DropIndex(
                name: "IX_fivepd_users_UserCallsignFormatId",
                table: "fivepd_users");

            migrationBuilder.DropColumn(
                name: "UserCallsignFormatId",
                table: "fivepd_users");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "fivepd_ranks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_ranks_DepartmentId",
                table: "fivepd_ranks",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_ranks_fivepd_departments_DepartmentId",
                table: "fivepd_ranks",
                column: "DepartmentId",
                principalTable: "fivepd_departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
