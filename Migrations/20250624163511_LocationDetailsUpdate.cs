using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSFBackendApi.Migrations
{
    /// <inheritdoc />
    public partial class LocationDetailsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "District",
                table: "Locations",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(4)",
                oldMaxLength: 4)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // migrationBuilder.AddColumn<string>(
            //     name: "Country",
            //     table: "Locations",
            //     type: "longtext",
            //     nullable: false)
            //     .Annotation("MySql:CharSet", "utf8mb4");

            // migrationBuilder.AddColumn<string>(
            //     name: "LGA",
            //     table: "Locations",
            //     type: "longtext",
            //     nullable: false)
            //     .Annotation("MySql:CharSet", "utf8mb4");

            // migrationBuilder.AddColumn<string>(
            //     name: "State",
            //     table: "Locations",
            //     type: "longtext",
            //     nullable: false)
            //     .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "Country",
            //     table: "Locations");

            // migrationBuilder.DropColumn(
            //     name: "LGA",
            //     table: "Locations");

            // migrationBuilder.DropColumn(
            //     name: "State",
            //     table: "Locations");

            migrationBuilder.AlterColumn<string>(
                name: "District",
                table: "Locations",
                type: "varchar(4)",
                maxLength: 4,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
