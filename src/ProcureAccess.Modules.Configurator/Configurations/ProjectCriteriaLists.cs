// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Configurator.Persistence;

namespace ProcureAccess.Modules.Configurator.Configurations;

/// <inheritdoc cref="IProjectCriteriaLists"/>
internal sealed class ProjectCriteriaLists(ConfiguratorDbContext db, TimeProvider clock) : IProjectCriteriaLists
{
    public async Task<ProjectCopyResult> CopyIntoProjectAsync(
        Guid sourceConfigurationId,
        Guid projectId,
        Guid organizationId,
        string userId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        try
        {
            return await ReplaceAsync(sourceConfigurationId, projectId, organizationId, userId, cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception is DbUpdateConcurrencyException
            || exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Two first assignments at once: the one that loses the insert replaces the winner's
            // copy, as a later assignment would. Or the copy to replace got a new version (a
            // decision on S11) or went since it was loaded: the replacement does not depend on the
            // copy's version — the project's If-Match guards the assignment (API-03, S6-20).
            db.ChangeTracker.Clear();
            return await ReplaceAsync(sourceConfigurationId, projectId, organizationId, userId, cancellationToken);
        }
    }

    public async Task<ProjectCriteriaList?> FindOfProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
        await db.CriteriaConfigurations.AsNoTracking()
                .Where(c => c.ProcurementProjectId == projectId)
                .Select(c => new ProjectCriteriaList(c.Id, c.Name, c.ReleaseVersion))
                .SingleOrDefaultAsync(cancellationToken);

    private async Task<ProjectCopyResult> ReplaceAsync(
        Guid sourceConfigurationId,
        Guid projectId,
        Guid organizationId,
        string userId,
        CancellationToken cancellationToken)
    {
        // Through the organisation filter (TEN-01); a member of two organisations sees both, so the
        // source's organisation is compared with the project's as well.
        var source = await db.CriteriaConfigurations.AsNoTracking()
                             .SingleOrDefaultAsync(c => c.Id == sourceConfigurationId, cancellationToken);
        if (source is null || source.OrganizationId != organizationId)
        {
            return new ProjectCopyResult(ProjectCopyOutcome.SourceNotFound, null);
        }

        if (source.IsProjectCopy)
        {
            return new ProjectCopyResult(ProjectCopyOutcome.SourceIsProjectCopy, null);
        }

        var previous = await db.CriteriaConfigurations
                               .Where(c => c.ProcurementProjectId == projectId)
                               .ToListAsync(cancellationToken);
        db.CriteriaConfigurations.RemoveRange(previous);

        var copy = source.CopyForProject(projectId, userId, clock.GetUtcNow());
        db.CriteriaConfigurations.Add(copy);

        // One save: the old copy goes and the new one comes together, or neither (R-PRJ-04).
        await db.SaveChangesAsync(cancellationToken);
        return new ProjectCopyResult(ProjectCopyOutcome.Copied, new ProjectCriteriaList(copy.Id, copy.Name, copy.ReleaseVersion));
    }
}
