// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

/// <summary>
/// The seven ASP.NET Core Identity tables as the legacy context created them (S6-10, ADR-014
/// rule 4): their names without the <c>AspNet</c> prefix, and every text column Identity leaves
/// without a length at 200 characters, which the legacy context's string convention had set. The
/// tables hold accounts, so the layout must stay exactly this one;
/// <c>IdentityTablesHandOverTests</c> compares it with what the legacy migrations created.
/// </summary>
internal static class IdentityTablesConfiguration
{
    /// <summary>Length of the text columns without a length of their own (ids, stamps, hashes, claim and token values).</summary>
    public const int TextLength = 200;

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("Users");
            user.Property(x => x.Id).HasMaxLength(TextLength);
            user.Property(x => x.ConcurrencyStamp).HasMaxLength(TextLength);
            user.Property(x => x.PasswordHash).HasMaxLength(TextLength);
            user.Property(x => x.PhoneNumber).HasMaxLength(TextLength);
            user.Property(x => x.SecurityStamp).HasMaxLength(TextLength);

            // A deleted account does not exist for UserManager and every query (§9.3, S5-28a).
            user.HasQueryFilter(x => !x.IsDeleted);
        });
        modelBuilder.Entity<IdentityRole>(role =>
        {
            role.ToTable("Roles");
            role.Property(x => x.Id).HasMaxLength(TextLength);
            role.Property(x => x.ConcurrencyStamp).HasMaxLength(TextLength);
        });
        modelBuilder.Entity<IdentityRoleClaim<string>>(claim =>
        {
            claim.ToTable("RoleClaims");
            claim.Property(x => x.RoleId).HasMaxLength(TextLength);
            claim.Property(x => x.ClaimType).HasMaxLength(TextLength);
            claim.Property(x => x.ClaimValue).HasMaxLength(TextLength);
        });
        modelBuilder.Entity<IdentityUserClaim<string>>(claim =>
        {
            claim.ToTable("UserClaims");
            claim.Property(x => x.UserId).HasMaxLength(TextLength);
            claim.Property(x => x.ClaimType).HasMaxLength(TextLength);
            claim.Property(x => x.ClaimValue).HasMaxLength(TextLength);
        });
        modelBuilder.Entity<IdentityUserLogin<string>>(login =>
        {
            login.ToTable("UserLogins");
            login.Property(x => x.LoginProvider).HasMaxLength(TextLength);
            login.Property(x => x.ProviderKey).HasMaxLength(TextLength);
            login.Property(x => x.ProviderDisplayName).HasMaxLength(TextLength);
            login.Property(x => x.UserId).HasMaxLength(TextLength);
        });
        modelBuilder.Entity<IdentityUserRole<string>>(userRole =>
        {
            userRole.ToTable("UserRoles");
            userRole.Property(x => x.UserId).HasMaxLength(TextLength);
            userRole.Property(x => x.RoleId).HasMaxLength(TextLength);
        });
        modelBuilder.Entity<IdentityUserToken<string>>(token =>
        {
            token.ToTable("UserTokens");
            token.Property(x => x.UserId).HasMaxLength(TextLength);
            token.Property(x => x.LoginProvider).HasMaxLength(TextLength);
            token.Property(x => x.Name).HasMaxLength(TextLength);
            token.Property(x => x.Value).HasMaxLength(TextLength);
        });
    }
}
