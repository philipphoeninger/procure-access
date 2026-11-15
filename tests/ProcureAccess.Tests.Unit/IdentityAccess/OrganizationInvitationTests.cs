// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Organizations;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// <see cref="OrganizationInvitation"/>, <see cref="IssuedInvitation.DaysOf"/> and
/// <see cref="InvitationSettings"/> (S5-17a; ARCHITECTURE §5.2, §9.1, C72, C80; SPEC R-ORG-02).
/// </summary>
public class OrganizationInvitationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(14);
    private static readonly Guid OrganizationId = Guid.NewGuid();

    [Fact]
    public void Create_ValidValues_KeepsThemOpenUntilTheLifetimeEnds()
    {
        var hash = new byte[32];

        var invitation = OrganizationInvitation.Create(OrganizationId, "Anna.Muster@Example.test", OrganizationRole.Editor, hash, Now, Lifetime);

        invitation.Id.ShouldNotBe(Guid.Empty);
        invitation.OrganizationId.ShouldBe(OrganizationId);
        invitation.Email.ShouldBe("Anna.Muster@Example.test");
        invitation.NormalizedEmail.ShouldBe("ANNA.MUSTER@EXAMPLE.TEST");
        invitation.Role.ShouldBe(OrganizationRole.Editor);
        invitation.TokenHash.ShouldBe(hash);
        invitation.CreatedAt.ShouldBe(Now);
        invitation.ExpiresAt.ShouldBe(Now.AddDays(14));
        invitation.AcceptedAt.ShouldBeNull();
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(IsOpenAt_AtTheEndOfTheLifetime_IsFalse))]
    [Trait("Invariant", "SEC-11")]
    public void IsOpenAt_AtTheEndOfTheLifetime_IsFalse()
    {
        var invitation = AnInvitation();

        invitation.IsOpenAt(Now.AddDays(14).AddSeconds(-1)).ShouldBeTrue();
        invitation.IsOpenAt(Now.AddDays(14)).ShouldBeFalse();
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Accept_Twice_IsRefusedTheSecondTime))]
    [Trait("Invariant", "SEC-11")]
    public void Accept_Twice_IsRefusedTheSecondTime()
    {
        var invitation = AnInvitation();

        invitation.Accept(Now.AddDays(1));

        invitation.AcceptedAt.ShouldBe(Now.AddDays(1));
        invitation.IsOpenAt(Now.AddDays(1)).ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => invitation.Accept(Now.AddDays(2)));
    }

    [Fact]
    public void Accept_AfterExpiry_Throws() =>
        Should.Throw<InvalidOperationException>(() => AnInvitation().Accept(Now.AddDays(14)));

    [Theory]
    [InlineData("anna@example.test", true)]
    [InlineData("ANNA@EXAMPLE.TEST", true)]
    [InlineData(" anna@example.test ", true)]
    [InlineData("anna@example.org", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsFor_Address_ComparesAsIdentityNormalisesIt(string? email, bool expected) =>
        AnInvitation("Anna@Example.test").IsFor(email).ShouldBe(expected);

    [Fact]
    public void ToString_NeverNamesTheAddress() =>
        AnInvitation("anna@example.test").ToString().ShouldNotContain("anna");

    [Fact]
    public void Create_EmptyOrganization_Throws() =>
        Should.Throw<ArgumentException>(() =>
            OrganizationInvitation.Create(Guid.Empty, "a@example.test", OrganizationRole.Reader, new byte[32], Now, Lifetime));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NoAddress_Throws(string email) =>
        Should.Throw<ArgumentException>(() =>
            OrganizationInvitation.Create(OrganizationId, email, OrganizationRole.Reader, new byte[32], Now, Lifetime));

    [Fact]
    public void Create_AddressOver256Characters_Throws() =>
        Should.Throw<ArgumentException>(() =>
            OrganizationInvitation.Create(OrganizationId, new string('a', 245) + "@example.test", OrganizationRole.Reader, new byte[32], Now, Lifetime));

    [Fact]
    public void Create_UndefinedRole_Throws() =>
        Should.Throw<ArgumentOutOfRangeException>(() =>
            OrganizationInvitation.Create(OrganizationId, "a@example.test", (OrganizationRole)99, new byte[32], Now, Lifetime));

    [Fact]
    public void Create_HashOfTheWrongLength_Throws() =>
        Should.Throw<ArgumentException>(() =>
            OrganizationInvitation.Create(OrganizationId, "a@example.test", OrganizationRole.Reader, new byte[16], Now, Lifetime));

    [Fact]
    public void Create_LifetimeNotPositive_Throws() =>
        Should.Throw<ArgumentOutOfRangeException>(() =>
            OrganizationInvitation.Create(OrganizationId, "a@example.test", OrganizationRole.Reader, new byte[32], Now, TimeSpan.Zero));

    [Theory]
    [InlineData(14 * 24, 14)]
    [InlineData(1, 1)]
    [InlineData(25, 2)]
    public void DaysOf_Lifetime_IsWholeDaysRoundedUp(int hours, int expected) =>
        IssuedInvitation.DaysOf(TimeSpan.FromHours(hours)).ShouldBe(expected);

    [Fact]
    public void Settings_ValidValues_AreRead()
    {
        var settings = InvitationSettings.From(Configuration("14.00:00:00", "01:00:00"));

        settings.Lifetime.ShouldBe(TimeSpan.FromDays(14));
        settings.CleanupInterval.ShouldBe(TimeSpan.FromHours(1));
    }

    [Theory]
    [InlineData(null, "01:00:00")]
    [InlineData("14 Tage", "01:00:00")]
    [InlineData("00:00:00", "01:00:00")]
    [InlineData("14.00:00:00", "-01:00:00")]
    public void Settings_MissingOrUnusableValue_StopsTheStart(string? lifetime, string interval) =>
        Should.Throw<InvalidOperationException>(() => InvitationSettings.From(Configuration(lifetime, interval)));

    private static OrganizationInvitation AnInvitation(string email = "anna@example.test") =>
        OrganizationInvitation.Create(OrganizationId, email, OrganizationRole.Reader, new byte[32], Now, Lifetime);

    private static IConfiguration Configuration(string? lifetime, string interval) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Organizations:Invitations:Lifetime"] = lifetime,
                ["Organizations:Invitations:CleanupInterval"] = interval,
            })
            .Build();
}
