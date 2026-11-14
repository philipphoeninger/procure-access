// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities.Authorization;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// The legacy sign-up and password reset send their mails to the user's own address (#302, S5-11,
/// owner decision 3a), through Platform's <see cref="IEmailSender"/> with the <c>email.*</c> texts.
/// These flows go with S5-10; S5-12 and S5-13 test their replacements.
/// </summary>
public sealed class LegacyAccountEmailTests(ApiFixture api) : IntegrationTest(api)
{
    private const string Address = "new.user@example.test";
    private const string Password = "Sicheres-Passwort-2026!";

    private CapturingEmailSender Emails => Api.Factory.Emails;

    [Fact]
    public async Task SignUp_NewAccount_SendsTheConfirmationToItsOwnAddress()
    {
        var client = await PrepareAsync();

        (await SignUpAsync(client)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var confirmation = Emails.Sent.ShouldHaveSingleItem().ShouldBeOfType<AddressConfirmationEmail>();
        confirmation.Recipient.ShouldBe(Address);
        confirmation.Link.AbsoluteUri.ShouldStartWith("http://procureaccess.tech:4200/confirm-email?userId=");
    }

    [Fact]
    public async Task ForgotPassword_ConfirmedAccount_SendsTheResetToItsOwnAddress()
    {
        var client = await PrepareAsync();
        await SignUpAsync(client);
        var confirmation = (AddressConfirmationEmail)Emails.Sent.Single();
        (await client.GetAsync($"/api{confirmation.Link.PathAndQuery}", TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
        Emails.Clear();

        (await ForgotPasswordAsync(client, Address.ToUpperInvariant())).StatusCode.ShouldBe(HttpStatusCode.OK);

        var reset = Emails.Sent.ShouldHaveSingleItem().ShouldBeOfType<PasswordResetEmail>();
        reset.Recipient.ShouldBe(Address);
        reset.ValidHours.ShouldBe(24, "the lifetime of the legacy Identity token");
        reset.Link.AbsoluteUri.ShouldStartWith($"http://procureaccess.tech:4200/reset-password?email={Uri.EscapeDataString(Address)}&token=");
    }

    [Fact]
    public async Task ForgotPassword_UnknownAddress_SendsNothingAndAnswersTheSame()
    {
        var client = await PrepareAsync();

        (await ForgotPasswordAsync(client, "nobody@example.test")).StatusCode.ShouldBe(HttpStatusCode.OK);

        Emails.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task SignUp_MailCannotBeSent_KeepsTheAccount()
    {
        var client = await PrepareAsync();
        Emails.Fail = true;

        (await SignUpAsync(client)).StatusCode.ShouldBe(HttpStatusCode.OK);

        using var scope = Api.Factory.Services.CreateScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<User>>().FindByEmailAsync(Address);
        user.ShouldNotBeNull().EmailConfirmed.ShouldBeFalse();
    }

    /// <summary>A fresh database with the legacy <c>Member</c> role, which sign-up grants (#301), and no mail sent.</summary>
    private async Task<HttpClient> PrepareAsync()
    {
        await Api.ResetDatabaseAsync();
        using (var scope = Api.Factory.Services.CreateScope())
        {
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roles.RoleExistsAsync(Roles.Member))
            {
                await roles.CreateAsync(new IdentityRole(Roles.Member));
            }
        }

        Emails.Clear();
        return CreateClient();
    }

    private static Task<HttpResponseMessage> SignUpAsync(HttpClient client) =>
        client.PostAsJsonAsync("/api/signUp", new { email = Address, password = Password }, TestContext.Current.CancellationToken);

    private static Task<HttpResponseMessage> ForgotPasswordAsync(HttpClient client, string email) =>
        client.PostAsJsonAsync("/api/forgot-password", new { email }, TestContext.Current.CancellationToken);
}
