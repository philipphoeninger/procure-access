// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// The verbatim BIT inklusiv text of a criterion for one procedure (ARCHITECTURE §5.1, CAT-16):
/// original number and title, applicability, requirement, rationale, allowed ratings and examples,
/// with a link to the procedure step. A revision has at most one per test type. Testing procedure,
/// delimitation and references are linked through <see cref="SourceUrl"/>, not copied. Created only
/// by <see cref="CriterionRevision.AddProcedureText"/>.
/// </summary>
internal sealed class CriterionProcedureText : CatalogEntity
{
    public const int OriginalNumberMaxLength = 32;
    public const int OriginalTitleMaxLength = 500;
    public const int SourceUrlMaxLength = 2048;

    private CriterionProcedureText()
    {
    }

    internal CriterionProcedureText(CriterionRevision revision, TestType testType, ProcedureTextContent content)
    {
        Revision = revision;
        TestType = testType;
        OriginalNumber = content.OriginalNumber;
        OriginalTitleDe = content.OriginalTitleDe;
        ApplicabilityDe = content.ApplicabilityDe;
        RequirementDe = content.RequirementDe;
        RationaleWhyDe = content.RationaleWhyDe;
        AllowedRatings = content.AllowedRatings;
        PositiveExamplesDe = content.PositiveExamplesDe;
        NegativeExamplesDe = content.NegativeExamplesDe;
        SourceUrl = content.SourceUrl;
    }

    public int CriterionRevisionId { get; private set; }

    public CriterionRevision Revision { get; private set; } = null!;

    public int TestTypeId { get; private set; }

    public TestType TestType { get; private set; } = null!;

    /// <summary>Number of the step in the procedure, e.g. <c>11.2.1.1</c>.</summary>
    public string OriginalNumber { get; private set; } = null!;

    public string OriginalTitleDe { get; private set; } = null!;

    public string? ApplicabilityDe { get; private set; }

    public string? RequirementDe { get; private set; }

    public string? RationaleWhyDe { get; private set; }

    /// <summary>The levels this procedure allows (never empty; „Nicht eingeschätzt“ is always allowed).</summary>
    public Rating AllowedRatings { get; private set; }

    public string? PositiveExamplesDe { get; private set; }

    public string? NegativeExamplesDe { get; private set; }

    /// <summary>Link to the step in the procedure (with anchor).</summary>
    public string SourceUrl { get; private set; } = null!;
}

/// <summary>Content of a <see cref="CriterionProcedureText"/>, as the release folder holds it (S3-15).</summary>
internal sealed record ProcedureTextContent(
    string OriginalNumber,
    string OriginalTitleDe,
    Rating AllowedRatings,
    string SourceUrl,
    string? ApplicabilityDe = null,
    string? RequirementDe = null,
    string? RationaleWhyDe = null,
    string? PositiveExamplesDe = null,
    string? NegativeExamplesDe = null);
