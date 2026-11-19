using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Migrations
{
    /// <summary>
    /// The state of the rating legend on the assessment tab S34b as a fifth user setting (S6-19b;
    /// SPEC R-CAN-08, §8.12; owner decision S6-19 Q1 (b)). Every stored row starts at <c>Auto</c>
    /// — expanded until the user's first assessment —, the value of a user who never saved any.
    /// Internal, like everything outside <c>Contracts</c> (ARC-03).
    /// </summary>
    internal partial class AssessmentLegendPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssessmentLegend",
                schema: "identity",
                table: "UserPreferences",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Auto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssessmentLegend",
                schema: "identity",
                table: "UserPreferences");
        }
    }
}
