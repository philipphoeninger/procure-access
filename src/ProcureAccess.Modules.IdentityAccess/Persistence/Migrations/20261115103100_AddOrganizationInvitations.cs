using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Migrations
{
    /// <summary>
    /// The invitations by e-mail (S5-17a, ARCHITECTURE §5.2, §9.1; SPEC R-ORG-02): only the token's
    /// SHA-256 is stored (SEC-11), one open invitation per organisation and address, deleted with the
    /// organisation. The organisation filter is bound in the context, not in the schema.
    /// </summary>
    internal partial class AddOrganizationInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationInvitations",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TokenHash = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationInvitations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "identity",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_ExpiresAt",
                schema: "identity",
                table: "OrganizationInvitations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_OrganizationId_NormalizedEmail",
                schema: "identity",
                table: "OrganizationInvitations",
                columns: new[] { "OrganizationId", "NormalizedEmail" },
                unique: true,
                filter: "\"AcceptedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_TokenHash",
                schema: "identity",
                table: "OrganizationInvitations",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationInvitations",
                schema: "identity");
        }
    }
}
