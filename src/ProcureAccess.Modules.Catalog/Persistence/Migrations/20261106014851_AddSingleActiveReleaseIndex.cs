using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Catalog.Persistence.Migrations
{
    /// <summary>
    /// At most one active release (<c>catalog activate</c>, S3-17 decision 4): a unique index on
    /// <c>IsActive</c> over the active rows only, so any number of releases can be inactive.
    /// </summary>
    internal partial class AddSingleActiveReleaseIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CatalogReleases_SingleActive",
                schema: "catalog",
                table: "CatalogReleases",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CatalogReleases_SingleActive",
                schema: "catalog",
                table: "CatalogReleases");
        }
    }
}
