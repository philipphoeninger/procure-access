using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCriteriaFilterProductsRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CriteriaFilters_FilterTypes_FilterTypeId",
                schema: "dbo",
                table: "CriteriaFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_CriteriaFilters_Products_ProductIdPart",
                schema: "dbo",
                table: "CriteriaFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_CriteriaFilters_Products_ProductIdTest",
                schema: "dbo",
                table: "CriteriaFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CriteriaFilters_TypeId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Link",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_CriteriaFilters_ProductIdPart",
                schema: "dbo",
                table: "CriteriaFilters");

            migrationBuilder.DropIndex(
                name: "IX_CriteriaFilters_ProductIdTest",
                schema: "dbo",
                table: "CriteriaFilters");

            migrationBuilder.DropColumn(
                name: "ProductIdPart",
                schema: "dbo",
                table: "CriteriaFilters")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.DropColumn(
                name: "ProductIdTest",
                schema: "dbo",
                table: "CriteriaFilters")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                schema: "dbo",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ProductsAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "ProductsAudit")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Products",
                type: "nvarchar(max)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 200)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ProductsAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "ProductsAudit")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.CreateTable(
                name: "ProductParts",
                schema: "dbo",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CriteriaFilterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductParts", x => new { x.ProductId, x.CriteriaFilterId });
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
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CriteriaFilterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTests", x => new { x.ProductId, x.CriteriaFilterId });
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

            migrationBuilder.CreateIndex(
                name: "IX_Products_Link",
                schema: "dbo",
                table: "Products",
                column: "Link",
                unique: true,
                filter: "[Link] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_CriteriaFilterId",
                schema: "dbo",
                table: "ProductParts",
                column: "CriteriaFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTests_CriteriaFilterId",
                schema: "dbo",
                table: "ProductTests",
                column: "CriteriaFilterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CriteriaFilters_FilterTypes_FilterTypeId",
                schema: "dbo",
                table: "CriteriaFilters",
                column: "FilterTypeId",
                principalSchema: "dbo",
                principalTable: "FilterTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CriteriaFilters_TypeId",
                schema: "dbo",
                table: "Products",
                column: "TypeId",
                principalSchema: "dbo",
                principalTable: "CriteriaFilters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CriteriaFilters_FilterTypes_FilterTypeId",
                schema: "dbo",
                table: "CriteriaFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CriteriaFilters_TypeId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductParts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProductTests",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Products_Link",
                schema: "dbo",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                schema: "dbo",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ProductsAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "ProductsAudit")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Products",
                type: "nvarchar(max)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 200,
                oldNullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ProductsAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "ProductsAudit")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.AddColumn<int>(
                name: "ProductIdPart",
                schema: "dbo",
                table: "CriteriaFilters",
                type: "int",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.AddColumn<int>(
                name: "ProductIdTest",
                schema: "dbo",
                table: "CriteriaFilters",
                type: "int",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Link",
                schema: "dbo",
                table: "Products",
                column: "Link",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaFilters_ProductIdPart",
                schema: "dbo",
                table: "CriteriaFilters",
                column: "ProductIdPart");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaFilters_ProductIdTest",
                schema: "dbo",
                table: "CriteriaFilters",
                column: "ProductIdTest");

            migrationBuilder.AddForeignKey(
                name: "FK_CriteriaFilters_FilterTypes_FilterTypeId",
                schema: "dbo",
                table: "CriteriaFilters",
                column: "FilterTypeId",
                principalSchema: "dbo",
                principalTable: "FilterTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CriteriaFilters_Products_ProductIdPart",
                schema: "dbo",
                table: "CriteriaFilters",
                column: "ProductIdPart",
                principalSchema: "dbo",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CriteriaFilters_Products_ProductIdTest",
                schema: "dbo",
                table: "CriteriaFilters",
                column: "ProductIdTest",
                principalSchema: "dbo",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CriteriaFilters_TypeId",
                schema: "dbo",
                table: "Products",
                column: "TypeId",
                principalSchema: "dbo",
                principalTable: "CriteriaFilters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
