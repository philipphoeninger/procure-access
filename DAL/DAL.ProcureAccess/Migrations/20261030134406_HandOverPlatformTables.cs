using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <summary>
    /// Hands <c>logging."SeriLogs"</c> and <c>analytics."UsageEvents"</c> over to the Platform
    /// module's own context (S2-07, ARCHITECTURE ADR-014). The tables stay as they are: only the
    /// model of this context forgets them. From here on the Platform migrations own them; their
    /// first migration adopts the existing tables (it creates them only where they are missing).
    /// </summary>
    /// <remarks>
    /// EF scaffolds <c>DropTable</c> here because the entities left the model. Never let it drop
    /// them: the log rows and the usage counts are data. <c>Down</c> is empty for the same reason:
    /// the tables still exist and the earlier migrations of this context describe them.
    /// </remarks>
    public partial class HandOverPlatformTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
