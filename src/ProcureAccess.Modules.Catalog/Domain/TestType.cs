// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Layer 4, „Prüfverfahren“ (ARCHITECTURE §5.1, Kriterienkatalog §2.4): BIT inklusiv Software-Test,
/// App-Test, PDF-Test; BIK BITV-Test (Web), (App). Selecting one adds its procedure criterion
/// <c>PROC-&lt;key&gt;</c> (CAT-16); its texts come from <see cref="CriterionProcedureText"/>.
/// </summary>
internal sealed class TestType : FilterValue
{
    public const int UrlMaxLength = 2048;

    private TestType()
    {
    }

    /// <summary>Link to the procedure.</summary>
    public string Url { get; private init; } = null!;

    public int SortOrder { get; private init; }

    public override FilterLayer Layer => FilterLayer.TestType;

    public static TestType Create(string key, string nameDe, string url, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameDe);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        return new TestType { Key = key, NameDe = nameDe, Url = url, SortOrder = sortOrder };
    }
}
