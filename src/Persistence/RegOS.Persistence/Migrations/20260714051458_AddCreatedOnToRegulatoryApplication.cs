using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedOnToRegulatoryApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill any existing rows with the current timestamp...
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "RegulatoryApplications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            // ...then drop the DB default so the column matches the model:
            // the application always supplies CreatedOn on insert.
            migrationBuilder.Sql(
                "ALTER TABLE \"RegulatoryApplications\" ALTER COLUMN \"CreatedOn\" DROP DEFAULT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "RegulatoryApplications");
        }
    }
}
