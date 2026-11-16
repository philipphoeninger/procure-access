// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;

namespace ProcureAccess.Modules.Platform.Logging;

/// <summary>
/// Masks the token of a share link in every logged text property before a sink writes the event
/// (TEN-08; owner decision S5-23 1a). The token travels in the path of the anonymous read,
/// <c>/api/v1/shared/{token}</c>, and ASP.NET Core puts the path into the request scope
/// (<c>RequestPath</c>) of every event of a request and into the hosting events (<c>Path</c>); the
/// rendered message is built from these properties, so it is masked too.
/// </summary>
internal sealed partial class ShareTokenMaskingEnricher : ILogEventEnricher
{
    /// <summary>What replaces the token.</summary>
    public const string Mask = "***";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        List<LogEventProperty>? masked = null;
        foreach (var (name, value) in logEvent.Properties)
        {
            if (value is ScalarValue { Value: string text } && SharedPath().IsMatch(text))
            {
                (masked ??= []).Add(new LogEventProperty(name, new ScalarValue(SharedPath().Replace(text, "${prefix}" + Mask))));
            }
        }

        // Replaced after the loop: the properties cannot change while they are enumerated.
        foreach (var property in masked ?? [])
        {
            logEvent.AddOrUpdateProperty(property);
        }
    }

    /// <summary>The read's path up to the token, in any case (routing ignores it), and the token.</summary>
    [GeneratedRegex(@"(?<prefix>/api/v[^/]+/shared/)[^/?#\s]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SharedPath();
}
