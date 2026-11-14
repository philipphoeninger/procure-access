// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using ProcureAccess.Modules.Platform.Email;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Email;

/// <summary>The embedded e-mail texts (S5-11): written by <c>check-i18n.mjs</c>, and the start fails without a key.</summary>
public class EmailTextsTests
{
    [Fact]
    public void LoadGerman_EmbeddedTexts_HoldEveryRequiredKey()
    {
        var texts = EmailTexts.LoadGerman();

        EmailTexts.Required.ShouldAllBe(key => !string.IsNullOrWhiteSpace(texts[key]));
        texts["org.members.role.owner"].ShouldBe("Verwalten");
    }

    [Fact]
    public void FromJson_RequiredKeyMissing_StopsTheStart()
    {
        using var document = JsonDocument.Parse("""{ "email": { "footer": "Fußzeile" } }""");

        Should.Throw<InvalidOperationException>(() => EmailTexts.FromJson(document.RootElement))
            .Message.ShouldContain("email.confirm.subject");
    }
}
