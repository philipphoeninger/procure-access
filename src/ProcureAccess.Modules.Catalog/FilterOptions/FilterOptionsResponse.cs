// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.FilterOptions;

// The answer of GET /api/v1/catalog/filter-options in the shape of SPEC §8.1 (C77). Ids are the
// keys of the filter values; JSON names are camelCase. Internal: the endpoint is a minimal API
// (O-S3-6), so no contract type is needed.

/// <summary>The whole answer: the release whose rules apply and the four layers.</summary>
internal sealed record FilterOptionsResponse(string Release, FilterOptionsLayers Layers);

/// <summary>The four layers in their fixed order (Kriterienkatalog §2).</summary>
internal sealed record FilterOptionsLayers(
    IReadOnlyList<UseCaseOption> UseCases,
    IReadOnlyList<AppTypeOption> AppTypes,
    IReadOnlyList<ProductPartOption> ProductParts,
    IReadOnlyList<TestTypeOption> TestTypes);

/// <summary>An active use case („Verwendungszweck“); <see cref="Hint"/> is its description.</summary>
internal sealed record UseCaseOption(string Id, string Name, string? Hint, IReadOnlyList<ExcludingValue> ExcludedBy);

/// <summary>An app type („Anwendungstyp“).</summary>
internal sealed record AppTypeOption(string Id, string Name, string? Hint, IReadOnlyList<ExcludingValue> ExcludedBy);

/// <summary>A product part („Funktionsbereich“).</summary>
internal sealed record ProductPartOption(
    string Id,
    string Name,
    string Explanation,
    bool AlwaysSelected,
    IReadOnlyList<ExcludingValue> ExcludedBy);

/// <summary>A test type („Prüfverfahren“) with the link to its procedure.</summary>
internal sealed record TestTypeOption(string Id, string Name, string Url, IReadOnlyList<ExcludingValue> ExcludedBy);

/// <summary>
/// A selected higher-layer value that excludes the option; <see cref="Layer"/> is <c>useCase</c>,
/// <c>appType</c> or <c>productPart</c>. No reason text (C77).
/// </summary>
internal sealed record ExcludingValue(string Layer, string Id, string Name);
