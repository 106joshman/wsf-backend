using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSFBackendApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTableIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Teachings_Month",
                table: "Teachings",
                column: "Month",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_District",
                table: "Locations",
                column: "District");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_IsVerified_IsActive",
                table: "Locations",
                columns: new[] { "IsVerified", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Teachings_Month",
                table: "Teachings");

            migrationBuilder.DropIndex(
                name: "IX_Locations_District",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_IsVerified_IsActive",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_Name",
                table: "Locations");
        }
    }
}
