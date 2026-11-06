using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Catalog.Persistence.Migrations
{
    /// <summary>
    /// Unique indexes the import relies on when it reuses rows (S3-16): a standard reference per
    /// standard, clause and URL, so a corrected URL is a new row (owner decision 5), and an exclusion
    /// rule per source, target and reason (S3-13 P1). Missing values count as equal
    /// (<c>NULLS NOT DISTINCT</c>, PostgreSQL 15+). The rule index starts with
    /// <c>SourceUseCaseId</c>, so the foreign-key index on it is no longer needed.
    /// </summary>
    internal partial class AddImportUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StandardReferences_Standard_Clause",
                schema: "catalog",
                table: "StandardReferences");

            migrationBuilder.DropIndex(
                name: "IX_FilterExclusions_SourceUseCaseId",
                schema: "catalog",
                table: "FilterExclusions");

            migrationBuilder.CreateIndex(
                name: "IX_StandardReferences_Standard_Clause_Url",
                schema: "catalog",
                table: "StandardReferences",
                columns: new[] { "Standard", "Clause", "Url" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_FilterExclusions_Rule",
                schema: "catalog",
                table: "FilterExclusions",
                columns: new[] { "SourceUseCaseId", "SourceAppTypeId", "SourceProductPartId", "TargetAppTypeId", "TargetProductPartId", "TargetTestTypeId", "ReasonDe" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StandardReferences_Standard_Clause_Url",
                schema: "catalog",
                table: "StandardReferences");

            migrationBuilder.DropIndex(
                name: "IX_FilterExclusions_Rule",
                schema: "catalog",
                table: "FilterExclusions");

            migrationBuilder.CreateIndex(
                name: "IX_StandardReferences_Standard_Clause",
                schema: "catalog",
                table: "StandardReferences",
                columns: new[] { "Standard", "Clause" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilterExclusions_SourceUseCaseId",
                schema: "catalog",
                table: "FilterExclusions",
                column: "SourceUseCaseId");
        }
    }
}
