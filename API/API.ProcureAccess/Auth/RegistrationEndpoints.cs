// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net.Mail;
using Microsoft.AspNetCore.Http.HttpResults;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.RateLimiting;
using IEmailSender = ProcureAccess.Modules.Platform.Contracts.Email.IEmailSender;

namespace API.ProcureAccess.Auth;

/// <summary>
/// Registration and e-mail confirmation (SPEC R-AUTH-01, R-AUTH-02, §8.8 v2.21; ARCHITECTURE §7.1,
/// §9.1, C72; S5-12a). In the host next to sign-in, because they need the legacy
/// <see cref="UserManager{TUser}"/>; they move into the IdentityAccess module with the users (M3.1).
/// </summary>
/// <remarks>
/// <para><c>POST /api/v1/auth/register</c>: 204 for a new and for a known address alike, so the
/// answer never tells whether an account exists (R-AUTH-01). A new address gets an account without
/// a role (O-S5-3 (a), #301) and the confirmation e-mail; a known one the „already registered“
/// e-mail. A failed send is logged and the answer stays 204, because a retry of the registration
/// would take the known-address path (owner decision S5-12 4 (a)); the user signs in and asks for
/// the e-mail again. Rate limit <see cref="RateLimitPolicies.Registration"/>.</para>
/// <para><c>POST /api/v1/auth/confirm-email</c>: redeems the token of the link; 204 when the
/// address is confirmed now or the token was used before for a confirmed account (owner decision
/// S5-12 4 (b)); otherwise 400 <c>validation</c> at <c>token</c>.</para>
/// <para><c>POST /api/v1/auth/confirm-email/resend</c>: signed in only; sends a new link unless the
/// address is confirmed. A failed send answers 503 <c>email-delivery</c>. Rate limit
/// <see cref="RateLimitPolicies.ConfirmationResend"/> per user.</para>
/// <para>All three are state-changing and validated against antiforgery (SEC-02); the client has the
/// token of the session probe.</para>
/// </remarks>
public static class RegistrationEndpoints
{
    public const string RegisterPath = "/register";
    public const string ConfirmEmailPath = "/confirm-email";
    public const string ResendPath = "/confirm-email/resend";

    /// <summary>Longest e-mail address: the column of the legacy <c>identity."Users"</c>.</summary>
    public const int EmailMaxLength = 256;

    /// <summary>Maps the endpoints below <c>/auth</c> of <paramref name="routes"/>, the versioned group.</summary>
    public static IEndpointRouteBuilder MapRegistrationEndpoints(this IEndpointRouteBuilder routes)
    {
        var auth = routes.MapGroup("/auth").WithTags("Auth");

        auth.MapPost(RegisterPath, RegisterAsync)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Registration)
            .WithName("PostRegister")
            .WithSummary("Creates an account and sends the confirmation e-mail")
            .WithDescription(
                "204 for a new and a known address alike (a known address gets the 'already registered' e-mail). " +
                "400 validation keyed email, password, privacyAcknowledged; 400 antiforgery; 429 rate-limit.")
            .Accepts<RegisterRequestBody>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        auth.MapPost(ConfirmEmailPath, ConfirmEmailAsync)
            .AllowAnonymous()
            .WithName("PostConfirmEmail")
            .WithSummary("Confirms the e-mail address with the token of the confirmation link")
            .WithDescription(
                "204 when the address is confirmed (also for a token used before for a confirmed account). " +
                "400 validation at 'token' for an unknown, expired or used token.")
            .Accepts<ConfirmEmailRequestBody>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        auth.MapPost(ResendPath, ResendAsync)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.ConfirmationResend)
            .WithName("PostResendConfirmation")
            .WithSummary("Sends the confirmation e-mail again to the signed-in user")
            .WithDescription(
                "204, also when the address is already confirmed (then no e-mail). 401 without a session; " +
                "503 email-delivery when the e-mail could not be sent; 429 rate-limit.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return routes;
    }

    private static async Task<Results<NoContent, ValidationProblem>> RegisterAsync(
        RegisterRequestBody body,
        UserManager<User> users,
        IPasswordHasher<User> passwordHasher,
        IOptions<IdentityOptions> identityOptions,
        IAccountTokens tokens,
        IEmailSender emails,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var errors = Validate(body, identityOptions.Value.Password.RequiredLength);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var logger = loggerFactory.CreateLogger(typeof(RegistrationEndpoints));
        var address = body.Email!.Trim();
        var password = body.Password!;

        var existing = await users.FindByEmailAsync(address);
        if (existing is not null)
        {
            await KnownAddressAsync(existing, password, passwordHasher, emails, configuration, logger, cancellationToken);
            return TypedResults.NoContent();
        }

        var user = new User { Email = address, UserName = address };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            // The address is taken after all (a registration in between, or a user name without
            // the same e-mail): answer as for a known address, never with Identity's "taken" text.
            if (created.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
            {
                var taken = await users.FindByEmailAsync(address) ?? await users.FindByNameAsync(address);
                if (taken is not null)
                {
                    await KnownAddressAsync(taken, password, passwordHasher, emails, configuration, logger, cancellationToken);
                }

                return TypedResults.NoContent();
            }

            return TypedResults.ValidationProblem(ErrorsOf(created), type: ProblemTypes.Validation);
        }

        var issued = await tokens.IssueAsync(user.Id, AccountTokenPurpose.EmailConfirmation, cancellationToken);
        await TrySendAsync(
            emails,
            new AddressConfirmationEmail(address, ConfirmationLink(configuration, issued), issued.ValidHours),
            logger,
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, ValidationProblem>> ConfirmEmailAsync(
        ConfirmEmailRequestBody body,
        IAccountTokens tokens,
        UserManager<User> users,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Token))
        {
            return TokenProblem("The token is required.");
        }

        var redemption = await tokens.RedeemAsync(body.Token, AccountTokenPurpose.EmailConfirmation, cancellationToken);
        var user = redemption.UserId is null ? null : await users.FindByIdAsync(redemption.UserId);
        if (user is null || user.IsDeleted)
        {
            return TokenProblem("The link is unknown, expired or already used.");
        }

        switch (redemption.Outcome)
        {
            case AccountTokenOutcome.Redeemed when !user.EmailConfirmed:
                user.EmailConfirmed = true;
                var updated = await users.UpdateAsync(user);
                if (!updated.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Confirming the address failed: " + string.Join(", ", updated.Errors.Select(e => e.Code)));
                }

                return TypedResults.NoContent();
            case AccountTokenOutcome.Redeemed:
            case AccountTokenOutcome.AlreadyUsed when user.EmailConfirmed:
                return TypedResults.NoContent();
            default:
                return TokenProblem("The link is unknown, expired or already used.");
        }
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> ResendAsync(
        ClaimsPrincipal principal,
        UserManager<User> users,
        IAccountTokens tokens,
        IEmailSender emails,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(principal);
        if (user is null || user.IsDeleted || user.EmailConfirmed || string.IsNullOrEmpty(user.Email))
        {
            return TypedResults.NoContent();
        }

        var issued = await tokens.IssueAsync(user.Id, AccountTokenPurpose.EmailConfirmation, cancellationToken);
        var sent = await TrySendAsync(
            emails,
            new AddressConfirmationEmail(user.Email, ConfirmationLink(configuration, issued), issued.ValidHours),
            loggerFactory.CreateLogger(typeof(RegistrationEndpoints)),
            cancellationToken);

        return sent
            ? TypedResults.NoContent()
            : TypedResults.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                type: ProblemTypes.EmailDelivery,
                title: "E-mail not sent",
                detail: "The confirmation e-mail could not be sent. Try again in a few minutes.");
    }

    /// <summary>
    /// A known address: the „already registered“ e-mail instead of a confirmation (R-AUTH-01). The
    /// password is hashed anyway, so the answer takes about as long as for a new account.
    /// </summary>
    private static async Task KnownAddressAsync(
        User existing,
        string password,
        IPasswordHasher<User> passwordHasher,
        IEmailSender emails,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        _ = passwordHasher.HashPassword(new User(), password);
        if (existing.IsDeleted || string.IsNullOrEmpty(existing.Email))
        {
            return;
        }

        await TrySendAsync(
            emails,
            new AlreadyRegisteredEmail(existing.Email, ClientLinks.To(configuration, ClientLinks.LoginPagePath)),
            logger,
            cancellationToken);
    }

    /// <summary>Sends <paramref name="message"/>; false after a failed send, logged without the address (PRV-11).</summary>
    private static async Task<bool> TrySendAsync(
        IEmailSender emails, EmailMessage message, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            await emails.SendAsync(message, cancellationToken);
            return true;
        }
        catch (EmailDeliveryException)
        {
            logger.LogWarning("An account e-mail ({MessageType}) could not be sent.", message.MessageType);
            return false;
        }
    }

    private static Uri ConfirmationLink(IConfiguration configuration, IssuedAccountToken issued) =>
        ClientLinks.To(configuration, ClientLinks.ConfirmationPagePath + "?token=" + Uri.EscapeDataString(issued.Token));

    private static Dictionary<string, string[]> Validate(RegisterRequestBody body, int passwordMinLength)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var email = body.Email?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            errors["email"] = ["The e-mail address is required."];
        }
        else if (email.Length > EmailMaxLength || !IsEmailAddress(email))
        {
            errors["email"] = ["The e-mail address is not valid."];
        }

        if (string.IsNullOrEmpty(body.Password))
        {
            errors["password"] = ["The password is required."];
        }
        else if (body.Password.Length < passwordMinLength)
        {
            errors["password"] = [$"The password must have at least {passwordMinLength} characters."];
        }
        else if (body.Password.Length > PasswordPolicy.MaxLength)
        {
            errors["password"] = [$"The password must have at most {PasswordPolicy.MaxLength} characters."];
        }

        if (body.PrivacyAcknowledged != true)
        {
            errors["privacyAcknowledged"] = ["The privacy notice must be acknowledged."];
        }

        return errors;
    }

    /// <summary>One plain address (<c>name@domain</c>), no display name and no list.</summary>
    private static bool IsEmailAddress(string value) =>
        MailAddress.TryCreate(value, out var parsed)
        && string.Equals(parsed.Address, value, StringComparison.Ordinal)
        && parsed.Host.Length > 0;

    /// <summary>Identity's refusals keyed by field: password rules at <c>password</c>, the rest at <c>email</c>.</summary>
    private static Dictionary<string, string[]> ErrorsOf(IdentityResult result)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var group in result.Errors.GroupBy(e => e.Code.StartsWith("Password", StringComparison.Ordinal) ? "password" : "email"))
        {
            errors[group.Key] = [.. group.Select(e => e.Description)];
        }

        return errors;
    }

    private static ValidationProblem TokenProblem(string message) =>
        TypedResults.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal) { ["token"] = [message] },
            type: ProblemTypes.Validation);
}

/// <summary>Body of <c>POST /api/v1/auth/register</c>. All three members are required.</summary>
/// <param name="Email">The e-mail address; it is also the user name.</param>
/// <param name="Password">At least 🔧 12, at most 128 characters, no composition rules.</param>
/// <param name="PrivacyAcknowledged">Must be true (⚖️ `auth.register.consent`); checked, not stored (decision S5-12 5 (a)).</param>
public sealed record RegisterRequestBody(string? Email, string? Password, bool? PrivacyAcknowledged);

/// <summary>Body of <c>POST /api/v1/auth/confirm-email</c>: the token of the link.</summary>
public sealed record ConfirmEmailRequestBody(string? Token);
