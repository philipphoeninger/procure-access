// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Configurator.Persistence;

namespace ProcureAccess.Modules.Configurator.Configurations;

/// <inheritdoc cref="IConfigurationReader"/>
internal sealed class ConfigurationReader(ConfiguratorDbContext db) : IConfigurationReader
{
    public async Task<SavedConfiguration?> FindAsync(Guid configurationId, CancellationToken cancellationToken)
    {
        // Through the organisation filter of the request (TEN-01).
        var list = await db.CriteriaConfigurations.AsNoTracking()
                           .SingleOrDefaultAsync(c => c.Id == configurationId, cancellationToken);
        return list is null
            ? null
            : new SavedConfiguration(
                list.Id,
                list.Name,
                list.ReleaseVersion,
                list.UseCaseKey,
                [.. list.AppTypes.Select(v => v.AppTypeKey)],
                [.. list.ProductParts.Select(v => v.ProductPartKey)],
                [.. list.TestTypes.Select(v => v.TestTypeKey)],
                [.. list.Criteria.Select(c => new SavedCriterion(
                    c.CriterionRevisionId, c.IsIncluded, c.ResolvedLevel, c.OverrideLevel, c.OverrideWeight, c.NoteDe))]);
    }
}
