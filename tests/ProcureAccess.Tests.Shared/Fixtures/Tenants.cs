// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Tests.Shared.Auth;

namespace ProcureAccess.Tests.Shared.Fixtures;

/// <summary>
/// The Tenants fixture of TESTING §6.2 (S5-15): organisations <see cref="A"/> and <see cref="B"/>,
/// each with the Owner, Editor and Reader of <see cref="TestUsers"/>; <see cref="TestUsers.Member"/>
/// is the user without organisation. Store <see cref="Entities"/> through the fixture
/// (<c>StoreAsync</c>). The organisation ids are random per build, like the real ones.
/// </summary>
/// <remarks>
/// The contributor profiles of the users without organisation (individual, vendor, auditor) and the
/// platform-role users come with the steps that need them (TESTING §9 step 3, steps 6 and 7).
/// </remarks>
internal sealed class Tenants
{
    private Tenants(Organization a, Organization b, IReadOnlyList<OrganizationMembership> memberships)
    {
        A = a;
        B = b;
        Memberships = memberships;
    }

    public Organization A { get; }

    public Organization B { get; }

    /// <summary>Six rows: Owner, Editor and Reader of each organisation.</summary>
    public IReadOnlyList<OrganizationMembership> Memberships { get; }

    /// <summary>Both organisations and every membership, in an order the foreign keys accept.</summary>
    public object[] Entities => [A, B, .. Memberships];

    /// <summary>Builds the fixture at <paramref name="now"/> (the API's clock in integration tests).</summary>
    public static Tenants Build(DateTimeOffset now)
    {
        var a = Organization.Create("Organisation A", OrganizationType.PublicBody, TestUsers.OwnerA.Id, now);
        var b = Organization.Create("Organisation B", OrganizationType.Company, TestUsers.OwnerB.Id, now);
        return new Tenants(a, b,
        [
            OrganizationMembership.Founder(a, now),
            OrganizationMembership.Create(a.Id, TestUsers.EditorA.Id, OrganizationRole.Editor, now, now),
            OrganizationMembership.Create(a.Id, TestUsers.ReaderA.Id, OrganizationRole.Reader, now, now),
            OrganizationMembership.Founder(b, now),
            OrganizationMembership.Create(b.Id, TestUsers.EditorB.Id, OrganizationRole.Editor, now, now),
            OrganizationMembership.Create(b.Id, TestUsers.ReaderB.Id, OrganizationRole.Reader, now, now),
        ]);
    }
}
