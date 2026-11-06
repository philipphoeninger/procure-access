// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>
/// Decides whether a criterion of a release folder is unchanged against a stored revision, so the
/// new release shares that revision (CAT-07). The lists of a revision are sets: their order in the
/// file is not content.
/// </summary>
internal static class RevisionContent
{
    /// <summary>
    /// True when <paramref name="revision"/> (loaded with its references, applicability, priorities
    /// and procedure texts) holds exactly what <paramref name="definition"/> holds.
    /// </summary>
    public static bool Matches(CriterionDefinition definition, CriterionRevision revision)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(revision);
        return revision.Criterion.Key == definition.Key
            && revision.TitleDe == definition.TitleDe
            && revision.PlainLanguageDe == definition.PlainLanguageDe
            && revision.RationaleDe == definition.RationaleDe
            && revision.SortOrder == definition.SortOrder
            && SameSet(
                definition.StandardReferences,
                revision.StandardReferences.Select(s => new StandardReferenceDefinition(s.Standard, s.Clause, s.Url)))
            && SameSet(
                definition.Applicability,
                revision.Applicability.Select(a => new ApplicabilityDefinition(a.AppType.Key, a.ProductPart?.Key)))
            && SameSet(
                definition.Priorities,
                revision.Priorities
                    // The folder has no source; the import always writes the only one (ExpertJudgement).
                    .Select(p => p.Source == PrioritySource.ExpertJudgement
                        ? new PriorityDefinition(p.AppType?.Key, p.UseCase?.Key, p.Level, p.Weight, p.RationaleDe)
                        : null))
            && SameSet(
                definition.ProcedureTexts,
                revision.ProcedureTexts.Select(t => new ProcedureTextDefinition(t.TestType.Key, new ProcedureTextContent(
                    t.OriginalNumber,
                    t.OriginalTitleDe,
                    t.AllowedRatings,
                    t.SourceUrl,
                    t.ApplicabilityDe,
                    t.RequirementDe,
                    t.RationaleWhyDe,
                    t.PositiveExamplesDe,
                    t.NegativeExamplesDe))));
    }

    /// <summary>Same elements; both sides hold no duplicates (the validation and the domain refuse them).</summary>
    private static bool SameSet<T>(IReadOnlyCollection<T> expected, IEnumerable<T?> actual)
        where T : class
    {
        var stored = actual.ToList();
        return stored.Count == expected.Count && stored.All(s => s is not null && expected.Contains(s));
    }
}
