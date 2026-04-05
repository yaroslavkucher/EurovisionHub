using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EurovisionHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceToParticipation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ми кажемо EF просто додати колонку до вже існуючої таблиці
            migrationBuilder.AddColumn<int>(
                name: "Place",
                table: "Participation",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Якщо захочемо відкотити міграцію — видаляємо цю колонку
            migrationBuilder.DropColumn(
                name: "Place",
                table: "Participation");
        }
    }
}
