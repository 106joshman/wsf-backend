using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSFBackendApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameAdminNameToCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminName",
                table: "Teachings");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "Teachings",
                newName: "CreatedById");

            migrationBuilder.RenameColumn(
                name: "AdminName",
                table: "PrayerOutlines",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "PrayerOutlines",
                newName: "CreatedById");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Teachings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Teachings");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Teachings",
                newName: "AdminId");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "PrayerOutlines",
                newName: "AdminId");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "PrayerOutlines",
                newName: "AdminName");

            migrationBuilder.AddColumn<string>(
                name: "AdminName",
                table: "Teachings",
                type: "text",
                nullable: true);
        }
    }
}
