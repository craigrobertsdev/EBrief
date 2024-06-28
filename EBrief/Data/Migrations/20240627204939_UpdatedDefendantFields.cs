using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBrief.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDefendantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "DefendantModel",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "DefendantModel",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DefendantModel",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "DefendantModel",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "DefendantModel");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "DefendantModel");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DefendantModel");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "DefendantModel");
        }
    }
}
