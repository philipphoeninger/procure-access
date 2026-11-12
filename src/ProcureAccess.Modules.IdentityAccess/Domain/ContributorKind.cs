// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Domain;

/// <summary>
/// Kind of a <see cref="ContributorProfile"/> (ARCHITECTURE §5.2, SPEC R-CTR-01). Self-declared by
/// the user and never verified (§4.6, §16.2.7). Flow E needs <see cref="Vendor"/> or
/// <see cref="Auditor"/> (§9.1). Stored by name, so never rename a value.
/// </summary>
internal enum ContributorKind
{
    Individual,
    Institution,
    Vendor,
    Auditor,
}
