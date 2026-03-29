using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AutoControlAndCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoControlEnabled",
                table: "Rooms",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropForeignKey(
                name: "FK_AccessInvites_AspNetUsers_CreatedByUserId",
                table: "AccessInvites");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessInvites_AspNetUsers_CreatedByUserId",
                table: "AccessInvites",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropForeignKey(
                name: "FK_AccessInvites_AspNetUsers_UsedByUserId",
                table: "AccessInvites");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessInvites_AspNetUsers_UsedByUserId",
                table: "AccessInvites",
                column: "UsedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_AccessInvites_AspNetUsers_CreatedByUserId",
                table: "AccessInvites");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessInvites_AspNetUsers_CreatedByUserId",
                table: "AccessInvites",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_AccessInvites_AspNetUsers_UsedByUserId",
                table: "AccessInvites");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessInvites_AspNetUsers_UsedByUserId",
                table: "AccessInvites",
                column: "UsedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropColumn(
                name: "AutoControlEnabled",
                table: "Rooms");
        }
    }
}
