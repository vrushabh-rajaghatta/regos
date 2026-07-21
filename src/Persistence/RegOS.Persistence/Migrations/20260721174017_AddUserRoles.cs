using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill, hand-written. Every existing user becomes a Member
            // (3) — least privilege, and the scaffolded 0 is not a defined
            // enum value so leaving it would be a lie waiting to deserialize.
            // The two development seed accounts get the roles their seeders
            // now create them with; matching by their fixed seeded emails is
            // safe because both exist only in development databases, and the
            // UPDATE matches nothing anywhere else.
            migrationBuilder.Sql(
                """
                UPDATE "Users" SET "Role" = 3;

                UPDATE "Users" SET "Role" = 2
                WHERE "Email" = 'dev@regos.local';

                UPDATE "Users" SET "Role" = 1
                WHERE "Email" = 'platform@regos.local';

                ALTER TABLE "Users" ALTER COLUMN "Role" DROP DEFAULT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
