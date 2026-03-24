using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccessInviteAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CoolerState",
                table: "Measurements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HeaterState",
                table: "Measurements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CoolerState",
                table: "Devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HeaterState",
                table: "Devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationSecret",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccessInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessInvites_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccessInvites_AspNetUsers_UsedByUserId",
                        column: x => x.UsedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccessInvites_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessInvites_BuildingId",
                table: "AccessInvites",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessInvites_CreatedByUserId",
                table: "AccessInvites",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessInvites_Token",
                table: "AccessInvites",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessInvites_UsedByUserId",
                table: "AccessInvites",
                column: "UsedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessInvites");

            migrationBuilder.DropColumn(
                name: "CoolerState",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "HeaterState",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "CoolerState",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "HeaterState",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RegistrationSecret",
                table: "Devices");
        }
    }
}
