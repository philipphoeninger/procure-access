// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// An exclusion rule: a value of a higher layer makes a value of a lower layer unavailable
/// (ARCHITECTURE §5.1, Kriterienkatalog §3.1), e.g. HR-Anwendung → Dokument. A release names the
/// rules it applies through <see cref="CatalogRelease.AddExclusion"/> (CAT-19).
/// </summary>
/// <remarks>
/// <para><b>CAT-13.</b> <see cref="Create"/> refuses a rule whose target layer is not lower than its
/// source layer; the table has a CHECK constraint with the same rule, plus exactly one source and
/// exactly one target (<c>FilterExclusionConfiguration</c>).</para>
/// <para>The typed foreign keys follow ARCHITECTURE v7 (Y5): sources are use cases, app types or
/// product parts; targets are app types, product parts or test types. A rule has no method that
/// changes it, like <see cref="StandardReference"/>: a rule of a published release is part of it,
/// and a changed reason is a new rule.</para>
/// </remarks>
internal sealed class FilterExclusion : CatalogEntity
{
    public const int ReasonMaxLength = 500;

    private FilterExclusion()
    {
    }

    public int? SourceUseCaseId { get; private set; }

    public UseCaseTemplate? SourceUseCase { get; private set; }

    public int? SourceAppTypeId { get; private set; }

    public AppType? SourceAppType { get; private set; }

    public int? SourceProductPartId { get; private set; }

    public ProductPart? SourceProductPart { get; private set; }

    public int? TargetAppTypeId { get; private set; }

    public AppType? TargetAppType { get; private set; }

    public int? TargetProductPartId { get; private set; }

    public ProductPart? TargetProductPart { get; private set; }

    public int? TargetTestTypeId { get; private set; }

    public TestType? TargetTestType { get; private set; }

    /// <summary>Catalog documentation only; the UI builds its reason from UX copy (SPEC §8.1).</summary>
    public string ReasonDe { get; private set; } = null!;

    /// <summary>The value that excludes; null only for a rule loaded without its navigations.</summary>
    public FilterValue? Source => (FilterValue?)SourceUseCase ?? (FilterValue?)SourceAppType ?? SourceProductPart;

    /// <summary>The excluded value; null only for a rule loaded without its navigations.</summary>
    public FilterValue? Target => (FilterValue?)TargetAppType ?? (FilterValue?)TargetProductPart ?? TargetTestType;

    /// <summary>
    /// Creates a rule. Throws <see cref="ArgumentException"/> unless the target lies in a lower
    /// layer than the source (CAT-13).
    /// </summary>
    public static FilterExclusion Create(FilterValue source, FilterValue target, string reasonDe)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonDe);
        if (target.Layer <= source.Layer)
        {
            throw new ArgumentException(
                $"An exclusion must point to a lower layer: '{source.Key}' ({source.Layer}) cannot exclude " +
                $"'{target.Key}' ({target.Layer}).",
                nameof(target));
        }

        var exclusion = new FilterExclusion { ReasonDe = reasonDe };
        switch (source)
        {
            case UseCaseTemplate useCase:
                exclusion.SourceUseCase = useCase;
                break;
            case AppType appType:
                exclusion.SourceAppType = appType;
                break;
            case ProductPart productPart:
                exclusion.SourceProductPart = productPart;
                break;
            default:
                // Only a test type is left, and it is the lowest layer: the layer check above
                // already refused it.
                throw new ArgumentException($"Unknown filter value type {source.GetType().Name}.", nameof(source));
        }

        switch (target)
        {
            case AppType appType:
                exclusion.TargetAppType = appType;
                break;
            case ProductPart productPart:
                exclusion.TargetProductPart = productPart;
                break;
            case TestType testType:
                exclusion.TargetTestType = testType;
                break;
            default:
                throw new ArgumentException($"Unknown filter value type {target.GetType().Name}.", nameof(target));
        }

        return exclusion;
    }
}
