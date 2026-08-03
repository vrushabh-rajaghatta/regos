using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-007a S004 — where a document placed in a blueprint section is
    /// written on disk. One nullable column, and <b>every row will be null</b>.
    /// </summary>
    /// <remarks>
    /// <b>The shape is established; the values are not, and that is the whole
    /// point of shipping it empty.</b> A leaf's path cannot be derived from a
    /// section code without inventing a convention, and ICH Appendix 4 — which
    /// carries the directory table — is not in this repository. Appendix 8, the
    /// DTD, is the only part that was transcribed.
    /// <para>
    /// So this column says exactly what a null <c>Token</c> says on the three
    /// eCTD catalogues: <i>not in evidence</i>. Not "unknown", and emphatically
    /// not "work it out from the code". The alternative was a switch statement
    /// in a renderer, which would have buried regulatory knowledge in an
    /// algorithm — the one thing a metadata-driven system exists not to do.
    /// </para>
    /// <para>
    /// <b>Filling it in will be a new blueprint version, not an UPDATE.</b>
    /// Published versions are frozen (EPIC-007a S002) and the value is set at
    /// construction, so Appendix 4 arriving is a versioning event. That is the
    /// same reasoning ADR-045 §2 gives for freezing the operation: a package
    /// regenerated under a placement rule that changed after transmission would
    /// put files somewhere other than where the authority received them.
    /// </para>
    /// </remarks>
    public partial class AddTemplateSectionEctdFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EctdFolder",
                table: "TemplateSections",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EctdFolder",
                table: "TemplateSections");
        }
    }
}
