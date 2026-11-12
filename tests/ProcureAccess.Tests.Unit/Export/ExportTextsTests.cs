// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Export.Texts;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Export;

/// <summary>
/// The texts of the export files (owner decisions O-S4-62, O-S4-63 (a)): the embedded
/// <c>Texts/de.json</c> holds every key the renderers read, in the format of the client's
/// <c>de.json</c>. Whether its texts match the UX copy is checked by <c>scripts/check-i18n.mjs</c>.
/// </summary>
public class ExportTextsTests
{
    [Fact]
    public void LoadGerman_EmbeddedTexts_HoldEveryKeyTheRenderersRead()
    {
        var texts = ExportTexts.LoadGerman();

        foreach (var key in ExportTextKeys.Required)
        {
            texts[key].ShouldNotBeNullOrWhiteSpace(key);
        }

        texts[ExportTextKeys.ColumnIndex].ShouldBe("Nr.");
        texts.Level(PriorityLevel.A).ShouldBe("A – Mindestanforderung");
    }

    [Fact]
    public void FromJson_PropertyNameWithDots_IsReadAsPartOfTheKey()
    {
        var texts = ExportTexts.FromJson(Json(WithEveryRequiredKey("""{ "x": { "menu": "Menü", "menu.close": "Schließen" } }""")));

        texts["x.menu"].ShouldBe("Menü");
        texts["x.menu.close"].ShouldBe("Schließen");
    }

    [Fact]
    public void FromJson_RequiredKeyMissing_RefusesWithTheKey()
    {
        var error = Should.Throw<InvalidOperationException>(() => ExportTexts.FromJson(Json("""{ "x": "y" }""")));

        error.Message.ShouldContain(ExportTextKeys.ColumnRelease);
        error.Message.ShouldContain("check-i18n.mjs --write");
    }

    [Fact]
    public void Indexer_UnknownKey_Throws() =>
        Should.Throw<KeyNotFoundException>(() => ExportTexts.LoadGerman()["exportFile.unknown"]);

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    // Every required key as a flat property name with dots, plus the object of the test.
    private static string WithEveryRequiredKey(string json)
    {
        var required = string.Join(", ", ExportTextKeys.Required.Select(key => $"\"{key}\": \"{key}\""));
        return json.TrimEnd().TrimEnd('}') + ", " + required + " }";
    }
}
