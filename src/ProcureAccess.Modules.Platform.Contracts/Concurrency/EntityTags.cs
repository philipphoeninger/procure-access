// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace ProcureAccess.Modules.Platform.Contracts.Concurrency;

/// <summary>
/// The <c>ETag</c> of a shared resource and its comparison with <c>If-Match</c> (ARCHITECTURE §7.1;
/// API-03, API-04; S6-20): <c>"&lt;epoch&gt;-&lt;version&gt;"</c>, a strong tag of the 🔧
/// <c>Concurrency:ETagEpoch</c> and the version the resource counts with every change of its own
/// fields. One instance per application, registered by Platform.
/// </summary>
/// <remarks>
/// <b>The epoch (restore safety).</b> A restored database brings back older versions, so a tag
/// issued before the restore could match a row it was not issued for. Raising the epoch after a
/// restore makes every tag issued before stale (runbook: <c>TECHNICAL.md</c>).
/// </remarks>
public sealed class EntityTags
{
    private readonly int _epoch;

    /// <param name="epoch">🔧 <c>Concurrency:ETagEpoch</c>, at least 1.</param>
    public EntityTags(int epoch)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(epoch, 1);
        _epoch = epoch;
    }

    /// <summary>The value of the <c>ETag</c> header for <paramref name="version"/>, with its quotes.</summary>
    public string Of(int version) => string.Create(CultureInfo.InvariantCulture, $"\"{_epoch}-{version}\"");

    /// <summary>
    /// Whether <paramref name="ifMatch"/> is the tag of <paramref name="version"/> under the
    /// current epoch. Exactly one strong tag counts: <c>*</c>, a weak tag and a list of tags match
    /// nothing, because a change is allowed only on the one version its sender has seen.
    /// </summary>
    public bool Matches(string? ifMatch, int version) =>
        string.Equals(ifMatch?.Trim(), Of(version), StringComparison.Ordinal);
}
