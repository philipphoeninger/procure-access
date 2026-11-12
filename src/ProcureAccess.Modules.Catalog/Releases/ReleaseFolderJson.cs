// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>
/// The JSON shape of the files of a release folder, as written by hand. Every property is nullable,
/// so a missing value becomes a validation error with its path instead of stopping at the first one;
/// an unknown property (a typo) is refused while reading. The validator turns these into
/// <see cref="ReleaseDefinition"/>.
/// </summary>
internal static class ReleaseFolderJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        AllowDuplicateProperties = false,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false,
    };
}

/// <summary><c>release.json</c>.</summary>
internal sealed class ReleaseJson
{
    public string? Version { get; set; }

    public string? ReleaseNotes { get; set; }
}

/// <summary><c>exclusions.json</c>.</summary>
internal sealed class ExclusionsJson
{
    public List<ExclusionJson?>? Exclusions { get; set; }
}

internal sealed class ExclusionJson
{
    public FilterValueReferenceJson? Source { get; set; }

    public FilterValueReferenceJson? Target { get; set; }

    public string? ReasonDe { get; set; }
}

internal sealed class FilterValueReferenceJson
{
    /// <summary><c>useCase</c>, <c>appType</c>, <c>productPart</c> or <c>testType</c>.</summary>
    public string? Layer { get; set; }

    public string? Key { get; set; }
}

/// <summary><c>procedure-mapping.json</c>.</summary>
internal sealed class ProcedureMappingJson
{
    public List<ProcedureMappingEntryJson?>? Procedures { get; set; }
}

internal sealed class ProcedureMappingEntryJson
{
    public string? AppType { get; set; }

    public List<string?>? TestTypes { get; set; }
}

/// <summary><c>criteria/&lt;key&gt;.json</c>; the key is the file name.</summary>
internal sealed class CriterionJson
{
    /// <summary><c>requirement</c> or <c>testProcedure</c>.</summary>
    public string? Kind { get; set; }

    public string? TestType { get; set; }

    public string? Category { get; set; }

    public List<string?>? Supersedes { get; set; }

    public string? TitleDe { get; set; }

    public string? PlainLanguageDe { get; set; }

    /// <summary>One or two plain sentences, no Markdown (Kurzbeschreibung, ADR-016).</summary>
    public string? ShortDescriptionDe { get; set; }

    /// <summary>Lines of Markdown, joined with line breaks.</summary>
    public List<string?>? RationaleDe { get; set; }

    public int? SortOrder { get; set; }

    public List<StandardReferenceJson?>? StandardReferences { get; set; }

    public List<ApplicabilityJson?>? Applicability { get; set; }

    public List<PriorityJson?>? Priorities { get; set; }

    public List<ProcedureTextJson?>? ProcedureTexts { get; set; }
}

internal sealed class StandardReferenceJson
{
    /// <summary><c>en301549</c>, <c>wcag</c>, <c>bitInklusiv</c> or <c>iso9241</c>.</summary>
    public string? Standard { get; set; }

    public string? Clause { get; set; }

    public string? Url { get; set; }
}

/// <summary>The criterion applies to every listed app type, for each listed product part (or for any part).</summary>
internal sealed class ApplicabilityJson
{
    public List<string?>? AppTypes { get; set; }

    public List<string?>? ProductParts { get; set; }
}

internal sealed class PriorityJson
{
    public string? AppType { get; set; }

    public string? UseCase { get; set; }

    /// <summary><c>A</c>, <c>B</c> or <c>Info</c>.</summary>
    public string? Level { get; set; }

    public int? Weight { get; set; }

    public string? RationaleDe { get; set; }
}

internal sealed class ProcedureTextJson
{
    public string? TestType { get; set; }

    public string? OriginalNumber { get; set; }

    public string? OriginalTitleDe { get; set; }

    public List<string?>? ApplicabilityDe { get; set; }

    public List<string?>? RequirementDe { get; set; }

    public List<string?>? RationaleWhyDe { get; set; }

    /// <summary>
    /// <c>fulfilled</c>, <c>slightRestriction</c>, <c>restriction</c>, <c>barrier</c>,
    /// <c>blockade</c>, <c>notApplicable</c>.
    /// </summary>
    public List<string?>? AllowedRatings { get; set; }

    public List<string?>? PositiveExamplesDe { get; set; }

    public List<string?>? NegativeExamplesDe { get; set; }

    public string? SourceUrl { get; set; }
}
