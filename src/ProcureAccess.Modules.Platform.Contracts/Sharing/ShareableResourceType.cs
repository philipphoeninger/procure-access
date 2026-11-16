// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Sharing;

/// <summary>
/// What a share link points to (ARCHITECTURE §5.3, C59). In v1 only an organisation's saved
/// criteria list can be shared; there are no project share links (D7).
/// </summary>
public enum ShareableResourceType
{
    /// <summary>A saved criteria list of the Configurator module.</summary>
    Configuration,
}
