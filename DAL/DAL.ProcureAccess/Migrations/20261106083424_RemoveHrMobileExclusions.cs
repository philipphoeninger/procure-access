using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.ProcureAccess.Migrations
{
    /// <summary>
    /// Removes the seed exclusions HR-Anwendung (17) → Mobile Anwendung (offen) (12) and
    /// → Mobile Anwendung (geschlossen) (13): HR-Anwendung does not exclude the mobile app types
    /// (owner, 2026-10-06; ARCHITECTURE §16.3.2, C103; S3-18).
    /// </summary>
    public partial class RemoveHrMobileExclusions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumns: new[] { "CriteriaFilterId", "ExclusionId" },
                keyValues: new object[] { 17, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumns: new[] { "CriteriaFilterId", "ExclusionId" },
                keyValues: new object[] { 17, 13 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                columns: new[] { "CriteriaFilterId", "ExclusionId" },
                values: new object[,]
                {
                    { 17, 12 },
                    { 17, 13 }
                });
        }
    }
}
