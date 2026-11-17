// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Tests.Architecture.Modules;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Architecture.Tenancy;

/// <summary>
/// TEN-02 (ARCHITECTURE §5.4, ADR-007; S5-15): <c>IgnoreQueryFilters()</c> bypasses the organisation
/// filter, and so does raw SQL (<c>FromSql…</c>, <c>SqlQuery…</c>, <c>ExecuteSql…</c>; since S5-16a,
/// the first module call), so they appear only at the call sites listed in <see cref="AllowList"/>,
/// each with its reason. The scan reads every production source file (the modules, the host and the legacy
/// projects); tests are not scanned, because they bypass the filter to set data up.
/// </summary>
/// <remarks>
/// A new call site fails this test until it is listed here with its reason, so a bypass is a
/// reviewed decision. A listed call site that is gone fails too, so the list stays honest.
/// </remarks>
public class IgnoreQueryFiltersAllowListTests
{
    /// <summary>
    /// The calls, not the names: the legacy repositories have methods named after them
    /// (<c>IgnoreQueryFilters</c>, <c>ExecuteSqlString</c>). Each prefix covers its variants
    /// (<c>FromSqlRaw</c>, <c>ExecuteSqlAsync</c> …).
    /// </summary>
    private static readonly string[] Tokens = [".IgnoreQueryFilters(", ".FromSql", ".SqlQuery", ".ExecuteSql"];

    private static readonly string[] ProductionFolders = ["src", "API", "DAL", "SERVICES"];

    /// <summary>Allowed call sites: repository-relative path → expected number of calls (all tokens) in that file.</summary>
    private static readonly IReadOnlyDictionary<string, int> AllowList = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        // Loads the signed-in user's own memberships, which define the filter, so they cannot be
        // read through it (ARCHITECTURE §5.4; one query per signed-in request).
        ["src/ProcureAccess.Modules.IdentityAccess/Tenancy/OrganizationContextMiddleware.cs"] = 1,
        // Raw SQL that reads nothing: the row lock of the organisation before a role change, a
        // removal (TEN-11, S5-16a), an invitation, a revocation or an acceptance (S5-17a), taken only after a
        // filtered read found the organisation or the link's token found the invitation.
        ["src/ProcureAccess.Modules.IdentityAccess/Organizations/OrganizationRowLock.cs"] = 1,
        // The invitation of a link and its organisation's name: the invitee is no member yet, so the
        // filter would hide it; the token, which only the invited mailbox received, is the credential
        // (S5-17a). One query, used by the page's read and by accepting.
        ["src/ProcureAccess.Modules.IdentityAccess/Organizations/OrganizationInvitationService.cs"] = 1,
        // The clean-up of expired invitations runs outside a request, where the filter shows nothing
        // (S5-17a, owner decision S5-17 D7 (a)).
        ["src/ProcureAccess.Modules.IdentityAccess/Organizations/InvitationCleanupService.cs"] = 1,
        // The E2E seeder runs outside a request, where the filter shows nothing: its check whether
        // the test organisation exists reads past it (Development and E2E only, S5-16b).
        ["src/ProcureAccess.Modules.IdentityAccess/Organizations/TestOrganizationSeeder.cs"] = 1,
        // The anonymous read of a share link (S5-23, ARCHITECTURE C59): the reader has no account and
        // so no organisation; the token, stored only as a hash, is the credential. Platform finds the
        // link by the token's hash, then the provider of the owning module reads the one list it names.
        ["src/ProcureAccess.Modules.Platform/Sharing/ShareLinkService.cs"] = 1,
        ["src/ProcureAccess.Modules.Configurator/Sharing/ConfigurationShareProvider.cs"] = 1,
        // Legacy: the generic repositories bypass the legacy context's own filters (soft delete)
        // and offer raw SQL (ExecuteSqlRaw, FromSqlRaw; two calls each with IgnoreQueryFilters).
        // UserRepo's call went with its unused RestoreUserAsync (S5-10), UserRepo itself with S5-28b.
        // The legacy context has no organisation-scoped entity (TEN-01 finds none), so these touch
        // no organisation filter; they go with the legacy repositories in their migration steps.
        ["DAL/DAL.ProcureAccess/Repos/Base/BaseRepo.cs"] = 2,
        ["DAL/DAL.ProcureAccess/Repos/Base/BaseViewRepo.cs"] = 2,
    };

    [Fact(DisplayName = "INV:TEN-02 " + nameof(IgnoreQueryFilters_ProductionSources_OnlyAtAllowListedCallSites))]
    [Trait("Invariant", "TEN-02")]
    public void IgnoreQueryFilters_ProductionSources_OnlyAtAllowListedCallSites()
    {
        var root = ProjectGraph.FindRepositoryRoot();

        var found = ProductionFolders
            .Select(folder => Path.Combine(root, folder))
            .Where(Directory.Exists)
            .SelectMany(folder => Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories))
            .Where(file => !IsBuildOutput(file))
            .Select(file => (Path: Path.GetRelativePath(root, file).Replace('\\', '/'), Count: CountOccurrences(file)))
            .Where(hit => hit.Count > 0)
            .ToDictionary(hit => hit.Path, hit => hit.Count, StringComparer.Ordinal);

        found.ShouldBe(AllowList, ignoreOrder: true,
            $"{string.Join(", ", Tokens)} bypass the organisation filter (TEN-02): list every call site with its reason in {nameof(AllowList)}.");
    }

    [Fact]
    public void AllowList_EveryEntry_NamesAnExistingFile()
    {
        var root = ProjectGraph.FindRepositoryRoot();

        foreach (var path in AllowList.Keys)
        {
            File.Exists(Path.Combine(root, path)).ShouldBeTrue($"{path} is allow-listed but does not exist.");
        }
    }

    private static int CountOccurrences(string file)
    {
        var count = 0;
        foreach (var line in File.ReadLines(file))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue; // a comment that names the method, e.g. this file's own documentation
            }

            foreach (var token in Tokens)
            {
                var index = 0;
                while ((index = line.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
                {
                    count++;
                    index += token.Length;
                }
            }
        }

        return count;
    }

    private static bool IsBuildOutput(string file)
    {
        var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        return file.Split(separators).Any(segment => segment is "bin" or "obj");
    }
}
