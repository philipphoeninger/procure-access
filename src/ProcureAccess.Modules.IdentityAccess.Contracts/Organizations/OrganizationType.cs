// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

/// <summary>The kind of organisation (ARCHITECTURE §5.2, SPEC R-ORG-01): public body, company or association.</summary>
public enum OrganizationType
{
    PublicBody = 1,
    Company = 2,
    Association = 3,
}
