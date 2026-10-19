// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.SeedData;

public static class FilterTypeSeed
{
    public static readonly FilterType[] Data =
    [
        new() { Id = 1, Name = "Produkt-Typen", Description = ""},
        new() { Id = 2, Name = "App-Typen", Description = ""},
        new() { Id = 3, Name = "Produkt-Teile", Description = ""},
        new() { Id = 4, Name = "Test-Typen", Description = ""}
    ];
}
