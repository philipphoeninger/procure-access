using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.ProcureAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssociationEntitiesNotUsingBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCriteriaFilters",
                schema: "public",
                table: "ProductCriteriaFilters");

            migrationBuilder.DropIndex(
                name: "IX_ProductCriteriaFilters_ProductId",
                schema: "public",
                table: "ProductCriteriaFilters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CriteriaFilterExclusions",
                schema: "public",
                table: "CriteriaFilterExclusions");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 1, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 2, 2 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 3, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 4, 4 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 5, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 6, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 7, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 8, 8 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 9, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 10, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 11, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 12, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 13, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 14, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 15, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 16, 16 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 1, 17, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 2, 18, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 3, 19, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 2, 20, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 3, 21, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 2, 22, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 3, 23, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 2, 24, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 3, 25, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 2, 26, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 3, 27, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 2, 28, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 3, 29, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 2, 30, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 3, 31, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 4, 32, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 5, 33, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 6, 34, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 4, 35, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 5, 36, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 6, 37, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 7, 38, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 23, 39, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 24, 40, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 24, 41, 2 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 23, 42, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 24, 43, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 25, 44, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 26, 45, 4 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 23, 46, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 25, 47, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 26, 48, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 24, 49, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 26, 50, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 26, 51, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 25, 52, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 23, 53, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 24, 54, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 23, 55, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 26, 56, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 8, 57, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 14, 58, 2 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 15, 59, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 11, 60, 4 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 12, 61, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 8, 62, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 9, 63, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 15, 64, 8 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 8, 65, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 8, 66, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 8, 67, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 13, 68, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 10, 69, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 8, 70, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 8, 71, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 14, 72, 16 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 9, 73, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyColumnTypes: new[] { "integer", "integer", "integer" },
                keyValues: new object[] { 17, 74, 13 });

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "public",
                table: "ProductCriteriaFilters");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "public",
                table: "ProductCriteriaFilters");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "public",
                table: "ProductCriteriaFilters");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "public",
                table: "CriteriaFilterExclusions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "public",
                table: "CriteriaFilterExclusions");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "public",
                table: "CriteriaFilterExclusions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCriteriaFilters",
                schema: "public",
                table: "ProductCriteriaFilters",
                columns: new[] { "ProductId", "CriteriaFilterId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CriteriaFilterExclusions",
                schema: "public",
                table: "CriteriaFilterExclusions",
                columns: new[] { "CriteriaFilterId", "ExclusionId" });

            migrationBuilder.InsertData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                columns: new[] { "CriteriaFilterId", "ExclusionId" },
                values: new object[,]
                {
                    { 8, 24 },
                    { 8, 25 },
                    { 8, 27 },
                    { 17, 9 },
                    { 17, 12 },
                    { 17, 13 },
                    { 17, 14 },
                    { 17, 15 },
                    { 17, 25 }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "ProductCriteriaFilters",
                columns: new[] { "CriteriaFilterId", "ProductId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 8, 1 },
                    { 23, 1 },
                    { 24, 1 },
                    { 1, 2 },
                    { 14, 2 },
                    { 24, 2 },
                    { 1, 3 },
                    { 15, 3 },
                    { 23, 3 },
                    { 24, 3 },
                    { 25, 3 },
                    { 1, 4 },
                    { 11, 4 },
                    { 26, 4 },
                    { 1, 5 },
                    { 4, 5 },
                    { 5, 5 },
                    { 6, 5 },
                    { 12, 5 },
                    { 23, 5 },
                    { 1, 6 },
                    { 2, 6 },
                    { 3, 6 },
                    { 8, 6 },
                    { 1, 7 },
                    { 9, 7 },
                    { 25, 7 },
                    { 26, 7 },
                    { 1, 8 },
                    { 15, 8 },
                    { 1, 9 },
                    { 2, 9 },
                    { 3, 9 },
                    { 8, 9 },
                    { 1, 10 },
                    { 2, 10 },
                    { 3, 10 },
                    { 8, 10 },
                    { 24, 10 },
                    { 26, 10 },
                    { 1, 11 },
                    { 2, 11 },
                    { 3, 11 },
                    { 8, 11 },
                    { 26, 11 },
                    { 1, 12 },
                    { 4, 12 },
                    { 5, 12 },
                    { 6, 12 },
                    { 13, 12 },
                    { 25, 12 },
                    { 1, 13 },
                    { 7, 13 },
                    { 10, 13 },
                    { 17, 13 },
                    { 1, 14 },
                    { 2, 14 },
                    { 3, 14 },
                    { 8, 14 },
                    { 23, 14 },
                    { 1, 15 },
                    { 2, 15 },
                    { 3, 15 },
                    { 8, 15 },
                    { 24, 15 },
                    { 1, 16 },
                    { 14, 16 },
                    { 1, 17 },
                    { 9, 17 },
                    { 23, 17 },
                    { 26, 17 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCriteriaFilters_ProductId_CriteriaFilterId",
                schema: "public",
                table: "ProductCriteriaFilters",
                columns: new[] { "ProductId", "CriteriaFilterId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCriteriaFilters",
                schema: "public",
                table: "ProductCriteriaFilters");

            migrationBuilder.DropIndex(
                name: "IX_ProductCriteriaFilters_ProductId_CriteriaFilterId",
                schema: "public",
                table: "ProductCriteriaFilters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CriteriaFilterExclusions",
                schema: "public",
                table: "CriteriaFilterExclusions");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumns: new[] { "CriteriaFilterId", "ExclusionId" },
                keyValues: new object[] { 8, 24 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumns: new[] { "CriteriaFilterId", "ExclusionId" },
                keyValues: new object[] { 8, 25 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumns: new[] { "CriteriaFilterId", "ExclusionId" },
                keyValues: new object[] { 8, 27 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumns: new[] { "CriteriaFilterId", "ExclusionId" },
                keyValues: new object[] { 17, 9 });

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

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumns: new[] { "CriteriaFilterId", "ExclusionId" },
                keyValues: new object[] { 17, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumns: new[] { "CriteriaFilterId", "ExclusionId" },
                keyValues: new object[] { 17, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumns: new[] { "CriteriaFilterId", "ExclusionId" },
                keyValues: new object[] { 17, 25 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 8, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 23, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 24, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 14, 2 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 24, 2 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 15, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 23, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 24, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 25, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 4 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 11, 4 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 26, 4 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 4, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 5, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 6, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 12, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 23, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 2, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 3, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 8, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 9, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 25, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 26, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 8 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 15, 8 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 2, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 3, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 8, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 2, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 3, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 8, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 24, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 26, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 2, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 3, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 8, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 26, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 4, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 5, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 6, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 13, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 25, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 7, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 10, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 17, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 2, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 3, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 8, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 23, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 2, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 3, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 8, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 24, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 16 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 14, 16 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 1, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 9, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 23, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "ProductId" },
                keyValues: new object[] { 26, 17 });

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "public",
                table: "ProductCriteriaFilters",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "public",
                table: "ProductCriteriaFilters",
                type: "timestamptz",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "public",
                table: "ProductCriteriaFilters",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "public",
                table: "CriteriaFilterExclusions",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "public",
                table: "CriteriaFilterExclusions",
                type: "timestamptz",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "public",
                table: "CriteriaFilterExclusions",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCriteriaFilters",
                schema: "public",
                table: "ProductCriteriaFilters",
                columns: new[] { "Id", "ProductId", "CriteriaFilterId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CriteriaFilterExclusions",
                schema: "public",
                table: "CriteriaFilterExclusions",
                column: "Id");

            migrationBuilder.InsertData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                columns: new[] { "Id", "CriteriaFilterId", "ExclusionId" },
                values: new object[,]
                {
                    { 1, 17, 9 },
                    { 2, 17, 12 },
                    { 3, 17, 13 },
                    { 4, 17, 14 },
                    { 5, 17, 15 },
                    { 6, 17, 25 },
                    { 7, 8, 24 },
                    { 8, 8, 25 },
                    { 9, 8, 27 }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "ProductCriteriaFilters",
                columns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 1, 2, 2 },
                    { 1, 3, 3 },
                    { 1, 4, 4 },
                    { 1, 5, 5 },
                    { 1, 6, 6 },
                    { 1, 7, 7 },
                    { 1, 8, 8 },
                    { 1, 9, 9 },
                    { 1, 10, 10 },
                    { 1, 11, 11 },
                    { 1, 12, 12 },
                    { 1, 13, 13 },
                    { 1, 14, 14 },
                    { 1, 15, 15 },
                    { 1, 16, 16 },
                    { 1, 17, 17 },
                    { 2, 18, 1 },
                    { 3, 19, 1 },
                    { 2, 20, 6 },
                    { 3, 21, 6 },
                    { 2, 22, 9 },
                    { 3, 23, 9 },
                    { 2, 24, 10 },
                    { 3, 25, 10 },
                    { 2, 26, 11 },
                    { 3, 27, 11 },
                    { 2, 28, 14 },
                    { 3, 29, 14 },
                    { 2, 30, 15 },
                    { 3, 31, 15 },
                    { 4, 32, 5 },
                    { 5, 33, 5 },
                    { 6, 34, 5 },
                    { 4, 35, 12 },
                    { 5, 36, 12 },
                    { 6, 37, 12 },
                    { 7, 38, 13 },
                    { 23, 39, 1 },
                    { 24, 40, 1 },
                    { 24, 41, 2 },
                    { 23, 42, 3 },
                    { 24, 43, 3 },
                    { 25, 44, 3 },
                    { 26, 45, 4 },
                    { 23, 46, 5 },
                    { 25, 47, 7 },
                    { 26, 48, 7 },
                    { 24, 49, 10 },
                    { 26, 50, 10 },
                    { 26, 51, 11 },
                    { 25, 52, 12 },
                    { 23, 53, 14 },
                    { 24, 54, 15 },
                    { 23, 55, 17 },
                    { 26, 56, 17 },
                    { 8, 57, 1 },
                    { 14, 58, 2 },
                    { 15, 59, 3 },
                    { 11, 60, 4 },
                    { 12, 61, 5 },
                    { 8, 62, 6 },
                    { 9, 63, 7 },
                    { 15, 64, 8 },
                    { 8, 65, 9 },
                    { 8, 66, 10 },
                    { 8, 67, 11 },
                    { 13, 68, 12 },
                    { 10, 69, 13 },
                    { 8, 70, 14 },
                    { 8, 71, 15 },
                    { 14, 72, 16 },
                    { 9, 73, 17 },
                    { 17, 74, 13 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCriteriaFilters_ProductId",
                schema: "public",
                table: "ProductCriteriaFilters",
                column: "ProductId");
        }
    }
}
