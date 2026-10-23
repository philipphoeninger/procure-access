// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// Antiforgery hook of the test client (TESTING §5.2, ARCHITECTURE §8.3). Before the first
/// state-changing request it calls the session probe once, which issues the <c>XSRF-TOKEN</c>
/// cookie; on every state-changing request it sends the current cookie value as
/// <c>X-XSRF-TOKEN</c>.
/// </summary>
/// <remarks>
/// The session probe only exists from migration step 3 on. Until then it answers 404, which is
/// tolerated: requests go out without the header, and no test has to change once antiforgery
/// validation is switched on. The cookie is read on every request, so a token that rotates on
/// sign-in or sign-out is picked up. Not thread-safe: one client per test.
/// </remarks>
/// <param name="cookies">The cookie jar shared with the <c>CookieContainerHandler</c> below this handler.</param>
public sealed class AntiforgeryHandler(CookieContainer cookies) : DelegatingHandler
{
    public const string SessionProbePath = "/api/v1/auth/session";
    public const string TokenCookieName = "XSRF-TOKEN";
    public const string TokenHeaderName = "X-XSRF-TOKEN";

    private bool _probed;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!IsStateChanging(request.Method))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var requestUri = request.RequestUri
            ?? throw new InvalidOperationException("The request has no URI.");

        if (!_probed)
        {
            await ProbeSessionAsync(request, requestUri, cancellationToken);
            _probed = true;
        }

        var token = cookies.GetCookies(requestUri)[TokenCookieName]?.Value;
        if (token is not null && !request.Headers.Contains(TokenHeaderName))
        {
            request.Headers.Add(TokenHeaderName, token);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task ProbeSessionAsync(
        HttpRequestMessage request,
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        using var probe = new HttpRequestMessage(HttpMethod.Get, new Uri(requestUri, SessionProbePath));

        // The token is bound to the signed-in user, so the probe runs as the same user.
        foreach (var header in request.Headers)
        {
            probe.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        using var response = await base.SendAsync(probe, cancellationToken);

        // 404: the probe does not exist yet (before migration step 3). Anything else that is not
        // a success is a broken probe and must not be hidden behind later antiforgery failures.
        if (response.StatusCode != HttpStatusCode.NotFound && !response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Session probe {SessionProbePath} answered {(int)response.StatusCode}.");
        }
    }

    private static bool IsStateChanging(HttpMethod method) =>
        method != HttpMethod.Get
        && method != HttpMethod.Head
        && method != HttpMethod.Options
        && method != HttpMethod.Trace;
}
