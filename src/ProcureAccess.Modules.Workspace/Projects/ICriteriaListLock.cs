// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Projects;

/// <summary>
/// Whether a project's criteria list may still be replaced (SPEC R-PRJ-04; ARCHITECTURE C47, C51,
/// C53; WSP-02): it is locked once any candidate of the project has an assessment with a value
/// other than „Nicht eingeschätzt“ (<c>Unknown</c>). Rows with <c>Unknown</c> do not lock it, and
/// adjusting the copy within its release never is locked.
/// </summary>
/// <remarks>
/// <b>Hook for S6-18.</b> Assessments do not exist before S6-18, so <see cref="NoAssessmentsYet"/>
/// answers „not locked“ for every project. S6-18 replaces that registration in
/// <c>WorkspaceModule</c> by the query on its assessment rows; the endpoint's 409
/// <c>locked-criteria-list</c> is already in place and tested with a test double.
/// </remarks>
internal interface ICriteriaListLock
{
    /// <summary>
    /// <c>true</c> when a candidate of <paramref name="projectId"/> has an assessment other than
    /// <c>Unknown</c>, so the project's list can no longer be replaced.
    /// </summary>
    Task<bool> IsLockedAsync(Guid projectId, CancellationToken cancellationToken);
}

/// <summary>
/// The lock until S6-18 stores assessments: no project has one, so no list is locked.
/// </summary>
internal sealed class NoAssessmentsYet : ICriteriaListLock
{
    public Task<bool> IsLockedAsync(Guid projectId, CancellationToken cancellationToken) => Task.FromResult(false);
}
