// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace ProcureAccess.Modules.Catalog.Http;

/// <summary>
/// The language of a catalog answer (API-06, ARCHITECTURE §7.1): the catalog endpoints honour
/// <c>Accept-Language</c> with <c>de</c> and <c>en</c>, and German is the fallback for everything
/// else — a language the release has no texts in, an unknown one, none, or a header that cannot be
/// read. Never 406: a catalog answer in German is more useful than no answer (S4-23).
/// </summary>
/// <remarks>
/// Only the header counts, not the culture of the host's request localization: that one also reads
/// a query parameter and a cookie, and an answer that depended on a cookie could not be cached with
/// <c>Vary: Accept-Language</c> (ARCHITECTURE §9.6).
/// </remarks>
internal static class CatalogLanguage
{
    /// <summary>German, the language of every catalog text so far and the fallback.</summary>
    public const string German = "de";

    /// <summary>English, which a release may carry later (SPEC §3: English user interface later).</summary>
    public const string English = "en";

    /// <summary>
    /// The languages every stored release has texts in. Releases have only German texts so far: the
    /// release folder format has no language, and English texts come with an ADR of their own.
    /// </summary>
    public static IReadOnlyCollection<string> OfReleases { get; } = [German];

    /// <summary>The sentence the OpenAPI description of a catalog endpoint ends with.</summary>
    public const string OpenApiNote =
        " Honours Accept-Language (de, en) and names the language served in Content-Language; " +
        "German when the release has no texts in the requested language.";

    /// <summary>
    /// Picks the language of the answer: the first of the client's languages by weight (ties in header
    /// order) that the release has, where <c>en-US</c> counts as <c>en</c> and <c>*</c> as any
    /// language the header does not name; German when none fits.
    /// </summary>
    /// <param name="acceptLanguage">The values of the request's <c>Accept-Language</c> header.</param>
    /// <param name="available">The languages the release has texts in; always contains German.</param>
    public static string Negotiate(StringValues acceptLanguage, IReadOnlyCollection<string> available)
    {
        ArgumentNullException.ThrowIfNull(available);
        if (!StringWithQualityHeaderValue.TryParseList(acceptLanguage, out var ranges))
        {
            return German;
        }

        var named = ranges.Select(range => PrimaryLanguage(range.Value)).ToHashSet(StringComparer.Ordinal);
        foreach (var range in ranges.Where(r => (r.Quality ?? 1) > 0).OrderByDescending(r => r.Quality ?? 1))
        {
            var language = PrimaryLanguage(range.Value);
            if (language == "*")
            {
                var unnamed = available.FirstOrDefault(a => !named.Contains(a));
                if (unnamed is not null)
                {
                    return unnamed;
                }
            }
            else if (available.Contains(language))
            {
                return language;
            }
        }

        return German;
    }

    /// <summary>The primary subtag of a language range in lower case: <c>en-US</c> → <c>en</c>.</summary>
    private static string PrimaryLanguage(StringSegment range)
    {
        var dash = range.IndexOf('-');
        var primary = dash < 0 ? range : range.Subsegment(0, dash);
        return primary.Value!.ToLowerInvariant();
    }
}
