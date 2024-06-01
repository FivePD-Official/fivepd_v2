using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable

namespace FivePD.Server.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fivepd_callout_details",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    TypeName = table.Column<string>(nullable: true),
                    AssemblyName = table.Column<string>(nullable: true),
                    RulesetType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_callout_details", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_incident_histories",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    IncidentId = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    GeneratedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_incident_histories", x => x.Id);
                });

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
                name: "fivepd_locations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Position = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_ped_histories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
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
                    table.PrimaryKey("PK_fivepd_ped_histories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_variable_data_sets",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_variable_data_sets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_vehicle_configs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Model = table.Column<string>(nullable: true),
                    Livery = table.Column<int>(nullable: false),
                    Extras = table.Column<string>(nullable: true),
                    Options = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_vehicle_configs", x => x.Id);
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
                name: "fivepd_ped_licenses",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    PedHistoryId = table.Column<int>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    IssueDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_ped_licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_ped_licenses_fivepd_ped_histories_PedHistoryId",
                        column: x => x.PedHistoryId,
                        principalTable: "fivepd_ped_histories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_variable_data_attributes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SetId = table.Column<string>(nullable: true),
                    DataType = table.Column<int>(nullable: false),
                    AddonId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Required = table.Column<bool>(nullable: false),
                    Hidden = table.Column<bool>(nullable: false),
                    Readonly = table.Column<bool>(nullable: false),
                    ShowInGrid = table.Column<bool>(nullable: false),
                    Placeholder = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    HelpLabel = table.Column<string>(nullable: true),
                    Options = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_variable_data_attributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_variable_data_attributes_fivepd_variable_data_sets_Se~",
                        column: x => x.SetId,
                        principalTable: "fivepd_variable_data_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_variable_data_entities",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    SetId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_variable_data_entities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_variable_data_entities_fivepd_variable_data_sets_SetId",
                        column: x => x.SetId,
                        principalTable: "fivepd_variable_data_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "fivepd_variable_data_values",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AttributeId = table.Column<int>(nullable: true),
                    EntityId = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_variable_data_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_variable_data_values_fivepd_variable_data_attributes_~",
                        column: x => x.AttributeId,
                        principalTable: "fivepd_variable_data_attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fivepd_variable_data_values_fivepd_variable_data_entities_En~",
                        column: x => x.EntityId,
                        principalTable: "fivepd_variable_data_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_callout_details_departments",
                columns: table => new
                {
                    CalloutDetailsId = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_callout_details_departments", x => new { x.CalloutDetailsId, x.DepartmentId });
                    table.ForeignKey(
                        name: "FK_fivepd_callout_details_departments_fivepd_callout_details_Ca~",
                        column: x => x.CalloutDetailsId,
                        principalTable: "fivepd_callout_details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_department_area_coordinates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DepartmentId = table.Column<int>(nullable: true),
                    X = table.Column<float>(nullable: false),
                    Y = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_department_area_coordinates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_department_rank",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(nullable: false),
                    RankId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_department_rank", x => new { x.RankId, x.DepartmentId });
                });

            migrationBuilder.CreateTable(
                name: "fivepd_divisions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_divisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_ranks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Global = table.Column<bool>(nullable: false),
                    RequiredXp = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Permission_Kick = table.Column<bool>(nullable: true),
                    Permission_SetRank = table.Column<bool>(nullable: true),
                    Permission_SetDivision = table.Column<bool>(nullable: true),
                    Permission_DepartmentManager = table.Column<bool>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_ranks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_departments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Callsign = table.Column<string>(nullable: true),
                    UseGlobalRanks = table.Column<bool>(nullable: false),
                    CallsignFormat = table.Column<string>(nullable: true),
                    MaxUsers = table.Column<int>(nullable: false),
                    DefaultRankId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_departments_fivepd_ranks_DefaultRankId",
                        column: x => x.DefaultRankId,
                        principalTable: "fivepd_ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccessFivePD = table.Column<bool>(nullable: false),
                    PreferredLanguage = table.Column<string>(nullable: true),
                    FirstLogin = table.Column<bool>(nullable: false),
                    DismissedWelcomeMessage = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IdentifierLicense = table.Column<string>(nullable: true),
                    IdentifierDiscord = table.Column<string>(nullable: true),
                    IdentifierSteam = table.Column<string>(nullable: true),
                    IdentifierXbl = table.Column<string>(nullable: true),
                    IdentifierLive = table.Column<string>(nullable: true),
                    Xp = table.Column<int>(nullable: false),
                    Role = table.Column<int>(nullable: false),
                    Callsign = table.Column<string>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true),
                    DivisionId = table.Column<int>(nullable: true),
                    RankId = table.Column<int>(nullable: true),
                    FixedRank = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_users_fivepd_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "fivepd_departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fivepd_users_fivepd_divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "fivepd_divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fivepd_users_fivepd_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "fivepd_ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_incident_history_users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: true),
                    IncidentHistoryId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_incident_history_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_incident_history_users_fivepd_incident_histories_Inci~",
                        column: x => x.IncidentHistoryId,
                        principalTable: "fivepd_incident_histories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fivepd_incident_history_users_fivepd_users_UserId",
                        column: x => x.UserId,
                        principalTable: "fivepd_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fivepd_messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Sender = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    SentAt = table.Column<DateTime>(nullable: false),
                    Read = table.Column<bool>(nullable: false),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fivepd_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fivepd_messages_fivepd_users_UserId",
                        column: x => x.UserId,
                        principalTable: "fivepd_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_callout_details_departments_DepartmentId",
                table: "fivepd_callout_details_departments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_department_area_coordinates_DepartmentId",
                table: "fivepd_department_area_coordinates",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_department_rank_DepartmentId",
                table: "fivepd_department_rank",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_departments_DefaultRankId",
                table: "fivepd_departments",
                column: "DefaultRankId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_divisions_DepartmentId",
                table: "fivepd_divisions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_incident_history_users_IncidentHistoryId",
                table: "fivepd_incident_history_users",
                column: "IncidentHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_incident_history_users_UserId",
                table: "fivepd_incident_history_users",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_loadout_weapon_WeaponId",
                table: "fivepd_loadout_weapon",
                column: "WeaponId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_messages_UserId",
                table: "fivepd_messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_ped_licenses_PedHistoryId",
                table: "fivepd_ped_licenses",
                column: "PedHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_ranks_DepartmentId",
                table: "fivepd_ranks",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_users_DepartmentId",
                table: "fivepd_users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_users_DivisionId",
                table: "fivepd_users",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_users_RankId",
                table: "fivepd_users",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_variable_data_attributes_SetId",
                table: "fivepd_variable_data_attributes",
                column: "SetId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_variable_data_entities_SetId",
                table: "fivepd_variable_data_entities",
                column: "SetId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_variable_data_values_AttributeId",
                table: "fivepd_variable_data_values",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_variable_data_values_EntityId",
                table: "fivepd_variable_data_values",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_fivepd_weapon_weaponComponent_WeaponComponentId",
                table: "fivepd_weapon_weaponComponent",
                column: "WeaponComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_callout_details_departments_fivepd_departments_Depart~",
                table: "fivepd_callout_details_departments",
                column: "DepartmentId",
                principalTable: "fivepd_departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_department_area_coordinates_fivepd_departments_Depart~",
                table: "fivepd_department_area_coordinates",
                column: "DepartmentId",
                principalTable: "fivepd_departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_department_rank_fivepd_departments_DepartmentId",
                table: "fivepd_department_rank",
                column: "DepartmentId",
                principalTable: "fivepd_departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_department_rank_fivepd_ranks_RankId",
                table: "fivepd_department_rank",
                column: "RankId",
                principalTable: "fivepd_ranks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_divisions_fivepd_departments_DepartmentId",
                table: "fivepd_divisions",
                column: "DepartmentId",
                principalTable: "fivepd_departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fivepd_ranks_fivepd_departments_DepartmentId",
                table: "fivepd_ranks",
                column: "DepartmentId",
                principalTable: "fivepd_departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fivepd_ranks_fivepd_departments_DepartmentId",
                table: "fivepd_ranks");

            migrationBuilder.DropTable(
                name: "fivepd_callout_details_departments");

            migrationBuilder.DropTable(
                name: "fivepd_department_area_coordinates");

            migrationBuilder.DropTable(
                name: "fivepd_department_rank");

            migrationBuilder.DropTable(
                name: "fivepd_incident_history_users");

            migrationBuilder.DropTable(
                name: "fivepd_loadout_weapon");

            migrationBuilder.DropTable(
                name: "fivepd_locations");

            migrationBuilder.DropTable(
                name: "fivepd_messages");

            migrationBuilder.DropTable(
                name: "fivepd_ped_licenses");

            migrationBuilder.DropTable(
                name: "fivepd_variable_data_values");

            migrationBuilder.DropTable(
                name: "fivepd_vehicle_configs");

            migrationBuilder.DropTable(
                name: "fivepd_weapon_weaponComponent");

            migrationBuilder.DropTable(
                name: "fivepd_callout_details");

            migrationBuilder.DropTable(
                name: "fivepd_incident_histories");

            migrationBuilder.DropTable(
                name: "fivepd_loadout_configs");

            migrationBuilder.DropTable(
                name: "fivepd_users");

            migrationBuilder.DropTable(
                name: "fivepd_ped_histories");

            migrationBuilder.DropTable(
                name: "fivepd_variable_data_attributes");

            migrationBuilder.DropTable(
                name: "fivepd_variable_data_entities");

            migrationBuilder.DropTable(
                name: "fivepd_weapon_components");

            migrationBuilder.DropTable(
                name: "fivepd_weapons");

            migrationBuilder.DropTable(
                name: "fivepd_divisions");

            migrationBuilder.DropTable(
                name: "fivepd_variable_data_sets");

            migrationBuilder.DropTable(
                name: "fivepd_departments");

            migrationBuilder.DropTable(
                name: "fivepd_ranks");
        }
    }
}
