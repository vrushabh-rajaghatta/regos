using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryToRegulatoryApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryApplications_ApplicantOrganizationId",
                table: "RegulatoryApplications",
                column: "ApplicantOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryApplications_AuthorityId",
                table: "RegulatoryApplications",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryApplications_CountryId",
                table: "RegulatoryApplications",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryApplications_ProductId_CountryId_AuthorityId",
                table: "RegulatoryApplications",
                columns: new[] { "ProductId", "CountryId", "AuthorityId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryApplications_Authorities_AuthorityId",
                table: "RegulatoryApplications",
                column: "AuthorityId",
                principalTable: "Authorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryApplications_Countries_CountryId",
                table: "RegulatoryApplications",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryApplications_Organizations_ApplicantOrganizationId",
                table: "RegulatoryApplications",
                column: "ApplicantOrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryApplications_Products_ProductId",
                table: "RegulatoryApplications",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryApplications_Authorities_AuthorityId",
                table: "RegulatoryApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryApplications_Countries_CountryId",
                table: "RegulatoryApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryApplications_Organizations_ApplicantOrganizationId",
                table: "RegulatoryApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryApplications_Products_ProductId",
                table: "RegulatoryApplications");

            migrationBuilder.DropIndex(
                name: "IX_RegulatoryApplications_ApplicantOrganizationId",
                table: "RegulatoryApplications");

            migrationBuilder.DropIndex(
                name: "IX_RegulatoryApplications_AuthorityId",
                table: "RegulatoryApplications");

            migrationBuilder.DropIndex(
                name: "IX_RegulatoryApplications_CountryId",
                table: "RegulatoryApplications");

            migrationBuilder.DropIndex(
                name: "IX_RegulatoryApplications_ProductId_CountryId_AuthorityId",
                table: "RegulatoryApplications");
        }
    }
}
