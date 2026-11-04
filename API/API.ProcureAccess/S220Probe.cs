// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// S2-20 CI probe (throwaway draft PR #173, never merged): breaks the build on purpose to show
// that stage 8 is skipped without a build (#106).
namespace API.ProcureAccess;

internal static class S220Probe
{
    internal static int Broken() => "not an int";
}
