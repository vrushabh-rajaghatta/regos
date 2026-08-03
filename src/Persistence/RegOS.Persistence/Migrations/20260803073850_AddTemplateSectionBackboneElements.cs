using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-007a S005 — what a section is called in each backbone.
    /// </summary>
    /// <remarks>
    /// <b>Two columns, not one, because a backbone is a contract</b> (evidence
    /// E16). The names come from different DTDs and the split runs opposite
    /// ways: ICH declares one Module 1 element and the whole of Modules 2-5's
    /// structure, FDA declares 147 for Module 1 and nothing above it.
    /// <para>
    /// <b>Unlike the folder column, these carry no provenance — and cannot need
    /// one.</b> RegOS can never invent an element name, because an invented one
    /// is DTD-invalid. The format forecloses the failure mode
    /// <c>EctdFolderSource</c> exists to expose, and a seed test asserts every
    /// value is declared in the DTD the package ships.
    /// </para>
    /// </remarks>
    public partial class AddTemplateSectionBackboneElements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IchElement",
                table: "TemplateSections",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionalElement",
                table: "TemplateSections",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IchElement",
                table: "TemplateSections");

            migrationBuilder.DropColumn(
                name: "RegionalElement",
                table: "TemplateSections");
        }
    }
}
