// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Workspace.Domain;

namespace ProcureAccess.Modules.Workspace.Candidates;

/// <summary>
/// The wire names of the test status (ARCHITECTURE §7.2: <c>tested | notTested | unknown</c>) and of
/// the test kind (<c>procedure | other | own</c>), in both directions.
/// </summary>
internal static class CandidateNames
{
    private static readonly IReadOnlyDictionary<string, TestStatus> StatusByName =
        new Dictionary<string, TestStatus>(StringComparer.Ordinal)
        {
            ["tested"] = TestStatus.Tested,
            ["notTested"] = TestStatus.NotTested,
            ["unknown"] = TestStatus.Unknown,
        };

    private static readonly IReadOnlyDictionary<string, TestKind> KindByName =
        new Dictionary<string, TestKind>(StringComparer.Ordinal)
        {
            ["procedure"] = TestKind.Procedure,
            ["other"] = TestKind.Other,
            ["own"] = TestKind.Own,
        };

    /// <summary>The names a body may use for the test status, in the order of S34a.</summary>
    public static IReadOnlyCollection<string> StatusNames => [.. StatusByName.Keys];

    /// <summary>The names a body may use for the test kind.</summary>
    public static IReadOnlyCollection<string> KindNames => [.. KindByName.Keys];

    public static TestStatus? StatusOf(string name) => StatusByName.TryGetValue(name, out var status) ? status : null;

    public static TestKind? KindOf(string name) => KindByName.TryGetValue(name, out var kind) ? kind : null;

    public static string Of(TestStatus status) => StatusByName.Single(pair => pair.Value == status).Key;

    public static string Of(TestKind kind) => KindByName.Single(pair => pair.Value == kind).Key;
}
