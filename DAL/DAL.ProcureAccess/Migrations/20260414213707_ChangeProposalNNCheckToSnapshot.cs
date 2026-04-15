using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProposalNNCheckToSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductId_CriterionId_NN",
                schema: "dbo",
                table: "Proposals");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Proposal_Snapshot_NN",
                schema: "dbo",
                table: "Proposals",
                sql: "([ProductSnapshot] IS NOT NULL AND [CriterionSnapshot] IS NULL) OR ([ProductSnapshot] IS NULL AND [CriterionSnapshot] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Proposal_Snapshot_NN",
                schema: "dbo",
                table: "Proposals");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductId_CriterionId_NN",
                schema: "dbo",
                table: "Proposals",
                sql: "([ProductId] IS NOT NULL AND [CriterionId] IS NULL) OR ([ProductId] IS NULL AND [CriterionId] IS NOT NULL)");
        }
    }
}
