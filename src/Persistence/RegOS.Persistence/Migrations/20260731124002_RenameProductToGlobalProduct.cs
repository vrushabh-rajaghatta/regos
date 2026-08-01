using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductToGlobalProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductDocuments_Products_ProductId",
                table: "ProductDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Products_ProductId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryApplications_Products_ProductId",
                table: "RegulatoryApplications");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "RegulatoryApplications",
                newName: "GlobalProductId");

            migrationBuilder.RenameIndex(
                name: "IX_RegulatoryApplications_ProductId_CountryId_AuthorityId",
                table: "RegulatoryApplications",
                newName: "IX_RegulatoryApplications_GlobalProductId_CountryId_AuthorityId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Registrations",
                newName: "GlobalProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Registrations_ProductId",
                table: "Registrations",
                newName: "IX_Registrations_GlobalProductId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductDocuments",
                newName: "GlobalProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductDocuments_ProductId_Name",
                table: "ProductDocuments",
                newName: "IX_ProductDocuments_GlobalProductId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ProductDocuments_ProductId",
                table: "ProductDocuments",
                newName: "IX_ProductDocuments_GlobalProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDocuments_Products_GlobalProductId",
                table: "ProductDocuments",
                column: "GlobalProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Products_GlobalProductId",
                table: "Registrations",
                column: "GlobalProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryApplications_Products_GlobalProductId",
                table: "RegulatoryApplications",
                column: "GlobalProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductDocuments_Products_GlobalProductId",
                table: "ProductDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Products_GlobalProductId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryApplications_Products_GlobalProductId",
                table: "RegulatoryApplications");

            migrationBuilder.RenameColumn(
                name: "GlobalProductId",
                table: "RegulatoryApplications",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_RegulatoryApplications_GlobalProductId_CountryId_AuthorityId",
                table: "RegulatoryApplications",
                newName: "IX_RegulatoryApplications_ProductId_CountryId_AuthorityId");

            migrationBuilder.RenameColumn(
                name: "GlobalProductId",
                table: "Registrations",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Registrations_GlobalProductId",
                table: "Registrations",
                newName: "IX_Registrations_ProductId");

            migrationBuilder.RenameColumn(
                name: "GlobalProductId",
                table: "ProductDocuments",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductDocuments_GlobalProductId_Name",
                table: "ProductDocuments",
                newName: "IX_ProductDocuments_ProductId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ProductDocuments_GlobalProductId",
                table: "ProductDocuments",
                newName: "IX_ProductDocuments_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDocuments_Products_ProductId",
                table: "ProductDocuments",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Products_ProductId",
                table: "Registrations",
                column: "ProductId",
                principalTable: "Products",
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
    }
}
