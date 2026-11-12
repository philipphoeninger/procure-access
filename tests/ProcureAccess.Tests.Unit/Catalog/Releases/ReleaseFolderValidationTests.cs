// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Nodes;
using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog.Releases;

/// <summary>
/// Every rule of the release folder validation (S3-15, prepares CAT-23): each test changes one
/// thing in a copy of a test release and expects exactly the error that names the file and the JSON
/// path. Valid input is covered by <see cref="ReleaseFolderReaderTests"/>.
/// </summary>
public class ReleaseFolderValidationTests
{
    private const string T1 = MiniCatalog.T1Version;
    private const string T2 = MiniCatalog.T2Version;
    private static readonly FilterValueKeys Keys = FilterValueKeys.From(MiniCatalogValues.InMemory().All);

    [Fact]
    public void Layout_UnexpectedFiles_AreRefused()
    {
        var result = Validate(T1, files =>
        {
            files["README.md"] = "# Notes";
            files["criteria/old/99.1.json"] = files["criteria/99.1.json"];
        });

        result.Errors.Select(e => e.File).ShouldBe(["README.md", "criteria/old/99.1.json"]);
        result.Errors.ShouldAllBe(e => e.Path == "$" && e.Message.StartsWith("Unexpected file."));
    }

    [Theory]
    [InlineData("release.json")]
    [InlineData("exclusions.json")]
    [InlineData("procedure-mapping.json")]
    public void Layout_MissingFile_IsNamed(string file)
    {
        var result = Validate(T1, files => files.Remove(file));

        ShouldFailWith(result, file, "$", "The file is missing.");
    }

    [Fact]
    public void Layout_NoCriteria_IsRefused()
    {
        var result = Validate(T1, files =>
        {
            foreach (var key in files.Keys.Where(k => k.StartsWith("criteria/", StringComparison.Ordinal)).ToList())
            {
                files.Remove(key);
            }

            Edit(files, "procedure-mapping.json", json => json["procedures"] = new JsonArray());
        });

        ShouldFailWith(result, "criteria/", "$", "The release has no criteria.");
    }

    [Fact]
    public void Json_SyntaxError_NamesPathAndLine()
    {
        var result = Validate(T1, files => files["release.json"] = "{\n  \"version\": \"T.1\",\n  \"releaseNotes\": \n}");

        var error = result.Errors.ShouldHaveSingleItem();
        (error.File, error.Path).ShouldBe(("release.json", "$.releaseNotes"));
        error.Message.ShouldStartWith("Invalid JSON (line 4):");
    }

    [Fact]
    public void Json_UnknownProperty_IsRefused()
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.7.json", json => json["titelDe"] = "Tippfehler"));

        var error = result.Errors.ShouldHaveSingleItem();
        (error.File, error.Path).ShouldBe(("criteria/99.7.json", "$.titelDe"));
        error.Message.ShouldStartWith("Invalid JSON");
    }

    [Fact]
    public void Json_MissingRequiredValues_AreAllReported()
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.7.json", json =>
        {
            json.AsObject().Remove("titleDe");
            json.AsObject().Remove("sortOrder");
            json.AsObject().Remove("kind");
        }));

        result.Errors.Select(e => e.Path).ShouldBe(["$.kind", "$.titleDe", "$.sortOrder"], ignoreOrder: true);
        result.Errors.ShouldAllBe(e => e.File == "criteria/99.7.json" && e.Message == "The value is missing.");
    }

    [Fact]
    public void Errors_OfSeveralFiles_AreCollectedInOneRun()
    {
        var result = Validate(T1, files =>
        {
            Edit(files, "criteria/99.7.json", json => json["kind"] = "requirment");
            Edit(files, "exclusions.json", json => json["exclusions"]![0]!["reasonDe"] = " ");
        });

        result.IsValid.ShouldBeFalse();
        result.Release.ShouldBeNull();
        result.Errors.Select(e => (e.File, e.Path)).ShouldBe(
            [("exclusions.json", "$.exclusions[0].reasonDe"), ("criteria/99.7.json", "$.kind")], ignoreOrder: true);
    }

    [Theory]
    [InlineData("5.2")]
    [InlineData("11.1.3.1c")]
    [InlineData("10.1.3.1a")]
    [InlineData("10.1.4.13")]
    [InlineData("UA1-6.2")]
    [InlineData("UA1-7.10")]
    public void Key_EnNumberOrPdfUaClause_IsAccepted(string key)
    {
        var result = Validate(T1, files => Rename(files, "criteria/99.7.json", $"criteria/{key}.json"));

        result.Errors.ShouldBeEmpty();
        result.Release!.Criteria.ShouldContain(c => c.Key == key);
    }

    [Theory]
    [InlineData("T-REMOVED")]
    [InlineData("BITV-SW-11.1.3.1c")]
    [InlineData("11.1.3.1C")]
    [InlineData("11.1.3.1 c")]
    [InlineData("UA1 6.2")]
    [InlineData("11..1")]
    public void Key_OtherSchemes_AreRefused(string key)
    {
        var result = Validate(T1, files => Rename(files, "criteria/99.7.json", $"criteria/{key}.json"));

        ShouldFailWith(result, $"criteria/{key}.json", "$", "is not a criterion key");
    }

    [Fact]
    public void FilterValues_UnknownKey_IsRefused()
    {
        var result = Validate(T1, files => Edit(
            files, "criteria/99.7.json", json => json["applicability"]![0]!["appTypes"]![0] = "webb"));

        ShouldFailWith(result, "criteria/99.7.json", "$.applicability[0].appTypes[0]", "Unknown AppType 'webb'.");
    }

    [Fact]
    public void FilterValues_KeyOfAnotherLayer_IsRefused()
    {
        var result = Validate(T1, files => Edit(
            files, "exclusions.json", json => json["exclusions"]![0]!["target"]!["layer"] = "productPart"));

        ShouldFailWith(result, "exclusions.json", "$.exclusions[0].target.key", "Unknown ProductPart 'hardware'.");
    }

    [Theory(DisplayName = "INV:CAT-13 " + nameof(Exclusion_TargetLayerNotLower_IsRefused))]
    [Trait("Invariant", "CAT-13")]
    [InlineData("appType", "web", "appType", "mobileOpen")]
    [InlineData("productPart", "audio", "appType", "web")]
    [InlineData("productPart", "audio", "productPart", "video")]
    [InlineData("useCase", "hr", "useCase", "databaseApplication")]
    public void Exclusion_TargetLayerNotLower_IsRefused(string sourceLayer, string sourceKey, string targetLayer, string targetKey)
    {
        var result = Validate(T1, files => Edit(files, "exclusions.json", json => json["exclusions"]!.AsArray().Add(new JsonObject
        {
            ["source"] = new JsonObject { ["layer"] = sourceLayer, ["key"] = sourceKey },
            ["target"] = new JsonObject { ["layer"] = targetLayer, ["key"] = targetKey },
            ["reasonDe"] = "Testgrund.",
        })));

        ShouldFailWith(result, "exclusions.json", "$.exclusions[6].target.layer", "An exclusion must point to a lower layer");
    }

    [Fact]
    public void Exclusion_FromATestType_IsRefused()
    {
        var result = Validate(T1, files => Edit(files, "exclusions.json", json => json["exclusions"]![0]!["source"] =
            new JsonObject { ["layer"] = "testType", ["key"] = "appTest" }));

        ShouldFailWith(result, "exclusions.json", "$.exclusions[0].source.layer", "A test type is the lowest layer");
    }

    [Fact]
    public void Exclusion_ListedTwice_IsRefused()
    {
        var result = Validate(T1, files => Edit(
            files, "exclusions.json", json => json["exclusions"]!.AsArray().Add(json["exclusions"]![0]!.DeepClone())));

        ShouldFailWith(result, "exclusions.json", "$.exclusions[6]", "is listed twice");
    }

    [Fact]
    public void ProcedureMapping_AppTypeListedTwice_IsRefused()
    {
        var result = Validate(T1, files => Edit(files, "procedure-mapping.json", json => json["procedures"]!.AsArray().Add(
            new JsonObject { ["appType"] = "web", ["testTypes"] = new JsonArray("appTest") })));

        ShouldFailWith(result, "procedure-mapping.json", "$.procedures[3].appType", "has more than one entry");
    }

    [Fact]
    public void ProcedureMapping_UnknownTestType_IsRefused()
    {
        var result = Validate(T1, files => Edit(
            files, "procedure-mapping.json", json => json["procedures"]![0]!["testTypes"]![0] = "softwaretest"));

        ShouldFailWith(result, "procedure-mapping.json", "$.procedures[0].testTypes[0]", "Unknown TestType 'softwaretest'.");
    }

    [Fact]
    public void ProcedureText_WhoseTestTypeNoAppTypeOfTheCriterionUses_IsRefused()
    {
        // 99.4 applies to Web and Software (offen); only Mobile (offen) is mapped to the App-Test.
        var result = Validate(T1, files => Edit(files, "criteria/99.4.json", json =>
        {
            var text = json["procedureTexts"]![0]!.DeepClone();
            text["testType"] = "appTest";
            json["procedureTexts"]!.AsArray().Add(text);
        }));

        ShouldFailWith(result, "criteria/99.4.json", "$.procedureTexts[1].testType", "No app type this criterion applies to is mapped");
    }

    [Fact]
    public void ProcedureText_WhenTheMappingFileIsInvalid_IsNotReportedAsWell()
    {
        // Without a valid mapping the texts cannot be checked against it; one error, not one per text.
        var result = Validate(T1, files => Edit(
            files, "procedure-mapping.json", json => json["procedures"]![0]!["appType"] = "webb"));

        ShouldFailWith(result, "procedure-mapping.json", "$.procedures[0].appType", "Unknown AppType 'webb'.");
    }

    [Fact]
    public void ProcedureText_WhenAnAppTypeOfTheCriterionIsUnknown_IsNotReportedAsWell()
    {
        // 99.4's Software-Test text is mapped through Web and Software (offen); a typo in one of them
        // is one error, not a second one about the text.
        var result = Validate(T1, files => Edit(files, "criteria/99.4.json", json =>
        {
            json["applicability"]![0]!["appTypes"]![0] = "webb";
            json["applicability"]![0]!["appTypes"]![1] = "softwareOpenn";
        }));

        result.Errors.Select(e => e.Path).ShouldBe(["$.applicability[0].appTypes[0]", "$.applicability[0].appTypes[1]"]);
    }

    [Fact]
    public void ProcedureText_TwiceForOneTestType_IsRefused()
    {
        var result = Validate(T1, files => Edit(
            files, "criteria/99.4.json", json => json["procedureTexts"]!.AsArray().Add(json["procedureTexts"]![0]!.DeepClone())));

        ShouldFailWith(result, "criteria/99.4.json", "$.procedureTexts[1].testType", "There is already a text for test type 'softwareTest'.");
    }

    [Fact]
    public void ProcedureText_RequiredValuesAndLimits_AreChecked()
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.4.json", json =>
        {
            var text = json["procedureTexts"]![0]!;
            text["originalNumber"] = new string('1', 33);
            text.AsObject().Remove("originalTitleDe");
            text["sourceUrl"] = "softwaretest.bit-inklusiv.de/#schritt";
        }));

        result.Errors.Select(e => e.Path).ShouldBe(
            ["$.procedureTexts[0].originalNumber", "$.procedureTexts[0].originalTitleDe", "$.procedureTexts[0].sourceUrl"]);
        result.Errors[0].Message.ShouldBe("The text has 33 characters; at most 32 are allowed.");
        result.Errors[2].Message.ShouldContain("is not an absolute http(s) URL");
    }

    [Theory]
    [InlineData("[]", "$.procedureTexts[0].allowedRatings", "Name at least one allowed rating")]
    [InlineData("[\"fulfilled\", \"erfüllt\"]", "$.procedureTexts[0].allowedRatings[1]", "Unknown rating 'erfüllt'")]
    [InlineData("[\"fulfilled\", \"unknown\"]", "$.procedureTexts[0].allowedRatings[1]", "Unknown rating 'unknown'")]
    [InlineData("[\"blockade\", \"blockade\"]", "$.procedureTexts[0].allowedRatings[1]", "is listed twice")]
    public void AllowedRatings_EmptyUnknownOrRepeated_AreRefused(string ratings, string path, string message)
    {
        var result = Validate(T1, files => Edit(
            files, "criteria/99.4.json", json => json["procedureTexts"]![0]!["allowedRatings"] = JsonNode.Parse(ratings)));

        ShouldFailWith(result, "criteria/99.4.json", path, message);
    }

    [Fact]
    public void ProcedureCriterion_WithApplicabilityOrTexts_IsRefused()
    {
        var result = Validate(T1, files => Edit(files, "criteria/PROC-appTest.json", json =>
        {
            json["applicability"] = new JsonArray(new JsonObject { ["appTypes"] = new JsonArray("mobileOpen") });
            json["procedureTexts"] = JsonNode.Parse(files["criteria/99.4.json"])!["procedureTexts"]!.DeepClone();
        }));

        result.Errors.Select(e => e.Path).ShouldBe(["$.applicability", "$.procedureTexts"]);
    }

    [Fact]
    public void ProcedureCriterion_NamedAfterAnotherTestType_IsRefused()
    {
        var result = Validate(T1, files => Rename(files, "criteria/PROC-appTest.json", "criteria/PROC-pdfTest.json"));

        ShouldFailWith(result, "criteria/PROC-pdfTest.json", "$", "must be named PROC-appTest.json");
    }

    [Fact]
    public void ProcedureCriterion_WithoutTestType_IsRefused()
    {
        var result = Validate(T1, files => Edit(files, "criteria/PROC-appTest.json", json => json.AsObject().Remove("testType")));

        ShouldFailWith(result, "criteria/PROC-appTest.json", "$.testType", "The value is missing.");
    }

    [Fact]
    public void Requirement_WithTestTypeOrProcedureKey_IsRefused()
    {
        var result = Validate(T1, files =>
        {
            Edit(files, "criteria/99.7.json", json => json["testType"] = "softwareTest");
            Edit(files, "criteria/PROC-appTest.json", json =>
            {
                json["kind"] = "requirement";
                json.AsObject().Remove("testType");
                json["applicability"] = new JsonArray(new JsonObject { ["appTypes"] = new JsonArray("mobileOpen") });
            });
        });

        result.Errors.Select(e => (e.File, e.Path)).ShouldBe(
            [("criteria/99.7.json", "$.testType"), ("criteria/PROC-appTest.json", "$.kind")], ignoreOrder: true);
    }

    [Fact]
    public void Requirement_WithoutApplicability_IsRefused()
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.7.json", json => json["applicability"] = new JsonArray()));

        ShouldFailWith(result, "criteria/99.7.json", "$.applicability", "needs at least one app type");
    }

    [Fact]
    public void Applicability_PairListedTwice_IsRefused()
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.4.json", json => json["applicability"]!.AsArray().Add(
            new JsonObject { ["appTypes"] = new JsonArray("web"), ["productParts"] = new JsonArray("audio", "video") })));

        ShouldFailWith(result, "criteria/99.4.json", "$.applicability[1]", "App type 'web' with product part 'audio' is listed twice.");
    }

    [Fact]
    public void Supersedes_ASplit_IsRefused()
    {
        // 99.9 replaces 99.8 too; criteria are read in key order, so 99.11 comes first. One old key
        // has one successor (S3-14 P2).
        var result = Validate(T2, files => Edit(files, "criteria/99.11.json", json => json["supersedes"] = new JsonArray("99.8")));

        ShouldFailWith(result, "criteria/99.9.json", "$.supersedes[0]", "'99.8' is already replaced by '99.11'");
    }

    [Fact]
    public void Supersedes_ACriterionOfTheSameRelease_IsRefused()
    {
        var result = Validate(T2, files => Edit(files, "criteria/99.11.json", json => json["supersedes"] = new JsonArray("99.1")));

        ShouldFailWith(result, "criteria/99.11.json", "$.supersedes[0]", "'99.1' is a criterion of this release");
    }

    [Fact]
    public void Supersedes_NotAKey_IsRefused()
    {
        var result = Validate(T2, files => Edit(files, "criteria/99.11.json", json => json["supersedes"] = new JsonArray("T-OLD")));

        ShouldFailWith(result, "criteria/99.11.json", "$.supersedes[0]", "'T-OLD' is not a criterion key.");
    }

    [Fact]
    public void Priorities_Missing_AreAccepted()
    {
        // O-S3-5 Q2 (iii): a release without priorities can be imported; activate refuses it (S3-17).
        var result = Validate(T1, files =>
        {
            foreach (var file in files.Keys.Where(k => k.StartsWith("criteria/", StringComparison.Ordinal)).ToList())
            {
                Edit(files, file, json => json.AsObject().Remove("priorities"));
            }
        });

        result.Errors.ShouldBeEmpty();
        result.Release!.Criteria.ShouldAllBe(c => c.Priorities.Count == 0);
    }

    [Fact]
    public void Priorities_TwoRulesForOnePair_AreRefused()
    {
        var result = Validate(T1, files => Edit(
            files, "criteria/99.7.json", json => json["priorities"]!.AsArray().Add(json["priorities"]![0]!.DeepClone())));

        ShouldFailWith(result, "criteria/99.7.json", "$.priorities[1]", "There is already a rule for app type '(any)' and use case '(any)'.");
    }

    [Fact]
    public void Priorities_UnknownLevelAndMissingRationale_AreRefused()
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.7.json", json =>
        {
            json["priorities"]![0]!["level"] = "C";
            json["priorities"]![0]!.AsObject().Remove("rationaleDe");
        }));

        result.Errors.Select(e => e.Path).ShouldBe(["$.priorities[0].level", "$.priorities[0].rationaleDe"], ignoreOrder: true);
    }

    [Fact]
    public void Lines_AreJoinedWithLineBreaks()
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.4.json", json =>
            json["procedureTexts"]![0]!["requirementDe"] = new JsonArray("Absatz eins.", "", "- Punkt 1", "- Punkt 2")));

        result.Errors.ShouldBeEmpty();
        result.Release!.Criteria.Single(c => c.Key == "99.4").ProcedureTexts[0].Content.RequirementDe
            .ShouldBe("Absatz eins.\n\n- Punkt 1\n- Punkt 2");
    }

    [Theory]
    [InlineData("[]", "$.rationaleDe", "has no lines")]
    [InlineData("[\"\", \"Text\"]", "$.rationaleDe", "starts or ends with an empty line")]
    [InlineData("[\"Zeile eins\\nZeile zwei\"]", "$.rationaleDe[0]", "one line of text")]
    [InlineData("[\"Text\", null]", "$.rationaleDe[1]", "one line of text")]
    public void Lines_EmptyOrWithLineBreaks_AreRefused(string lines, string path, string message)
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.7.json", json => json["rationaleDe"] = JsonNode.Parse(lines)));

        ShouldFailWith(result, "criteria/99.7.json", path, message);
    }

    [Fact]
    public void Texts_WithSurroundingWhiteSpaceOrOverTheLimit_AreRefused()
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.7.json", json =>
        {
            json["titleDe"] = "Titel ";
            json["category"] = new string('K', 129);
        }));

        result.Errors.Select(e => e.Path).ShouldBe(["$.category", "$.titleDe"], ignoreOrder: true);
    }

    [Theory(DisplayName = "INV:CAT-23 " + nameof(ShortDescription_EmptyOrWithSurroundingWhiteSpace_IsRefused))]
    [Trait("Invariant", "CAT-23")]
    [InlineData("", "The text is empty; leave the property out instead.")]
    [InlineData("   ", "The text is empty; leave the property out instead.")]
    [InlineData(" Kurz.", "The text starts or ends with white space.")]
    public void ShortDescription_EmptyOrWithSurroundingWhiteSpace_IsRefused(string value, string message)
    {
        var result = Validate(T1, files => Edit(files, "criteria/99.7.json", json => json["shortDescriptionDe"] = value));

        ShouldFailWith(result, "criteria/99.7.json", "$.shortDescriptionDe", message);
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(ShortDescription_Given_IsReadAsWritten))]
    [Trait("Invariant", "CAT-23")]
    public void ShortDescription_Given_IsReadAsWritten()
    {
        var result = Validate(T1, files => Edit(
            files, "criteria/99.7.json", json => json["shortDescriptionDe"] = "Ein Satz. Noch ein Satz."));

        result.Errors.ShouldBeEmpty();
        var criterion = result.Release!.Criteria.Single(c => c.Key == "99.7");
        criterion.ShortDescriptionDe.ShouldBe("Ein Satz. Noch ein Satz.");
    }

    [Fact]
    public void StandardReference_UnknownStandard_IsRefused()
    {
        var result = Validate(T1, files => Edit(
            files, "criteria/99.3.json", json => json["standardReferences"]![0]!["standard"] = "EN 301 549"));

        ShouldFailWith(result, "criteria/99.3.json", "$.standardReferences[0].standard", "Unknown standard 'EN 301 549'");
    }

    [Fact]
    public void StandardReference_SameClauseWithAnotherUrl_IsRefused()
    {
        // The database keeps one row per standard and clause; 99.3 and 99.6 cite the same one.
        var result = Validate(T1, files => Edit(
            files, "criteria/99.6.json", json => json["standardReferences"]![0]!["url"] = "https://example.org/other"));

        ShouldFailWith(result, "criteria/99.6.json", "$.standardReferences[0]", "is cited with another URL in criteria/99.3.json");
    }

    [Fact]
    public void Url_WithSurroundingWhiteSpace_IsRefused()
    {
        var result = Validate(T1, files => Edit(
            files, "criteria/99.4.json", json => json["procedureTexts"]![0]!["sourceUrl"] = "https://example.org/x "));

        ShouldFailWith(result, "criteria/99.4.json", "$.procedureTexts[0].sourceUrl", "is not an absolute http(s) URL");
    }

    private static ReleaseFolderResult Validate(string version, Action<Dictionary<string, string>> change)
    {
        var files = TestReleaseFolders.FilesOf(version);
        change(files);
        return ReleaseFolderReader.Read(version, files, Keys);
    }

    private static void Edit(Dictionary<string, string> files, string file, Action<JsonNode> edit)
    {
        var json = JsonNode.Parse(files[file])!;
        edit(json);
        files[file] = json.ToJsonString();
    }

    private static void Rename(Dictionary<string, string> files, string from, string to)
    {
        files[to] = files[from];
        files.Remove(from);
    }

    private static void ShouldFailWith(ReleaseFolderResult result, string file, string path, string message)
    {
        result.IsValid.ShouldBeFalse();
        var error = result.Errors.ShouldHaveSingleItem();
        (error.File, error.Path).ShouldBe((file, path));
        error.Message.ShouldContain(message);
    }
}
