// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace;

/// <summary>
/// Who changes a shared resource and which version they have seen (API-03; S6-20).
/// </summary>
/// <param name="UserId">The signed-in user, stored as the editor of the new version.</param>
/// <param name="IfMatch">The <c>If-Match</c> header; the endpoint has refused a missing one (428).</param>
internal sealed record EditPrecondition(string UserId, string IfMatch);

/// <summary>
/// Runs a change of the workspace again when its save met a concurrent one (API-03; S6-20).
/// </summary>
/// <remarks>
/// <para>The <c>Version</c> of a project, a candidate and an evidence is the concurrency token of
/// its row, so a save fails with <see cref="DbUpdateConcurrencyException"/> when the row changed
/// or went since it was loaded. Two cases look the same to the save: the resource the request
/// changes got a new version — its <c>If-Match</c> is stale now —, or the save only stamped the
/// last change on a row (a parent, or the candidate of an assessment) that another request
/// touched. Running the change again tells them apart: the fresh load refuses the stale
/// <c>If-Match</c> with a conflict, and a stamp simply lands on the row as it is now.</para>
/// <para>After <see cref="Attempts"/> saves that each met a new concurrent change, the exception
/// leaves (500); that needs as many other changes of the same rows within milliseconds.</para>
/// </remarks>
internal static class ConcurrentChange
{
    public const int Attempts = 3;

    /// <param name="change">Loads what it changes itself: the tracked rows of a failed attempt are dropped.</param>
    public static async Task<T> RunAsync<T>(WorkspaceDbContext db, Func<Task<T>> change)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await change();
            }
            catch (DbUpdateConcurrencyException) when (attempt < Attempts)
            {
                db.ChangeTracker.Clear();
            }
        }
    }
}
