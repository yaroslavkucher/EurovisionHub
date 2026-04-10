using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EurovisionHub.Migrations
{
    /// <inheritdoc />
    public partial class AddMotivationToRoleRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Motivation",
                table: "RoleRequests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Motivation",
                table: "RoleRequests");
        }
    }
}
