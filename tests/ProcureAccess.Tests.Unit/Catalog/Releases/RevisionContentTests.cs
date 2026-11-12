// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Unit.Catalog.Releases;

/// <summary>
/// CAT-07: the import shares a stored revision exactly when the folder holds the same content
/// (<see cref="RevisionContent.Matches"/>); the order of a list in the file is not content.
/// </summary>
public class RevisionContentTests
{
    private static readonly MiniCatalog Catalog = MiniCatalog.Build(MiniCatalogValues.InMemory());

    [Theory(DisplayName = "INV:CAT-07 " + nameof(Matches_DefinitionOfTheSameRevision_IsTrue))]
    [Trait("Invariant", "CAT-07")]
    [InlineData(Keys.PriorityAllLevels)]
    [InlineData(Keys.DefaultOnly)]
    [InlineData(Keys.ProcedureSoftwareTest)]
    public void Matches_DefinitionOfTheSameRevision_IsTrue(string key)
    {
        var definition = DefinitionOf(key);

        RevisionContent.Matches(definition, MiniCatalog.RevisionOf(Catalog.T1, key)).ShouldBeTrue();
    }

    [Fact(DisplayName = "INV:CAT-07 " + nameof(Matches_ListsInAnotherOrder_IsTrue))]
    [Trait("Invariant", "CAT-07")]
    public void Matches_ListsInAnotherOrder_IsTrue()
    {
        var definition = DefinitionOf(Keys.PriorityAllLevels);
        var reordered = definition with
        {
            Applicability = [.. definition.Applicability.Reverse()],
            Priorities = [.. definition.Priorities.Reverse()],
            ProcedureTexts = [.. definition.ProcedureTexts.Reverse()],
        };

        RevisionContent.Matches(reordered, MiniCatalog.RevisionOf(Catalog.T1, Keys.PriorityAllLevels)).ShouldBeTrue();
    }

    [Fact(DisplayName = "INV:CAT-07 " + nameof(Matches_AnyFieldChanged_IsFalse))]
    [Trait("Invariant", "CAT-07")]
    public void Matches_AnyFieldChanged_IsFalse()
    {
        var revision = MiniCatalog.RevisionOf(Catalog.T1, Keys.PriorityAllLevels);
        var definition = DefinitionOf(Keys.PriorityAllLevels);
        var text = definition.ProcedureTexts[0];
        CriterionDefinition[] changed =
        [
            definition with { TitleDe = definition.TitleDe + " (neu)" },
            definition with { PlainLanguageDe = "Einfach" },
            definition with { ShortDescriptionDe = definition.ShortDescriptionDe + " Geändert." },
            definition with { ShortDescriptionDe = null },
            definition with { RationaleDe = "Warum" },
            definition with { SortOrder = definition.SortOrder + 1 },
            definition with { Applicability = [.. definition.Applicability.Skip(1)] },
            definition with { Priorities = [.. definition.Priorities.Select((p, i) => i == 0 ? p with { Weight = 9 } : p)] },
            definition with { ProcedureTexts = [text with { Content = text.Content with { RequirementDe = "Neu" } }, .. definition.ProcedureTexts.Skip(1)] },
            definition with { StandardReferences = [new StandardReferenceDefinition(Modules.Catalog.Domain.Standard.Wcag, "1.4.3", null)] },
        ];

        changed.ShouldAllBe(d => !RevisionContent.Matches(d, revision));
    }

    private static CriterionDefinition DefinitionOf(string key) =>
        ReleaseDefinitions.Of(Catalog.T1, null).Criteria.Single(c => c.Key == key);
}
