using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSFBackendApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPushNotificationTokensUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableOutlineNotifications",
                table: "PushNotificationTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableOutlineNotifications",
                table: "PushNotificationTokens");
        }
    }
}
