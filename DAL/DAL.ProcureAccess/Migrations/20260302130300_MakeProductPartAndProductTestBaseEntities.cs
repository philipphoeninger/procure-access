using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeProductPartAndProductTestBaseEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductTests",
                schema: "dbo",
                table: "ProductTests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductParts",
                schema: "dbo",
                table: "ProductParts");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "dbo",
                table: "ProductTests",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<byte[]>(
                name: "TimeStamp",
                schema: "dbo",
                table: "ProductTests",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "dbo",
                table: "ProductParts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<byte[]>(
                name: "TimeStamp",
                schema: "dbo",
                table: "ProductParts",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductTests",
                schema: "dbo",
                table: "ProductTests",
                columns: new[] { "Id", "ProductId", "CriteriaFilterId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductParts",
                schema: "dbo",
                table: "ProductParts",
                columns: new[] { "Id", "ProductId", "CriteriaFilterId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTests_ProductId",
                schema: "dbo",
                table: "ProductTests",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_ProductId",
                schema: "dbo",
                table: "ProductParts",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductTests",
                schema: "dbo",
                table: "ProductTests");

            migrationBuilder.DropIndex(
                name: "IX_ProductTests_ProductId",
                schema: "dbo",
                table: "ProductTests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductParts",
                schema: "dbo",
                table: "ProductParts");

            migrationBuilder.DropIndex(
                name: "IX_ProductParts_ProductId",
                schema: "dbo",
                table: "ProductParts");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "dbo",
                table: "ProductTests");

            migrationBuilder.DropColumn(
                name: "TimeStamp",
                schema: "dbo",
                table: "ProductTests");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "dbo",
                table: "ProductParts");

            migrationBuilder.DropColumn(
                name: "TimeStamp",
                schema: "dbo",
                table: "ProductParts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductTests",
                schema: "dbo",
                table: "ProductTests",
                columns: new[] { "ProductId", "CriteriaFilterId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductParts",
                schema: "dbo",
                table: "ProductParts",
                columns: new[] { "ProductId", "CriteriaFilterId" });
        }
    }
}
