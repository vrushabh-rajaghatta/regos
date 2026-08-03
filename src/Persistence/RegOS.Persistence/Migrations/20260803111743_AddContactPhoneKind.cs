using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// Office, fax or mobile — a business fact about a phone number, added
    /// because a number without it is incomplete, not because FDA asks for it.
    /// </summary>
    /// <remarks>
    /// <b>Additive, nullable, and deliberately not backfilled.</b> Every
    /// existing row gets null, and null means <em>recorded before RegOS
    /// asked</em>. Defaulting to 'Business' would have been the convenient
    /// choice and would have asserted, for every number already stored, an
    /// answer nobody was ever offered the chance to give — the mistake S001's
    /// migration refused when it declined to invent an application's
    /// classification.
    /// <para>
    /// Stored as text rather than an ordinal, so a reader of the database sees
    /// 'Mobile' and so reordering the enum cannot silently change what existing
    /// rows mean.
    /// </para>
    /// <para>
    /// <b>The <c>Down</c> is lossy, and honestly so:</b> dropping the column
    /// discards a fact a user supplied, and nothing can recreate it.
    /// </para>
    /// </remarks>
    public partial class AddContactPhoneKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "ContactPhones",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "ContactPhones");
        }
    }
}
