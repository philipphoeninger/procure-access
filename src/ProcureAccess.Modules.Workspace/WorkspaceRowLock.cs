// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace;

/// <summary>
/// The row lock of a procurement project (ARCHITECTURE O-31, owner decision O-S7-4; S7-08): the
/// assignment of a criteria list and the save of an assessment take it inside their transaction, so
/// the check whether the list is locked, the copy Configurator stores and the project's new version
/// are one step for every other change of the project (WSP-02, API-03). A rename or a status change
/// takes no lock of its own: its <c>UPDATE</c> waits for this one and then meets the new version.
/// </summary>
/// <remarks>
/// <para><b>Order.</b> A candidate's row before its project's row, the project's row before
/// assessment rows, and the workspace's rows before Configurator's: the saves of a candidate and of
/// an evidence write the candidate and then stamp the project in one batch, so a path that took the
/// project first could wait for them in a circle. One circle is left — the removal of a candidate
/// deletes its assessment rows before it stamps the project, while an assignment holds the project
/// and deletes the notes without a value —; PostgreSQL ends it, and the context's execution
/// strategy runs the ended change again.</para>
/// <para><b>Raw SQL that reads nothing</b>, so it passes the organisation filter: the callers take
/// a lock only after a filtered read found the row (TEN-02 allow-list).</para>
/// </remarks>
internal static class WorkspaceRowLock
{
    /// <summary>
    /// Runs <paramref name="change"/> in a transaction of its own and commits it. Inside the
    /// context's execution strategy, which retries on transient failures: a retry starts again from
    /// the database, so <paramref name="change"/> loads what it changes itself.
    /// </summary>
    public static async Task<T> InTransactionAsync<T>(
        WorkspaceDbContext db, Func<CancellationToken, Task<T>> change, CancellationToken cancellationToken)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            async ct =>
            {
                db.ChangeTracker.Clear();
                await using var transaction = await db.Database.BeginTransactionAsync(ct);
                var result = await change(ct);
                await transaction.CommitAsync(ct);
                return result;
            },
            cancellationToken);
    }

    /// <summary>Locks the row of the project <paramref name="projectId"/> until the current transaction ends.</summary>
    public static async Task LockProjectAsync(WorkspaceDbContext db, Guid projectId, CancellationToken cancellationToken)
    {
        RequireTransaction(db);
        await db.Database.ExecuteSqlAsync(
            $"""SELECT 1 FROM workspace."ProcurementProjects" WHERE "Id" = {projectId} FOR UPDATE""", cancellationToken);
    }

    /// <summary>
    /// Locks the row of the candidate <paramref name="candidateId"/> until the current transaction
    /// ends; before <see cref="LockProjectAsync"/>, never after it.
    /// </summary>
    public static async Task LockCandidateAsync(WorkspaceDbContext db, Guid candidateId, CancellationToken cancellationToken)
    {
        RequireTransaction(db);
        await db.Database.ExecuteSqlAsync(
            $"""SELECT 1 FROM workspace."CandidateProducts" WHERE "Id" = {candidateId} FOR UPDATE""", cancellationToken);
    }

    private static void RequireTransaction(WorkspaceDbContext db)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A row lock of the workspace needs a transaction.");
        }
    }
}
