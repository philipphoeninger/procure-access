// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// A clause of a standard, shared by every revision that cites it (m:n, ARCHITECTURE §5.1).
/// </summary>
/// <remarks>
/// It has no method that changes it: a row that a revision of a published release cites is part
/// of that release (owner, S3-12 decision 2), and a row nobody cites yet is just as fixed. A
/// corrected clause or URL is a new row, cited by a new revision. One row per standard and clause.
/// </remarks>
internal sealed class StandardReference : CatalogEntity
{
    public const int ClauseMaxLength = 64;
    public const int UrlMaxLength = 2048;

    private StandardReference()
    {
    }

    public Standard Standard { get; private set; }

    /// <summary>Clause or success criterion, e.g. <c>11.2.1.1</c> or <c>1.4.3</c>.</summary>
    public string Clause { get; private set; } = null!;

    public string? Url { get; private set; }

    public static StandardReference Create(Standard standard, string clause, string? url = null)
    {
        if (!Enum.IsDefined(standard))
        {
            throw new ArgumentOutOfRangeException(nameof(standard), standard, "Unknown standard.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(clause);
        return new StandardReference { Standard = standard, Clause = clause, Url = url };
    }
}
