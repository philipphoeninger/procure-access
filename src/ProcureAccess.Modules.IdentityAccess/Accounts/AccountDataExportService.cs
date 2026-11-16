// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <inheritdoc cref="IAccountDataExport"/>
internal sealed class AccountDataExportService(IdentityAccessDbContext db) : IAccountDataExport
{
    /// <summary>German collation for organisation names, as the header lists them.</summary>
    private static readonly StringComparer NameOrder = StringComparer.Create(CultureInfo.GetCultureInfo("de-DE"), ignoreCase: true);

    public async Task<AccountDataPart> GetAsync(string userId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // Through the organisation filter of the request (TEN-01): the signed-in user's own
        // memberships are exactly the ones the filter lets through.
        var memberships = await (from m in db.OrganizationMemberships.AsNoTracking()
                                 join o in db.Organizations.AsNoTracking() on m.OrganizationId equals o.Id
                                 where m.UserId == userId
                                 select new { o.Name, m.Role, Since = m.AcceptedAt ?? m.InvitedAt })
                                .ToListAsync(cancellationToken);

        var profiles = await (from l in db.UserContributorLinks.AsNoTracking()
                              join p in db.ContributorProfiles.AsNoTracking() on l.ContributorProfileId equals p.Id
                              where l.UserId == userId
                              select new { p.DisplayName, p.Kind, p.CreatedAt })
                             .ToListAsync(cancellationToken);

        return new AccountDataPart(
            [.. memberships.OrderBy(m => m.Name, NameOrder).ThenBy(m => m.Since)
                           .Select(m => new ExportedMembership(m.Name, m.Role, m.Since))],
            [.. profiles.OrderBy(p => p.CreatedAt).ThenBy(p => p.DisplayName, StringComparer.Ordinal)
                        .Select(p => new ExportedContributorProfile(p.DisplayName, KindName(p.Kind), p.CreatedAt))]);
    }

    private static string KindName(ContributorKind kind) => kind switch
    {
        ContributorKind.Individual => "individual",
        ContributorKind.Institution => "institution",
        ContributorKind.Vendor => "vendor",
        ContributorKind.Auditor => "auditor",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown contributor kind."),
    };
}
