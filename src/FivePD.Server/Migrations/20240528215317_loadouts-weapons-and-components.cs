using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable

namespace FivePD.Server.Migrations
{
    public partial class loadoutsweaponsandcomponents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fivepd_loadout_configs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_loadout_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_weapon_components",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    HashKey = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_weapon_components", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_weapons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Model = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_weapons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_loadout_weapon",
                columns: table => new
                {
                    LoadoutConfigId = table.Column<int>(nullable: false),
                    WeaponId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_loadout_weapon", x => new { x.LoadoutConfigId, x.WeaponId });
                    table.ForeignKey(
                        name: "FK_fivepd_loadout_weapon_fivepd_loadout_configs_LoadoutConfigId",
                        column: x => x.LoadoutConfigId,
                        principalTable: "fivepd_loadout_configs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fivepd_loadout_weapon_fivepd_weapons_WeaponId",
                        column: x => x.WeaponId,
                        principalTable: "fivepd_weapons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_weapon_weaponComponent",
                columns: table => new
                {
                    WeaponId = table.Column<int>(nullable: false),
                    WeaponComponentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_weapon_weaponComponent", x => new { x.WeaponId, x.WeaponComponentId });
                    table.ForeignKey(
                        name: "FK_fivepd_weapon_weaponComponent_fivepd_weapon_components_Weapo~",
                        column: x => x.WeaponComponentId,
                        principalTable: "fivepd_weapon_components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fivepd_weapon_weaponComponent_fivepd_weapons_WeaponId",
                        column: x => x.WeaponId,
                        principalTable: "fivepd_weapons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_loadout_weapon_WeaponId",
                table: "fivepd_loadout_weapon",
                column: "WeaponId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_weapon_weaponComponent_WeaponComponentId",
                table: "fivepd_weapon_weaponComponent",
                column: "WeaponComponentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fivepd_loadout_weapon");

            migrationBuilder.DropTable(
                name: "fivepd_weapon_weaponComponent");

            migrationBuilder.DropTable(
                name: "fivepd_loadout_configs");

            migrationBuilder.DropTable(
                name: "fivepd_weapon_components");

            migrationBuilder.DropTable(
                name: "fivepd_weapons");
        }
    }
}
