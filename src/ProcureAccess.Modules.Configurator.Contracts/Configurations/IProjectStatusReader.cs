// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Configurator.Contracts.Configurations;

/// <summary>
/// What Configurator must know of the procurement project that owns a copy of a list
/// (ARCHITECTURE O-30, owner decision O-S7-4; S7-08): an archived project is read-only (SPEC
/// R-PRJ-05), and so is its copy. Declared here and implemented by Workspace, which owns the
/// projects, so Configurator does not depend on Workspace (ARCHITECTURE §4.3: Workspace may use
/// these contracts, not the other way).
/// </summary>
public interface IProjectStatusReader
{
    /// <summary>
    /// <c>true</c> when the project <paramref name="projectId"/> is archived. Read through the
    /// organisation filter of the request (TEN-01), as the copy that names the project was;
    /// <c>false</c> for a project the request does not see.
    /// </summary>
    Task<bool> IsArchivedAsync(Guid projectId, CancellationToken cancellationToken);
}
