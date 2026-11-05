// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>Standard a <see cref="StandardReference"/> points into (ARCHITECTURE §5.1).</summary>
internal enum Standard
{
    En301549 = 1,
    Wcag = 2,
    BitInklusiv = 3,
    Iso9241 = 4,
}
