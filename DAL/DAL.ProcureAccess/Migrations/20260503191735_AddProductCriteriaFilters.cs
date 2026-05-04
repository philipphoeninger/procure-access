using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCriteriaFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductParts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProductTests",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProductTypes",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "ProductCriteriaFilters",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CriteriaFilterId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCriteriaFilters", x => new { x.Id, x.ProductId, x.CriteriaFilterId });
                    table.ForeignKey(
                        name: "FK_ProductCriteriaFilters_CriteriaFilters_CriteriaFilterId",
                        column: x => x.CriteriaFilterId,
                        principalSchema: "dbo",
                        principalTable: "CriteriaFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductCriteriaFilters_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCriteriaFilters_CriteriaFilterId",
                schema: "dbo",
                table: "ProductCriteriaFilters",
                column: "CriteriaFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCriteriaFilters_ProductId",
                schema: "dbo",
                table: "ProductCriteriaFilters",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCriteriaFilters",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "ProductParts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CriteriaFilterId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductParts", x => new { x.Id, x.ProductId, x.CriteriaFilterId });
                    table.ForeignKey(
                        name: "FK_ProductParts_CriteriaFilters_CriteriaFilterId",
                        column: x => x.CriteriaFilterId,
                        principalSchema: "dbo",
                        principalTable: "CriteriaFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductParts_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductTests",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CriteriaFilterId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTests", x => new { x.Id, x.ProductId, x.CriteriaFilterId });
                    table.ForeignKey(
                        name: "FK_ProductTests_CriteriaFilters_CriteriaFilterId",
                        column: x => x.CriteriaFilterId,
                        principalSchema: "dbo",
                        principalTable: "CriteriaFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductTests_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CriteriaFilterId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => new { x.Id, x.ProductId, x.CriteriaFilterId });
                    table.ForeignKey(
                        name: "FK_ProductTypes_CriteriaFilters_CriteriaFilterId",
                        column: x => x.CriteriaFilterId,
                        principalSchema: "dbo",
                        principalTable: "CriteriaFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductTypes_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_CriteriaFilterId",
                schema: "dbo",
                table: "ProductParts",
                column: "CriteriaFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_ProductId",
                schema: "dbo",
                table: "ProductParts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTests_CriteriaFilterId",
                schema: "dbo",
                table: "ProductTests",
                column: "CriteriaFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTests_ProductId",
                schema: "dbo",
                table: "ProductTests",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_CriteriaFilterId",
                schema: "dbo",
                table: "ProductTypes",
                column: "CriteriaFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_ProductId",
                schema: "dbo",
                table: "ProductTypes",
                column: "ProductId");
        }
    }
}
