using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSFBackendApi.Migrations
{
    /// <inheritdoc />
    public partial class LocationTableUpdateWithContactAndDistrictColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Locations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Locations",
                type: "varchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Locations");
        }
    }
}
