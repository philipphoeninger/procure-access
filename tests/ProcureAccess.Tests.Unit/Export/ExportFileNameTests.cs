// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Export.Formatting;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Export;

/// <summary>The slug of a list name in the file name of a saved export (SPEC §8.3; rule confirmed by the owner in S5-21a).</summary>
public class ExportFileNameTests
{
    [Theory]
    [InlineData("Vergabe Büro-Software 2027", "Vergabe-Buero-Software-2027")]
    [InlineData("Größe & Übersicht: Ärzte", "Groesse-Uebersicht-Aerzte")]
    [InlineData("Café / Señal", "Cafe-Senal")]
    [InlineData("  --Liste__1--  ", "Liste-1")]
    [InlineData("Straße", "Strasse")]
    [InlineData("„Fachverfahren“ (HR)", "Fachverfahren-HR")]
    public void SlugOf_Name_SpellsOutUmlautsDropsAccentsAndJoinsWithHyphens(string name, string expected)
    {
        ExportFileWriter.SlugOf(name).ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("???")]
    [InlineData("名前")]
    public void SlugOf_NameWithoutLettersOrDigits_IsEmpty(string name)
    {
        ExportFileWriter.SlugOf(name).ShouldBeEmpty();
    }

    [Fact]
    public void SlugOf_LongName_IsCutWithoutATrailingHyphen()
    {
        var name = new string('a', 49) + " bcdef";

        var slug = ExportFileWriter.SlugOf(name);

        slug.ShouldBe(new string('a', 49));
        ExportFileWriter.SlugOf(new string('x', 120)).Length.ShouldBe(ExportFileWriter.SlugMaxLength);
    }
}
