using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCriteriaFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CriteriaFilterId",
                schema: "dbo",
                table: "Criteria",
                type: "int",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.CreateTable(
                name: "CriteriaFilters",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    FilterTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetDate()")
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    Display = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, computedColumnSql: "[Name]", stored: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom"),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriaFilters_FilterTypes_FilterTypeId",
                        column: x => x.FilterTypeId,
                        principalSchema: "dbo",
                        principalTable: "FilterTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_CriteriaFilterId",
                schema: "dbo",
                table: "Criteria",
                column: "CriteriaFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaFilters_FilterTypeId",
                schema: "dbo",
                table: "CriteriaFilters",
                column: "FilterTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaFilters_Name",
                schema: "dbo",
                table: "CriteriaFilters",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Criteria_CriteriaFilters_CriteriaFilterId",
                schema: "dbo",
                table: "Criteria",
                column: "CriteriaFilterId",
                principalSchema: "dbo",
                principalTable: "CriteriaFilters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Criteria_CriteriaFilters_CriteriaFilterId",
                schema: "dbo",
                table: "Criteria");

            migrationBuilder.DropTable(
                name: "CriteriaFilters",
                schema: "dbo")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaFiltersAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");

            migrationBuilder.DropIndex(
                name: "IX_Criteria_CriteriaFilterId",
                schema: "dbo",
                table: "Criteria");

            migrationBuilder.DropColumn(
                name: "CriteriaFilterId",
                schema: "dbo",
                table: "Criteria")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CriteriaAudit")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidTo")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFrom");
        }
    }
}
