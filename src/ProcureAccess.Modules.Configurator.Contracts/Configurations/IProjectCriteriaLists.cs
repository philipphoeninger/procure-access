// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Configurator.Contracts.Configurations;

/// <summary>
/// The criteria list a procurement project owns (ARCHITECTURE §5.4, C53; SPEC R-PRJ-02, R-PRJ-04;
/// S6-12): Workspace assigns or replaces it by copying an organisation list, and reads which list
/// the project has. Declared here and implemented by Configurator, so Configurator does not depend
/// on Workspace. The link is stored only on the copy (owner decision S6-12 D1 (a)).
/// </summary>
/// <remarks>
/// Every read runs through the organisation filter of the request (TEN-01). Whether the project
/// may get a new list — its organisation, the role, the archive and the lock once assessments
/// exist (C47) — is the caller's check before it calls <see cref="CopyIntoProjectAsync"/>.
/// </remarks>
public interface IProjectCriteriaLists
{
    /// <summary>
    /// Copies the organisation list <paramref name="sourceConfigurationId"/> into
    /// <paramref name="projectId"/> and deletes the project's previous copy in the same save, so a
    /// project never has two. The source stays unchanged. The source must be a list of
    /// <paramref name="organizationId"/> that the request can see and not a project copy itself.
    /// </summary>
    Task<ProjectCopyResult> CopyIntoProjectAsync(
        Guid sourceConfigurationId,
        Guid projectId,
        Guid organizationId,
        string userId,
        CancellationToken cancellationToken);

    /// <summary>The project's copy, or <c>null</c> while it has none.</summary>
    Task<ProjectCriteriaList?> FindOfProjectAsync(Guid projectId, CancellationToken cancellationToken);
}

/// <summary>How an assignment of a list to a project ended.</summary>
public enum ProjectCopyOutcome
{
    /// <summary>The copy is stored; <see cref="ProjectCopyResult.List"/> names it.</summary>
    Copied = 1,

    /// <summary>The source does not exist for the request or belongs to another organisation.</summary>
    SourceNotFound = 2,

    /// <summary>The source is a project copy, which is never offered as a source (WSP-02).</summary>
    SourceIsProjectCopy = 3,
}

/// <param name="List">The new copy; only with <see cref="ProjectCopyOutcome.Copied"/>.</param>
public sealed record ProjectCopyResult(ProjectCopyOutcome Outcome, ProjectCriteriaList? List);

/// <summary>
/// A project's copy of a list: its id (S11 opens it through the saved-list endpoints, C51), its
/// name — the name of the list it was copied from (<c>projects.detail.criteria.copyNote</c>) — and
/// the catalog release it is pinned to.
/// </summary>
public sealed record ProjectCriteriaList(Guid Id, string Name, string ReleaseVersion);
