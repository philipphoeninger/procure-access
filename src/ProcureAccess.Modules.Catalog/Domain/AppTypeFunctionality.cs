// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Open or closed functionality of an app type in the sense of BIT inklusiv (Kriterienkatalog §2.2):
/// whether assistive technology such as a screen reader can be used. Only non-web software and
/// mobile apps make the distinction.
/// </summary>
internal enum AppTypeFunctionality
{
    NotApplicable = 1,
    Open = 2,
    Closed = 3,
}
