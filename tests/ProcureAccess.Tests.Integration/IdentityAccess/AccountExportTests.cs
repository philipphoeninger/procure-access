// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using ProcureAccess.Modules.IdentityAccess.Accounts;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Tests.Integration.Configurator;
using ProcureAccess.Tests.Integration.Export;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// The personal data export <c>GET /api/v1/account/export</c> (S5-27; SPEC R-AUTH-09, §8.4;
/// ARCHITECTURE C79; PRV-07, account part). Every test starts from the Tenants fixture with the
/// legacy accounts of its users stored. Editor A is also a Reader of organisation B and has a
/// contributor profile; Owner B has one too, which Editor A's export must not show. The criteria
/// lists the user created came with S5-19 (owner decision S5-19 4), the settings with S5-25a (the
/// effective values, owner decision S5-25 4), the procurement projects with S6-25 (owner decisions
/// S6-25 D1 (a), D2 (a)), the stored assessments of their candidates with S7-05 (owner decision
/// O-S7-1). The part of PRV-07 that step 6 fills (contributions and purged content)
/// is empty here (owner decision S5-27 1a).
/// </summary>
public sealed class AccountExportTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Export = "/api/v1/account/export";

    private Tenants? _tenants;
    private ContributorProfile? _ownProfile;
    private ContributorProfile? _otherProfile;
    private DateTimeOffset _createdAt;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        var now = Clock.GetUtcNow();
        _createdAt = now.AddDays(-30);
        _tenants = Tenants.Build(now);
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => AccountOf(u, u == TestUsers.EditorA ? _createdAt : null))]);

        // Editor A joined organisation B a day later as a Reader: two memberships, both shared with other users.
        var secondMembership = OrganizationMembership.Create(
            _tenants.B.Id, TestUsers.EditorA.Id, OrganizationRole.Reader, now, now.AddDays(1));
        _ownProfile = ContributorProfile.Create("Erfahrungen Anna", ContributorKind.Individual, now.AddHours(-2));
        _otherProfile = ContributorProfile.Create("Prüfstelle Nord", ContributorKind.Auditor, now.AddHours(-1));

        await StoreAsync([.. _tenants.Entities, secondMembership]);
        await StoreAsync(
            _ownProfile, UserContributorLink.Create(TestUsers.EditorA.Id, _ownProfile),
            _otherProfile, UserContributorLink.Create(TestUsers.OwnerB.Id, _otherProfile));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:PRV-07 " + nameof(Export_UserWithMembershipsAndProfile_ContainsTheUsersOwnDataOnly))]
    [Trait("Invariant", "PRV-07")]
    public async Task Export_UserWithMembershipsAndProfile_ContainsTheUsersOwnDataOnly()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        using var body = JsonDocument.Parse(text);
        var root = body.RootElement;

        var account = root.GetProperty("account");
        account.GetProperty("email").GetString().ShouldBe(TestUsers.EditorA.Email);
        account.GetProperty("createdAt").GetDateTimeOffset().ShouldBe(_createdAt);
        account.GetProperty("emailConfirmed").GetBoolean().ShouldBeTrue();

        var now = Clock.GetUtcNow();
        root.GetProperty("memberships").EnumerateArray()
            .Select(m => (m.GetProperty("organization").GetString(), m.GetProperty("role").GetString(), m.GetProperty("since").GetDateTimeOffset()))
            .ShouldBe([("Organisation A", "editor", now), ("Organisation B", "reader", now.AddDays(1))]);
        root.GetProperty("contributorProfiles").EnumerateArray()
            .Select(p => (p.GetProperty("displayName").GetString(), p.GetProperty("kind").GetString(), p.GetProperty("createdAt").GetDateTimeOffset()))
            .ShouldBe([("Erfahrungen Anna", "individual", _ownProfile!.CreatedAt)]);

        // Nothing of another user, and no id at all — neither the user's own nor an organisation's or a profile's.
        foreach (var other in TestUsers.ByName.Values.Where(u => u != TestUsers.EditorA))
        {
            text.ShouldNotContain(other.Email, Case.Insensitive);
            text.ShouldNotContain(other.Id, Case.Insensitive);
        }

        text.ShouldNotContain(TestUsers.EditorA.Id, Case.Insensitive);
        text.ShouldNotContain(Fixture.A.Id.ToString(), Case.Insensitive);
        text.ShouldNotContain(Fixture.B.Id.ToString(), Case.Insensitive);
        text.ShouldNotContain(_ownProfile.Id.ToString(), Case.Insensitive);
        text.ShouldNotContain(_otherProfile!.Id.ToString(), Case.Insensitive);
        text.ShouldNotContain("Prüfstelle Nord");
    }

    [Fact(DisplayName = "INV:PRV-07 " + nameof(Export_UserWhoCreatedLists_ContainsTheirContentAndNoListOfOthersOrOfALeftOrganization))]
    [Trait("Invariant", "PRV-07")]
    public async Task Export_UserWhoCreatedLists_ContainsTheirContentAndNoListOfOthersOrOfALeftOrganization()
    {
        await ExportTestCatalog.ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        var selection = await SavedLists.ResolveAsync(Api.Factory.Services, MiniCatalog.T1Version, Cancellation);
        var now = Clock.GetUtcNow();
        var (excluded, changed) = (selection.Criteria[0], selection.Criteria[1]);
        var changedTo = changed.RecommendedPriority.Level == PriorityLevel.B ? PriorityLevel.A : PriorityLevel.B;

        // Editor A created two lists: one in A with decisions, a later one in B (where Editor A only reads by now).
        var own = SavedLists.Build(Fixture.A.Id, TestUsers.EditorA.Id, "Vergabe 2027", now.AddHours(-3), MiniCatalog.T1Version, selection, c =>
            c == excluded ? SavedLists.Draft(c, isIncluded: false)
            : c == changed ? SavedLists.Draft(c, overrideLevel: changedTo, overrideWeight: changedTo == PriorityLevel.B ? 8 : null, note: "Mit Anbieter klären")
            : null);
        var ownInB = SavedLists.Build(Fixture.B.Id, TestUsers.EditorA.Id, "Rahmenvertrag", now.AddHours(-1), MiniCatalog.T1Version, selection);
        // Not Editor A's: a list of a colleague in A, and one Editor A created in an organisation they have left.
        var colleagues = SavedLists.Build(Fixture.A.Id, TestUsers.OwnerA.Id, "Liste der Kollegin", now, MiniCatalog.T1Version, selection);
        var left = Organization.Create("Verlassene Organisation", OrganizationType.Association, TestUsers.OwnerB.Id, now);
        var inLeft = SavedLists.Build(left.Id, TestUsers.EditorA.Id, "Liste von früher", now, MiniCatalog.T1Version, selection);
        await StoreAsync(left, own, ownInB, colleagues, inLeft);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        using var body = JsonDocument.Parse(text);
        var lists = body.RootElement.GetProperty("createdByMe").GetProperty("criteriaLists").EnumerateArray().ToList();
        lists.Select(l => (l.GetProperty("name").GetString(), l.GetProperty("organization").GetString(), l.GetProperty("release").GetString()))
             .ShouldBe([("Vergabe 2027", "Organisation A", "T.1"), ("Rahmenvertrag", "Organisation B", "T.1")]);

        var first = lists[0];
        first.EnumerateObject().Select(p => p.Name).ShouldBe(["name", "organization", "release", "createdAt", "selection", "criteria"]);
        first.GetProperty("createdAt").GetDateTimeOffset().ShouldBe(now.AddHours(-3));
        var chosen = first.GetProperty("selection");
        chosen.EnumerateObject().Select(p => p.Name).ShouldBe(["useCase", "appTypes", "productParts", "testTypes"]);
        chosen.GetProperty("useCase").GetString().ShouldBe(selection.UseCase!.Name);
        chosen.GetProperty("appTypes").EnumerateArray().Select(v => v.GetString()).ShouldBe(selection.AppTypes.Select(v => v.Name));
        chosen.GetProperty("productParts").EnumerateArray().Select(v => v.GetString()).ShouldBe(selection.ProductParts.Select(v => v.Name));
        chosen.GetProperty("testTypes").EnumerateArray().Select(v => v.GetString()).ShouldBe(selection.TestTypes.Select(v => v.Name));

        // Every criterion of the selection in list order, with the user's decision next to the recommendation.
        var criteria = first.GetProperty("criteria").EnumerateArray().ToList();
        criteria[0].EnumerateObject().Select(p => p.Name).ShouldBe(
            ["number", "title", "isIncluded", "recommendedLevel", "effectiveLevel", "weight", "note"]);
        criteria.Select(c => c.GetProperty("title").GetString()).ShouldBe(selection.Criteria.Select(c => c.PlainLanguageTitle ?? c.Title));
        criteria.Select(c => c.GetProperty("number").GetString()).ShouldBe(selection.Criteria.Select(c => c.Number));
        criteria.Select(c => c.GetProperty("recommendedLevel").GetString())
                .ShouldBe(selection.Criteria.Select(c => c.RecommendedPriority.Level.ToString()));
        criteria.Select(c => c.GetProperty("isIncluded").GetBoolean()).ShouldBe(selection.Criteria.Select(c => c != excluded));
        criteria[1].GetProperty("effectiveLevel").GetString().ShouldBe(changedTo.ToString());
        criteria[1].GetProperty("note").GetString().ShouldBe("Mit Anbieter klären");
        if (changedTo == PriorityLevel.B)
        {
            criteria[1].GetProperty("weight").GetInt32().ShouldBe(8);
        }
        else
        {
            criteria[1].GetProperty("weight").ValueKind.ShouldBe(JsonValueKind.Null);
        }

        criteria[2].GetProperty("effectiveLevel").GetString().ShouldBe(selection.Criteria[2].RecommendedPriority.Level.ToString());
        criteria[2].GetProperty("note").ValueKind.ShouldBe(JsonValueKind.Null);

        // Nothing of the other lists, and no id: no list, organisation or user id, no revision id member.
        text.ShouldNotContain("Liste der Kollegin");
        text.ShouldNotContain("Liste von früher");
        text.ShouldNotContain("Verlassene Organisation");
        foreach (var id in new[] { own.Id, ownInB.Id, colleagues.Id, inLeft.Id, Fixture.A.Id, Fixture.B.Id, left.Id })
        {
            text.ShouldNotContain(id.ToString(), Case.Insensitive);
        }

        text.ShouldNotContain(TestUsers.EditorA.Id, Case.Insensitive);
        text.ShouldNotContain(TestUsers.OwnerA.Id, Case.Insensitive);
        text.ShouldNotContain("revisionId", Case.Insensitive);
    }

    [Fact(DisplayName = "INV:PRV-07 " + nameof(Export_UserWhoCreatedProjects_ContainsTheirContentAndNoProjectOfOthersOrOfALeftOrganization))]
    [Trait("Invariant", "PRV-07")]
    public async Task Export_UserWhoCreatedProjects_ContainsTheirContentAndNoProjectOfOthersOrOfALeftOrganization()
    {
        await ExportTestCatalog.ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        var selection = await SavedLists.ResolveAsync(Api.Factory.Services, MiniCatalog.T1Version, Cancellation);
        var requirements = selection.Criteria.Where(c => c.Kind == CriterionKind.Requirement).ToList();
        requirements.Count.ShouldBeGreaterThan(3);
        selection.Criteria.ShouldContain(c => c.Kind == CriterionKind.TestProcedure);
        var now = Clock.GetUtcNow();
        var testDate = DateOnly.FromDateTime(now.UtcDateTime).AddDays(-30);

        // Editor A created a project in A: two candidates, the first one tested with two
        // evidences and every private field, the second with nothing but its name. Owner A
        // assigned the criteria list, so the copy is Owner A's and only named here (D2 (a)).
        var own = ProcurementProject.Create(Fixture.A.Id, "Lernplattform 2027", TestUsers.EditorA.Id, now.AddHours(-3));
        var full = CandidateProduct.Create(
            own, new CandidateFacts("Lernwelt", "Lernwelt GmbH", "https://lernwelt.example", "Kurse, Tests"), now.AddHours(-2));
        full.ChangeTestStatus(TestStatus.Tested, removeEvidences: false, now.AddHours(-2));
        var procedure = full.AddEvidence(
            new TestEvidenceFacts(TestKind.Procedure, "softwareTest", null, "Prüfstelle Süd", testDate, "4.2", "https://lernwelt.example/bericht"),
            now.AddHours(-2)).Evidence!;
        var other = full.AddEvidence(
            new TestEvidenceFacts(TestKind.Other, null, "Hausinterne Prüfung", "IT-Referat", testDate.AddDays(1), null, null),
            now.AddHours(-1)).Evidence!;
        var bare = CandidateProduct.Create(own, new CandidateFacts("Kursraum", null, null, null), now.AddHours(-1));
        // The list leaves out its third requirement criterion, which was assessed before (WSP-03).
        var copy = SavedLists.Build(
                Fixture.A.Id, TestUsers.OwnerA.Id, "Vergabe 2027", now, MiniCatalog.T1Version, selection,
                c => c == requirements[2] ? SavedLists.Draft(c, isIncluded: false) : null)
            .CopyForProject(own.Id, TestUsers.OwnerA.Id, now);

        // The stored assessments of the first candidate, entered against the list order: a value
        // with a note, a note without a value, a value of the criterion the list excludes by now.
        // The fourth criterion and the second candidate have no row.
        var excludedValue = Assessment(full, requirements[2], Fulfillment.Fulfilled, null, now);
        var valueWithNote = Assessment(full, requirements[1], Fulfillment.Restriction, "Laut Prüfbericht, Abschnitt 3", now);
        var noteOnly = Assessment(full, requirements[0], Fulfillment.Unknown, "Beim Anbieter nachfragen", now);

        // A later one in B, archived, without candidates and without a list.
        var ownInB = ProcurementProject.Create(Fixture.B.Id, "Rahmenvertrag", TestUsers.EditorA.Id, now.AddHours(-1));
        ownInB.ChangeStatus(ProjectStatus.Archived, now);

        // Not Editor A's: a project of a colleague in A, and one Editor A created in an organisation they have left.
        var colleagues = ProcurementProject.Create(Fixture.A.Id, "Projekt der Kollegin", TestUsers.OwnerA.Id, now);
        var colleaguesCandidate = CandidateProduct.Create(colleagues, new CandidateFacts("Fremdes Produkt", null, null, null), now);
        var colleaguesCopy = SavedLists.Build(
                Fixture.A.Id, TestUsers.OwnerA.Id, "Liste der Kollegin", now, MiniCatalog.T1Version, selection)
            .CopyForProject(colleagues.Id, TestUsers.OwnerA.Id, now);
        var colleaguesAssessment = Assessment(colleaguesCandidate, requirements[0], Fulfillment.Barrier, "Einschätzung der Kollegin", now);
        var left = Organization.Create("Verlassene Organisation", OrganizationType.Association, TestUsers.OwnerB.Id, now);
        var inLeft = ProcurementProject.Create(left.Id, "Projekt von früher", TestUsers.EditorA.Id, now);
        await StoreAsync(
            left, own, full, bare, copy, ownInB, colleagues, colleaguesCandidate, colleaguesCopy, inLeft,
            excludedValue, valueWithNote, noteOnly, colleaguesAssessment,
            CandidatePrivateSection.Of(full, "Frau Beispiel, Vertrieb", "Angebot bis März"),
            TestEvidencePrivateNote.Of(procedure, "Zertifizierte Prüferin"),
            TestEvidencePrivateNote.Of(other, null),
            CandidatePrivateSection.Of(colleaguesCandidate, "Kontakt der Kollegin", null));
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        using var body = JsonDocument.Parse(text);
        var createdByMe = body.RootElement.GetProperty("createdByMe");
        var projects = createdByMe.GetProperty("projects").EnumerateArray().ToList();
        projects.Select(p => (p.GetProperty("name").GetString(), p.GetProperty("organization").GetString(), p.GetProperty("status").GetString()))
                .ShouldBe([("Lernplattform 2027", "Organisation A", "open"), ("Rahmenvertrag", "Organisation B", "archived")]);

        var first = projects[0];
        first.EnumerateObject().Select(p => p.Name).ShouldBe(["name", "organization", "status", "createdAt", "criteriaList", "candidates"]);
        first.GetProperty("createdAt").GetDateTimeOffset().ShouldBe(now.AddHours(-3));
        var list = first.GetProperty("criteriaList");
        list.EnumerateObject().Select(p => p.Name).ShouldBe(["name", "release"]);
        list.GetProperty("name").GetString().ShouldBe("Vergabe 2027");
        list.GetProperty("release").GetString().ShouldBe(MiniCatalog.T1Version);
        // The copy itself is in the export of Owner A, who assigned it.
        createdByMe.GetProperty("criteriaLists").GetArrayLength().ShouldBe(0);

        // The candidates in the order they were added, the private section next to the facts.
        var candidates = first.GetProperty("candidates").EnumerateArray().ToList();
        candidates.Select(c => c.GetProperty("name").GetString()).ShouldBe(["Lernwelt", "Kursraum"]);
        var candidate = candidates[0];
        candidate.EnumerateObject().Select(p => p.Name).ShouldBe(
            ["name", "vendor", "website", "functionalSummary", "testStatus", "contactNotes", "internalNotes", "createdAt", "testEvidences", "assessments"]);
        candidate.GetProperty("vendor").GetString().ShouldBe("Lernwelt GmbH");
        candidate.GetProperty("website").GetString().ShouldBe("https://lernwelt.example");
        candidate.GetProperty("functionalSummary").GetString().ShouldBe("Kurse, Tests");
        candidate.GetProperty("testStatus").GetString().ShouldBe("tested");
        candidate.GetProperty("contactNotes").GetString().ShouldBe("Frau Beispiel, Vertrieb");
        candidate.GetProperty("internalNotes").GetString().ShouldBe("Angebot bis März");
        candidate.GetProperty("createdAt").GetDateTimeOffset().ShouldBe(now.AddHours(-2));

        var evidences = candidate.GetProperty("testEvidences").EnumerateArray().ToList();
        evidences.Count.ShouldBe(2);
        evidences[0].EnumerateObject().Select(p => p.Name).ShouldBe(
            ["testKind", "testType", "otherTestTypeName", "testedBy", "testDate", "testedVersion", "evidenceUrl", "qualificationNote"]);
        evidences[0].GetProperty("testKind").GetString().ShouldBe("procedure");
        evidences[0].GetProperty("testType").GetString().ShouldBe("softwareTest");
        evidences[0].GetProperty("otherTestTypeName").ValueKind.ShouldBe(JsonValueKind.Null);
        evidences[0].GetProperty("testedBy").GetString().ShouldBe("Prüfstelle Süd");
        evidences[0].GetProperty("testDate").GetString().ShouldBe(testDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
        evidences[0].GetProperty("testedVersion").GetString().ShouldBe("4.2");
        evidences[0].GetProperty("evidenceUrl").GetString().ShouldBe("https://lernwelt.example/bericht");
        evidences[0].GetProperty("qualificationNote").GetString().ShouldBe("Zertifizierte Prüferin");
        evidences[1].GetProperty("testKind").GetString().ShouldBe("other");
        evidences[1].GetProperty("testType").ValueKind.ShouldBe(JsonValueKind.Null);
        evidences[1].GetProperty("otherTestTypeName").GetString().ShouldBe("Hausinterne Prüfung");
        evidences[1].GetProperty("qualificationNote").ValueKind.ShouldBe(JsonValueKind.Null);

        // The stored assessments in list order (O-S7-1): number, title, value and note, nothing
        // else. No entry for a criterion without a row or for a „Geprüft nach …“ criterion
        // (CAT-17); the row of the excluded criterion is stored and listed.
        var assessments = candidate.GetProperty("assessments").EnumerateArray().ToList();
        assessments.ShouldAllBe(a => a.EnumerateObject().Select(p => p.Name).SequenceEqual(new[] { "number", "title", "value", "note" }));
        assessments.Select(a => (
                a.GetProperty("number").GetString(), a.GetProperty("title").GetString(),
                a.GetProperty("value").GetString(), a.GetProperty("note").GetString()))
            .ShouldBe(
            [
                (requirements[0].Number, TitleOf(requirements[0]), "unknown", "Beim Anbieter nachfragen"),
                (requirements[1].Number, TitleOf(requirements[1]), "restriction", "Laut Prüfbericht, Abschnitt 3"),
                (requirements[2].Number, TitleOf(requirements[2]), "fulfilled", null),
            ]);

        // A candidate with only a name: „unknown“, no private section, no evidence, no assessment.
        candidates[1].GetProperty("testStatus").GetString().ShouldBe("unknown");
        candidates[1].GetProperty("contactNotes").ValueKind.ShouldBe(JsonValueKind.Null);
        candidates[1].GetProperty("internalNotes").ValueKind.ShouldBe(JsonValueKind.Null);
        candidates[1].GetProperty("testEvidences").GetArrayLength().ShouldBe(0);
        candidates[1].GetProperty("assessments").GetArrayLength().ShouldBe(0);

        projects[1].GetProperty("criteriaList").ValueKind.ShouldBe(JsonValueKind.Null);
        projects[1].GetProperty("candidates").GetArrayLength().ShouldBe(0);

        // Nothing of the other projects, no id of any row and no user.
        text.ShouldNotContain("Projekt der Kollegin");
        text.ShouldNotContain("Fremdes Produkt");
        text.ShouldNotContain("Kontakt der Kollegin");
        text.ShouldNotContain("Einschätzung der Kollegin");
        text.ShouldNotContain("Liste der Kollegin");
        text.ShouldNotContain("\"barrier\"");
        text.ShouldNotContain("revisionId", Case.Insensitive);
        text.ShouldNotContain("Projekt von früher");
        text.ShouldNotContain("Verlassene Organisation");
        foreach (var id in new[]
                 {
                     own.Id, ownInB.Id, colleagues.Id, inLeft.Id, full.Id, bare.Id, procedure.Id, other.Id, copy.Id,
                     colleaguesCandidate.Id, colleaguesCopy.Id,
                     Fixture.A.Id, Fixture.B.Id, left.Id,
                 })
        {
            text.ShouldNotContain(id.ToString(), Case.Insensitive);
        }

        foreach (var user in TestUsers.ByName.Values)
        {
            text.ShouldNotContain(user.Id, Case.Insensitive);
            if (user != TestUsers.EditorA)
            {
                text.ShouldNotContain(user.Email, Case.Insensitive);
            }
        }
    }

    private static CandidateAssessment Assessment(
        CandidateProduct candidate, ResolvedCatalogCriterion criterion, Fulfillment value, string? note, DateTimeOffset at)
    {
        var assessment = CandidateAssessment.Of(candidate, criterion.RevisionId);
        assessment.Change(value, note, at);
        return assessment;
    }

    /// <summary>The title as the export names a criterion, in the lists and in the assessments.</summary>
    private static string TitleOf(ResolvedCatalogCriterion criterion) => criterion.PlainLanguageTitle ?? criterion.Title;

    [Fact(DisplayName = "INV:PRV-07 " + nameof(Export_UserWithoutMembershipOrProfile_HasEverySectionOfSpec84AndTheUnbuiltOnesEmpty))]
    [Trait("Invariant", "PRV-07")]
    public async Task Export_UserWithoutMembershipOrProfile_HasEverySectionOfSpec84AndTheUnbuiltOnesEmpty()
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        var root = body.RootElement;
        root.EnumerateObject().Select(p => p.Name).ShouldBe(
            ["generatedAt", "formatVersion", "account", "memberships", "contributorProfiles", "contributions", "createdByMe"]);
        root.GetProperty("generatedAt").GetDateTimeOffset().ShouldBe(Clock.GetUtcNow());
        root.GetProperty("formatVersion").GetInt32().ShouldBe(1);

        var account = root.GetProperty("account");
        account.EnumerateObject().Select(p => p.Name).ShouldBe(["email", "createdAt", "emailConfirmed", "preferences"]);
        account.GetProperty("email").GetString().ShouldBe(TestUsers.Member.Email);
        account.GetProperty("createdAt").ValueKind.ShouldBe(JsonValueKind.Null); // an account from before S5-27
        // Never saved: the defaults (owner decision S5-25 4).
        PreferencesOf(account).ShouldBe(("de", "system", "system", "system", "auto"));

        root.GetProperty("memberships").GetArrayLength().ShouldBe(0);
        root.GetProperty("contributorProfiles").GetArrayLength().ShouldBe(0);
        root.GetProperty("contributions").GetArrayLength().ShouldBe(0);
        var createdByMe = root.GetProperty("createdByMe");
        createdByMe.EnumerateObject().Select(p => p.Name).ShouldBe(["criteriaLists", "projects"]);
        createdByMe.GetProperty("criteriaLists").GetArrayLength().ShouldBe(0);
        createdByMe.GetProperty("projects").GetArrayLength().ShouldBe(0);
    }

    [Fact(DisplayName = "INV:PRV-07 " + nameof(Export_UserWhoSavedSettings_ContainsTheUsersOwnSettings))]
    [Trait("Invariant", "PRV-07")]
    public async Task Export_UserWhoSavedSettings_ContainsTheUsersOwnSettings()
    {
        // Editor A chose dark and reduced motion; Owner B, light — Editor A's export shows only Editor A's.
        var own = UserPreferences.Defaults(TestUsers.EditorA.Id);
        own.Change(UserPreferences.German, AppearancePreference.Dark, MotionPreference.Reduced, ContrastPreference.System);
        own.ChangeAssessmentLegend(AssessmentLegendPreference.Collapsed);
        var other = UserPreferences.Defaults(TestUsers.OwnerB.Id);
        other.Change(UserPreferences.German, AppearancePreference.Light, MotionPreference.System, ContrastPreference.System);
        await StoreAsync(own, other);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        using var body = JsonDocument.Parse(text);
        var account = body.RootElement.GetProperty("account");
        account.GetProperty("preferences").EnumerateObject().Select(p => p.Name)
               .ShouldBe(["language", "appearance", "motion", "contrast", "assessmentLegend"]);
        PreferencesOf(account).ShouldBe(("de", "dark", "reduced", "system", "collapsed"));
        text.ShouldNotContain("\"light\"");
        text.ShouldNotContain(TestUsers.EditorA.Id, Case.Insensitive);
    }

    [Fact]
    public async Task Export_SignedIn_IsAJsonAttachmentNamedByTheDayAndNeverStored()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/json");
        var disposition = response.Content.Headers.ContentDisposition.ShouldNotBeNull();
        disposition.DispositionType.ShouldBe("attachment");
        var day = Clock.GetUtcNow().UtcDateTime.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        disposition.FileName.ShouldBe($"ProcureAccess_Daten_{day}.json");
        AccountExportEndpoints.FileNameOf(Clock.GetUtcNow()).ShouldBe($"ProcureAccess_Daten_{day}.json");
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
    }

    [Fact]
    public async Task Export_Anonymous_Returns401()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Export_DeletedAccount_Returns404NotStored()
    {
        await Api.ResetDatabaseAsync();
        await StoreAsync(AccountOf(TestUsers.Member, createdAt: null, deleted: true));
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
    }

    private static (string?, string?, string?, string?, string?) PreferencesOf(JsonElement account)
    {
        var preferences = account.GetProperty("preferences");
        return (
            preferences.GetProperty("language").GetString(),
            preferences.GetProperty("appearance").GetString(),
            preferences.GetProperty("motion").GetString(),
            preferences.GetProperty("contrast").GetString(),
            preferences.GetProperty("assessmentLegend").GetString());
    }

    private static User AccountOf(TestUser user, DateTimeOffset? createdAt, bool deleted = false) => new()
    {
        Id = user.Id,
        UserName = user.Email,
        NormalizedUserName = user.Email.ToUpperInvariant(),
        Email = user.Email,
        NormalizedEmail = user.Email.ToUpperInvariant(),
        EmailConfirmed = true,
        SecurityStamp = Guid.NewGuid().ToString(),
        CreatedAt = createdAt,
        IsDeleted = deleted,
    };
}
