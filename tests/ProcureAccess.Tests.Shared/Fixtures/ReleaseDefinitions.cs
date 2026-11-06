// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Releases;

namespace ProcureAccess.Tests.Shared.Fixtures;

/// <summary>
/// A release as the <see cref="ReleaseDefinition"/> its folder gives, so tests can compare a
/// fixture, a folder and a stored release (S3-15, S3-16).
/// </summary>
internal static class ReleaseDefinitions
{
    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    /// <summary>
    /// The definition a folder of <paramref name="release"/> must give. A criterion of the release
    /// supersedes the criteria of <paramref name="previous"/> whose <c>SupersededByKey</c> names it.
    /// The release needs its revisions with everything they hold, and its rules with their values.
    /// </summary>
    public static ReleaseDefinition Of(CatalogRelease release, CatalogRelease? previous)
    {
        ArgumentNullException.ThrowIfNull(release);
        var predecessors = (previous?.Criteria ?? []).Select(c => c.Revision.Criterion)
            .Where(c => c.SupersededByKey is not null)
            .ToLookup(c => c.SupersededByKey!, c => c.Key);
        var criteria = release.Criteria.Select(c => c.Revision)
            .OrderBy(r => r.Criterion.Key, StringComparer.Ordinal)
            .Select(r => new CriterionDefinition(
                r.Criterion.Key,
                r.Criterion.Kind,
                r.Criterion.TestType?.Key,
                r.Criterion.Category,
                [.. predecessors[r.Criterion.Key].Order(StringComparer.Ordinal)],
                r.TitleDe,
                r.PlainLanguageDe,
                r.RationaleDe,
                r.SortOrder,
                [.. r.StandardReferences.Select(s => new StandardReferenceDefinition(s.Standard, s.Clause, s.Url))],
                [.. r.Applicability.Select(a => new ApplicabilityDefinition(a.AppType.Key, a.ProductPart?.Key))],
                [.. r.Priorities.Select(p => new PriorityDefinition(p.AppType?.Key, p.UseCase?.Key, p.Level, p.Weight, p.RationaleDe))],
                [.. r.ProcedureTexts.Select(t => new ProcedureTextDefinition(t.TestType.Key, new ProcedureTextContent(
                    t.OriginalNumber, t.OriginalTitleDe, t.AllowedRatings, t.SourceUrl, t.ApplicabilityDe, t.RequirementDe,
                    t.RationaleWhyDe, t.PositiveExamplesDe, t.NegativeExamplesDe)))]))
            .ToList();
        var exclusions = release.Exclusions.Select(e => e.Exclusion)
            .Select(e => new ExclusionDefinition(Reference(e.Source!), Reference(e.Target!), e.ReasonDe))
            .ToList();
        var procedures = release.Procedures.Select(p => new ProcedureMappingDefinition(p.AppType.Key, p.TestType.Key)).ToList();
        return new ReleaseDefinition(release.Version, release.ReleaseNotes, criteria, exclusions, procedures);

        static FilterValueReference Reference(FilterValue value) => new(value.Layer, value.Key);
    }

    /// <summary>The definition as indented JSON, in the order of its lists.</summary>
    public static string Json(ReleaseDefinition definition) => JsonSerializer.Serialize(definition, Indented);

    /// <summary>
    /// The definition as indented JSON with every list sorted: a release loaded from the database
    /// has no order in its collections, so it compares only this way.
    /// </summary>
    public static string SortedJson(ReleaseDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var sorted = definition with
        {
            Criteria = [.. definition.Criteria.Select(c => c with
            {
                StandardReferences = Sorted(c.StandardReferences),
                Applicability = Sorted(c.Applicability),
                Priorities = Sorted(c.Priorities),
                ProcedureTexts = Sorted(c.ProcedureTexts),
            })],
            Exclusions = Sorted(definition.Exclusions),
            Procedures = Sorted(definition.Procedures),
        };
        return Json(sorted);

        static List<T> Sorted<T>(IEnumerable<T> items) =>
            [.. items.OrderBy(item => JsonSerializer.Serialize(item), StringComparer.Ordinal)];
    }
}
