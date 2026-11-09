// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Criteria;

// The answer of GET /api/v1/catalog/criteria in the shape of SPEC §8.2 (C77). Enum values are
// strings already, so the JSON does not depend on a serializer setting: camelCase names, except
// the priority level, which is sent as "A", "B", "Info" like the SPEC example and the UX keys
// criteria.priority.A / .B / .Info.

internal sealed record CriteriaResponse(
    string Release,
    IReadOnlyList<CriterionItem> Criteria,
    IReadOnlyList<AppTypeItem> AppTypesWithoutCriteria);

internal sealed record CriterionItem(
    int RevisionId,
    string Key,
    string Kind,
    string? Number,
    string Title,
    string? PlainLanguage,
    string? ProductPart,
    IReadOnlyList<StandardReferenceItem> StandardReferences,
    IReadOnlyList<string> AllowedRatings,
    IReadOnlyList<ProcedureTextItem> ProcedureTexts,
    ResolvedPriorityItem ResolvedPriority);

internal sealed record StandardReferenceItem(string Standard, string Clause, string? Url);

internal sealed record ProcedureTextItem(
    string TestType,
    string OriginalNumber,
    string OriginalTitle,
    string? Applicability,
    string? Requirement,
    string? RationaleWhy,
    IReadOnlyList<string> AllowedRatings,
    string? PositiveExamples,
    string? NegativeExamples,
    string SourceUrl);

internal sealed record ResolvedPriorityItem(string Level, int? Weight, string Rationale, string Source, MatchedRuleItem MatchedRule);

internal sealed record MatchedRuleItem(string Kind, string? AppType, string? UseCase);

internal sealed record AppTypeItem(string Id, string Name);
