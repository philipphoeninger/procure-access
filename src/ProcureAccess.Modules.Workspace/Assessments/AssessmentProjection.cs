// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Workspace.Domain;

namespace ProcureAccess.Modules.Workspace.Assessments;

/// <summary>
/// The rules of an assessment that need no database (S6-18): which value a criterion allows
/// (CAT-15), the seven counts and the progress of a candidate (SIG-13, R-CAN-08) and the derived
/// answer of a procedure criterion (CAT-17). Pure.
/// </summary>
internal static class AssessmentProjection
{
    private static readonly (Fulfillment Value, Rating Rating, string Name)[] Levels =
    [
        (Fulfillment.Fulfilled, Rating.Fulfilled, "fulfilled"),
        (Fulfillment.SlightRestriction, Rating.SlightRestriction, "slightRestriction"),
        (Fulfillment.Restriction, Rating.Restriction, "restriction"),
        (Fulfillment.Barrier, Rating.Barrier, "barrier"),
        (Fulfillment.Blockade, Rating.Blockade, "blockade"),
        (Fulfillment.NotApplicable, Rating.NotApplicable, "notApplicable"),
    ];

    private const string UnknownName = "unknown";

    /// <summary>The names a body may use for a value, in the order of the scale (<c>assessment.value.*</c>).</summary>
    public static IReadOnlyList<string> ValueNames { get; } = [.. Levels.Select(l => l.Name), UnknownName];

    /// <summary>The value of <paramref name="name"/>, or <c>null</c> for an unknown name.</summary>
    public static Fulfillment? ValueOf(string name) =>
        name == UnknownName ? Fulfillment.Unknown : Levels.Where(l => l.Name == name).Select(l => (Fulfillment?)l.Value).SingleOrDefault();

    /// <summary>The wire name of <paramref name="value"/>.</summary>
    public static string NameOf(Fulfillment value) =>
        value == Fulfillment.Unknown ? UnknownName : Levels.Single(l => l.Value == value).Name;

    /// <summary>The allowed levels by name, in the order of the scale; never <c>unknown</c>.</summary>
    public static IReadOnlyList<string> NamesOf(Rating allowed) => [.. Levels.Where(l => allowed.HasFlag(l.Rating)).Select(l => l.Name)];

    /// <summary>
    /// Whether a criterion with the allowed ratings <paramref name="allowed"/> — the union over the
    /// procedures of the list's app types, all six without a procedure text (C31) — may get
    /// <paramref name="value"/>. „Nicht eingeschätzt“ is always allowed (CAT-15).
    /// </summary>
    public static bool IsAllowed(Fulfillment value, Rating allowed) =>
        value == Fulfillment.Unknown || allowed.HasFlag(Levels.Single(l => l.Value == value).Rating);

    /// <summary>
    /// Progress, counts and procedure answers of one candidate against the project's list. Only the
    /// included requirement criteria count; a value of an excluded criterion or of a criterion
    /// outside the list is ignored, a criterion without a value is <c>unknown</c>, so the seven
    /// counts add up to the total.
    /// </summary>
    /// <param name="values">The candidate's values by criterion revision.</param>
    /// <param name="evidenceTestTypeKeys">The catalog procedures the candidate has an evidence of.</param>
    public static CandidateAssessmentSummary SummaryOf(
        ProjectCriteriaScope scope,
        IReadOnlyDictionary<int, Fulfillment> values,
        TestStatus testStatus,
        IReadOnlyCollection<string> evidenceTestTypeKeys)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(values);
        var perValue = scope.Requirements
            .GroupBy(c => values.GetValueOrDefault(c.RevisionId, Fulfillment.Unknown))
            .ToDictionary(g => g.Key, g => g.Count());
        int Count(Fulfillment value) => perValue.GetValueOrDefault(value);

        var counts = new FulfillmentCountsResponse(
            Count(Fulfillment.Fulfilled),
            Count(Fulfillment.SlightRestriction),
            Count(Fulfillment.Restriction),
            Count(Fulfillment.Barrier),
            Count(Fulfillment.Blockade),
            Count(Fulfillment.NotApplicable),
            Count(Fulfillment.Unknown));
        var total = scope.Requirements.Count;
        return new CandidateAssessmentSummary(
            new AssessmentProgressResponse(total - counts.Unknown, total),
            counts,
            [.. scope.Procedures.Select(p => new ProcedureAnswerResponse(
                p.RevisionId, p.TestTypeKey!, p.Title, AnswerOf(testStatus, evidenceTestTypeKeys, p.TestTypeKey!)))]);
    }

    /// <summary>
    /// The derived answer of the procedure criterion of <paramref name="testTypeKey"/> (C30):
    /// <c>yes</c> with an evidence of that procedure, <c>no</c> when the product was not tested,
    /// otherwise <c>notStated</c> — also for a product tested by other procedures only.
    /// </summary>
    public static string AnswerOf(TestStatus testStatus, IReadOnlyCollection<string> evidenceTestTypeKeys, string testTypeKey)
    {
        ArgumentNullException.ThrowIfNull(evidenceTestTypeKeys);
        if (evidenceTestTypeKeys.Contains(testTypeKey))
        {
            return "yes";
        }

        return testStatus == TestStatus.NotTested ? "no" : "notStated";
    }
}

/// <summary>
/// The criteria of a project's list as the assessments need them: resolved under the release the
/// list is pinned to (C34), with the user's decision whether each is included.
/// </summary>
/// <param name="Requirements">The included requirement criteria, in list order: what is assessed and counted.</param>
/// <param name="Procedures">The included procedure criteria, in list order: derived, never assessed (CAT-17).</param>
/// <param name="RatingsFromSeveralProcedures">More than one procedure tests the list's app types (C31).</param>
internal sealed record ProjectCriteriaScope(
    string ReleaseVersion,
    IReadOnlyList<ResolvedCatalogCriterion> Requirements,
    IReadOnlyList<ResolvedCatalogCriterion> Procedures,
    bool RatingsFromSeveralProcedures);
