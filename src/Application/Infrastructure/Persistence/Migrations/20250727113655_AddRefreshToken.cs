using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerticalSliceArchitecture.Application.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpires",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpires",
                schema: "identity",
                table: "users");
        }
    }
}
