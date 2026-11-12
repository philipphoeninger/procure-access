// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Primitives;
using ProcureAccess.Modules.Catalog.Http;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog;

/// <summary>
/// The language of a catalog answer from <c>Accept-Language</c> (API-06, S4-23): German, the only
/// language of every release so far, for everything the release has no texts in; English as soon as
/// a release has it. The HTTP part (headers, ETag, both endpoints) is in the endpoint tests.
/// </summary>
public class CatalogLanguageTests
{
    private static readonly string[] GermanOnly = [CatalogLanguage.German];
    private static readonly string[] GermanAndEnglish = [CatalogLanguage.German, CatalogLanguage.English];

    [Theory(DisplayName = "INV:API-06 " + nameof(Negotiate_ReleaseWithGermanTextsOnly_AnswersGerman))]
    [Trait("Invariant", "API-06")]
    [InlineData("de")]
    [InlineData("en")]
    [InlineData("en-US,en;q=0.9")]
    [InlineData("fr")]
    [InlineData("*")]
    [InlineData("de;q=0")]
    [InlineData("")]
    [InlineData(";;=q")]
    public void Negotiate_ReleaseWithGermanTextsOnly_AnswersGerman(string header)
    {
        CatalogLanguage.Negotiate(header, GermanOnly).ShouldBe("de");
    }

    [Fact]
    public void Negotiate_NoHeader_AnswersGerman()
    {
        CatalogLanguage.Negotiate(StringValues.Empty, GermanAndEnglish).ShouldBe("de");
    }

    [Theory(DisplayName = "INV:API-06 " + nameof(Negotiate_ReleaseWithEnglishTexts_AnswersTheLanguageTheClientPrefers))]
    [Trait("Invariant", "API-06")]
    [InlineData("en", "en")]
    [InlineData("EN-gb", "en")]
    [InlineData("de", "de")]
    [InlineData("de-AT,de;q=0.9", "de")]
    [InlineData("fr, en;q=0.8, de;q=0.5", "en")]
    [InlineData("de;q=0.5, en;q=0.8", "en")]
    [InlineData("en, de", "en")]
    [InlineData("de, en", "de")]
    [InlineData("en;q=0, de;q=0.1", "de")]
    [InlineData("fr", "de")]
    [InlineData("de;q=0, *", "en")]
    [InlineData("en;q=0, *", "de")]
    public void Negotiate_ReleaseWithEnglishTexts_AnswersTheLanguageTheClientPrefers(string header, string expected)
    {
        CatalogLanguage.Negotiate(header, GermanAndEnglish).ShouldBe(expected);
    }

    [Fact]
    public void Negotiate_HeaderInSeveralLines_ReadsEveryLine()
    {
        CatalogLanguage.Negotiate(new StringValues(["fr", "en;q=0.5"]), GermanAndEnglish).ShouldBe("en");
    }
}
