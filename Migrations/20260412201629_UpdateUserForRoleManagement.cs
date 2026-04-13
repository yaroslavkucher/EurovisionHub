using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EurovisionHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserForRoleManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleChangeComment",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowRoleChangeNotification",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleChangeComment",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ShowRoleChangeNotification",
                table: "AspNetUsers");
        }
    }
}
