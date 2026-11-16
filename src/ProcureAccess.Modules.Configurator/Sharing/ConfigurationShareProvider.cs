// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Sharing;

namespace ProcureAccess.Modules.Configurator.Sharing;

/// <summary>
/// The saved lists' side of a share link (ARCHITECTURE §5.3, C59; SPEC R-SAV-05, §8.5; S5-23):
/// Platform found an active link by its token and asks for the list it opens, redacted
/// (<see cref="SharedConfigurationResponse"/>, TEN-08).
/// </summary>
internal sealed class ConfigurationShareProvider(ConfiguratorDbContext db) : IShareableResourceProvider
{
    public ShareableResourceType ResourceType => ShareableResourceType.Configuration;

    public async Task<object?> ReadSharedAsync(Guid resourceId, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        // TEN-02 allow-list: the reader has no account and so no organisation; Platform checked
        // the link, which names exactly this list.
        var list = await db.CriteriaConfigurations.IgnoreQueryFilters().AsNoTracking()
                           .SingleOrDefaultAsync(c => c.Id == resourceId, cancellationToken);
        return list is null
            ? null
            : new SharedConfigurationResponse(
                list.Name,
                list.ReleaseVersion,
                expiresAt,
                ConfigurationService.SelectionOf(list),
                ConfigurationService.DecisionsOf(list));
    }
}
