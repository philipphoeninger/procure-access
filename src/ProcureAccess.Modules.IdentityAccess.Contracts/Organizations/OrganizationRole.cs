// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

/// <summary>
/// The role of a membership (ARCHITECTURE §9.1, SPEC R-ORG-05), ordered so that a higher value
/// includes the rights of a lower one: a Reader reads everything of the organisation and changes
/// nothing; an Editor creates and edits lists, projects, candidates and assessments and publishes
/// and withdraws the organisation's entries; an Owner also manages members and settings.
/// </summary>
public enum OrganizationRole
{
    Reader = 1,
    Editor = 2,
    Owner = 3,
}
