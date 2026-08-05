using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkUserCredentialToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The constraint cannot be added while orphans exist. Unlike the
            // duplicate emails that blocked MakeUserEmailGloballyUnique, this
            // needs no judgment: a credential whose user is gone is unreachable
            // by every code path, so deleting it is the only remediation there
            // is. Nothing that could still be signed in to is removed.
            migrationBuilder.Sql(
                """
                DELETE FROM "UserCredentials"
                WHERE "UserId" NOT IN (SELECT "Id" FROM "Users");
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCredentials_Users_UserId",
                table: "UserCredentials",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCredentials_Users_UserId",
                table: "UserCredentials");
        }
    }
}
