using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCriteriaFilterExclusions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriteriaFilterExclusions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriteriaFilterId = table.Column<int>(type: "int", nullable: false),
                    ExclusionId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaFilterExclusions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriaFilterExclusions_CriteriaFilters_CriteriaFilterId",
                        column: x => x.CriteriaFilterId,
                        principalSchema: "dbo",
                        principalTable: "CriteriaFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CriteriaFilterExclusions_CriteriaFilters_ExclusionId",
                        column: x => x.ExclusionId,
                        principalSchema: "dbo",
                        principalTable: "CriteriaFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaFilterExclusions_CriteriaFilterId_ExclusionId",
                schema: "dbo",
                table: "CriteriaFilterExclusions",
                columns: new[] { "CriteriaFilterId", "ExclusionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaFilterExclusions_ExclusionId",
                schema: "dbo",
                table: "CriteriaFilterExclusions",
                column: "ExclusionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CriteriaFilterExclusions",
                schema: "dbo");
        }
    }
}
