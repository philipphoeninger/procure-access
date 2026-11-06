// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// The four selection layers in their fixed order (ARCHITECTURE §5.1, „Filter layers and
/// exclusions“; Kriterienkatalog §2). A higher layer has a lower number; an exclusion always points
/// from a higher layer to a lower one (CAT-13).
/// </summary>
internal enum FilterLayer
{
    /// <summary>„Verwendungszweck“ (<see cref="UseCaseTemplate"/>).</summary>
    UseCase = 1,

    /// <summary>„Anwendungstyp“ (<see cref="AppType"/>).</summary>
    AppType = 2,

    /// <summary>„Funktionsbereiche“ (<see cref="ProductPart"/>).</summary>
    ProductPart = 3,

    /// <summary>„Prüfverfahren“ (<see cref="TestType"/>).</summary>
    TestType = 4,
}
