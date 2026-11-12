// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Export;

/// <summary>
/// The licence of the font package ClosedXML brings (ARCHITECTURE D12, C124): SixLabors.Fonts 2.0+
/// carries the Six Labors Split License, which D12 does not allow. ClosedXML accepts [1.0.0, 3.0.0)
/// and NuGet resolves 1.0.0 (Apache-2.0); Dependabot ignores 2.0+, and this test fails when another
/// package or a ClosedXML update pulls a newer major in.
/// </summary>
public class ClosedXmlLicenceTests
{
    [Fact]
    public void ClosedXmlFonts_ResolvedVersion_IsTheApacheLicensedMajorOne() =>
        typeof(SixLabors.Fonts.FontCollection).Assembly.GetName().Version!.Major.ShouldBe(1);
}
