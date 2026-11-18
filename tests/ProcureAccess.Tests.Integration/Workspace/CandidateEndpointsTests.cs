// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.Workspace.Candidates;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;
using ProcureAccess.Modules.Workspace.Projects;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Workspace;

/// <summary>
/// The candidate and evidence endpoints (S6-15; SPEC R-CAN-02, R-CAN-05, §7.2, §8.6 v2.53;
/// ARCHITECTURE §5.4, §7.2): adding, reading and changing a candidate with its private section,
/// the test status „Nicht bekannt“ of a new candidate (SIG-03), the evidences under „Ja“ with at
/// most one per procedure and the status change that removes them (WSP-01), archived projects,
/// the organisation boundary and the confirmed address. Every test starts from the Tenants fixture
/// with the accounts of its users stored (all confirmed but Reader B). The authorisation matrix is
/// <c>AuthorizationMatrixTests</c> (TEN-05).
/// </summary>
public sealed class CandidateEndpointsTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    // Before the fixture clock's start (2026-03-02): the date of an evidence may not lie in the future.
    private static readonly DateOnly TestDate = new(2026, 2, 1);

    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: u != TestUsers.ReaderB))]);
        await StoreAsync(_tenants.Entities);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:SIG-03 " + nameof(Create_EditorOfA_StoresTheCandidateAsNotKnownAndReadsItBack))]
    [Trait("Invariant", "SIG-03")]
    public async Task Create_EditorOfA_StoresTheCandidateAsNotKnownAndReadsItBack()
    {
        var project = await StoreProjectAsync(Fixture.A.Id);
        Clock.Advance(TimeSpan.FromMinutes(5));
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PostAsJsonAsync(
            CandidatesPath(project.Id),
            new
            {
                name = "  Bewerbermanager  ",
                vendor = "Anbieter GmbH",
                website = "https://anbieter.example",
                functionalSummary = "Stellenanzeigen, Bewerbungen",
                contactNotes = "Frau Beispiel, 040 123",
                internalNotes = "Angebot bis März",
            },
            Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        var created = (await response.Content.ReadFromJsonAsync<CandidateResponse>(Cancellation)).ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull().ToString().ShouldBe(CandidatePath(created.Id));
        var read = await ReadAsync(client, created.Id);
        read.ShouldBeEquivalentTo(created);
        read.ShouldSatisfyAllConditions(
            c => c.ProjectId.ShouldBe(project.Id),
            c => c.OrganizationId.ShouldBe(Fixture.A.Id),
            c => c.Name.ShouldBe("Bewerbermanager"),
            c => c.Vendor.ShouldBe("Anbieter GmbH"),
            c => c.Website.ShouldBe("https://anbieter.example"),
            c => c.FunctionalSummary.ShouldBe("Stellenanzeigen, Bewerbungen"),
            c => c.TestStatus.ShouldBe("unknown"),
            c => c.PrivateSection.ShouldBe(new CandidatePrivateSectionResponse("Frau Beispiel, 040 123", "Angebot bis März")),
            c => c.TestEvidences.ShouldBeEmpty(),
            c => c.CreatedAt.ShouldBe(Clock.GetUtcNow()),
            c => c.ModifiedAt.ShouldBe(Clock.GetUtcNow()));

        // The project lists it and counts it, and its last change moved (owner decision S6-15 5 (a)).
        var projectRead = (await client.GetFromJsonAsync<ProjectResponse>(ProjectPath(project.Id), Cancellation)).ShouldNotBeNull();
        projectRead.ProductCount.ShouldBe(1);
        projectRead.Candidates.ShouldHaveSingleItem().ShouldBe(new CandidateSummaryResponse(created.Id, "Bewerbermanager", "Anbieter GmbH", "unknown"));
        projectRead.ModifiedAt.ShouldBe(Clock.GetUtcNow());
        var list = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>($"/api/v1/organizations/{Fixture.A.Id}/projects", Cancellation);
        list.ShouldNotBeNull().ShouldHaveSingleItem().ProductCount.ShouldBe(1);
    }

    [Fact]
    public async Task Create_OnlyAName_Returns201WithoutTheOptionalFields()
    {
        var project = await StoreProjectAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.OwnerA);

        using var response = await client.PostAsJsonAsync(
            CandidatesPath(project.Id), new { name = new string('a', 200), vendor = "  ", website = "" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        var created = (await response.Content.ReadFromJsonAsync<CandidateResponse>(Cancellation)).ShouldNotBeNull();
        created.ShouldSatisfyAllConditions(
            c => c.Vendor.ShouldBeNull(),
            c => c.Website.ShouldBeNull(),
            c => c.FunctionalSummary.ShouldBeNull(),
            c => c.PrivateSection.ShouldBe(new CandidatePrivateSectionResponse(null, null)));
    }

    [Theory(DisplayName = "INV:API-01 " + nameof(Create_InvalidField_Returns400AtTheFieldAndStoresNothing))]
    [Trait("Invariant", "API-01")]
    [InlineData("name", null)]
    [InlineData("name", "   ")]
    [InlineData("name", "201")]
    [InlineData("vendor", "201")]
    [InlineData("website", "http://anbieter.example")]
    [InlineData("website", "anbieter.example")]
    [InlineData("website", "https://anbieter example")]
    [InlineData("functionalSummary", "2001")]
    [InlineData("contactNotes", "2001")]
    [InlineData("internalNotes", "2001")]
    public async Task Create_InvalidField_Returns400AtTheFieldAndStoresNothing(string field, string? value)
    {
        var project = await StoreProjectAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.EditorA);
        var body = new Dictionary<string, string?> { ["name"] = "Produkt", [field] = LongOrValue(value) };

        using var response = await client.PostAsJsonAsync(CandidatesPath(project.Id), body, Cancellation);

        await ShouldBeValidationProblemAsync(response, field);
        (await CountAsync<CandidateProduct>()).ShouldBe(0);
    }

    [Fact]
    public async Task Create_SoftwareProductIdOrUnknownProject_Returns400Or404()
    {
        var project = await StoreProjectAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.EditorA);

        // „Merken“ from the directory comes with M6 (R-CAN-01; owner decision S6-15 1 (a)).
        using var link = await client.PostAsJsonAsync(
            CandidatesPath(project.Id), new { name = "Produkt", softwareProductId = Guid.NewGuid() }, Cancellation);
        using var unknown = await client.PostAsJsonAsync(CandidatesPath(Guid.NewGuid()), new { name = "Produkt" }, Cancellation);

        link.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CountAsync<CandidateProduct>()).ShouldBe(0);
    }

    [Fact]
    public async Task Change_NamedFields_ChangesThemClearsEmptyOnesAndMovesTheLastChanges()
    {
        var (project, candidate) = await StoreCandidateAsync(Fixture.A.Id);
        Clock.Advance(TimeSpan.FromMinutes(5));
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PatchAsJsonAsync(
            CandidatePath(candidate.Id), new { name = " Neuer Name ", website = "", internalNotes = "Neu" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        var changed = (await response.Content.ReadFromJsonAsync<CandidateResponse>(Cancellation)).ShouldNotBeNull();
        changed.ShouldSatisfyAllConditions(
            c => c.Name.ShouldBe("Neuer Name"),
            c => c.Vendor.ShouldBe("Anbieter"),
            c => c.Website.ShouldBeNull(),
            c => c.FunctionalSummary.ShouldBe("Stichworte"),
            c => c.PrivateSection.ShouldBe(new CandidatePrivateSectionResponse("Kontakt", "Neu")),
            c => c.ModifiedAt.ShouldBe(Clock.GetUtcNow()));
        (await ReadAsync(client, candidate.Id)).ShouldBeEquivalentTo(changed);
        (await ReadProjectAsync(client, project.Id)).ModifiedAt.ShouldBe(Clock.GetUtcNow());
    }

    [Fact]
    public async Task Change_OnlyThePrivateSection_MovesTheLastChanges()
    {
        var (project, candidate) = await StoreCandidateAsync(Fixture.A.Id);
        Clock.Advance(TimeSpan.FromMinutes(5));
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PatchAsJsonAsync(CandidatePath(candidate.Id), new { contactNotes = "" }, Cancellation);

        (await response.Content.ReadFromJsonAsync<CandidateResponse>(Cancellation)).ShouldNotBeNull().ShouldSatisfyAllConditions(
            c => c.PrivateSection.ContactNotes.ShouldBeNull(),
            c => c.ModifiedAt.ShouldBe(Clock.GetUtcNow()));
        (await ReadProjectAsync(client, project.Id)).ModifiedAt.ShouldBe(Clock.GetUtcNow());
    }

    [Fact]
    public async Task Change_NothingNamedOrTheSameValues_KeepsTheLastChanges()
    {
        var (project, candidate) = await StoreCandidateAsync(Fixture.A.Id);
        Clock.Advance(TimeSpan.FromMinutes(5));
        using var client = CreateClient(TestUsers.EditorA);

        using var empty = await client.PatchAsJsonAsync(CandidatePath(candidate.Id), new { }, Cancellation);
        using var same = await client.PatchAsJsonAsync(
            CandidatePath(candidate.Id), new { name = "Produkt", contactNotes = "Kontakt" }, Cancellation);

        empty.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await same.Content.ReadFromJsonAsync<CandidateResponse>(Cancellation)).ShouldNotBeNull().ModifiedAt.ShouldBe(candidate.ModifiedAt);
        (await ReadProjectAsync(client, project.Id)).ModifiedAt.ShouldBe(project.ModifiedAt);
    }

    [Theory(DisplayName = "INV:API-01 " + nameof(Change_EmptyNameOrInvalidField_Returns400AndChangesNothing))]
    [Trait("Invariant", "API-01")]
    [InlineData("name", "")]
    [InlineData("vendor", "201")]
    [InlineData("website", "ftp://anbieter.example")]
    public async Task Change_EmptyNameOrInvalidField_Returns400AndChangesNothing(string field, string value)
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PatchAsJsonAsync(
            CandidatePath(candidate.Id), new Dictionary<string, string?> { [field] = LongOrValue(value) }, Cancellation);

        await ShouldBeValidationProblemAsync(response, field);
        (await ReadAsync(client, candidate.Id)).ShouldSatisfyAllConditions(c => c.Name.ShouldBe("Produkt"), c => c.Vendor.ShouldBe("Anbieter"));
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(Evidences_StatusTested_HoldsSeveralInTheOrderTheyWereAdded))]
    [Trait("Invariant", "WSP-01")]
    public async Task Evidences_StatusTested_HoldsSeveralInTheOrderTheyWereAdded()
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.EditorA);

        (await SetStatusAsync(client, candidate.Id, "tested")).StatusCode.ShouldBe(HttpStatusCode.OK);
        using var first = await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("softwareTest") with
        {
            TestedVersion = " 3.2 ",
            EvidenceUrl = "https://bericht.example/1",
            QualificationNote = "Zertifizierte Stelle",
        }, Cancellation);
        Clock.Advance(TimeSpan.FromSeconds(1));
        using var second = await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("bikWeb"), Cancellation);
        Clock.Advance(TimeSpan.FromSeconds(1));
        using var other = await client.PostAsJsonAsync(
            EvidencesPath(candidate.Id), Procedure(null) with { TestKind = "other", OtherTestTypeName = "Hausinterner Test" }, Cancellation);
        Clock.Advance(TimeSpan.FromSeconds(1));
        using var own = await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure(null) with { TestKind = "own" }, Cancellation);

        first.StatusCode.ShouldBe(HttpStatusCode.Created, await first.Content.ReadAsStringAsync(Cancellation));
        var firstEvidence = (await first.Content.ReadFromJsonAsync<TestEvidenceResponse>(Cancellation)).ShouldNotBeNull();
        first.Headers.Location.ShouldNotBeNull().ToString().ShouldBe($"/api/v1/test-evidences/{firstEvidence.Id}");
        firstEvidence.ShouldBe(new TestEvidenceResponse(
            firstEvidence.Id, "procedure", "softwareTest", null, "Prüfstelle", TestDate, "3.2", "https://bericht.example/1", "Zertifizierte Stelle"));
        foreach (var response in new[] { second, other, own })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        }

        var read = await ReadAsync(client, candidate.Id);
        read.TestStatus.ShouldBe("tested");
        read.TestEvidences.Select(e => (e.TestKind, e.TestTypeId, e.OtherTestTypeName)).ShouldBe(
        [
            ("procedure", "softwareTest", null),
            ("procedure", "bikWeb", null),
            ("other", null, "Hausinterner Test"),
            ("own", null, null),
        ]);
        read.TestEvidences[0].ShouldBe(firstEvidence);
    }

    [Theory(DisplayName = "INV:WSP-01 " + nameof(AddEvidence_StatusNotTested_Returns409TestStatusConflictAndStoresNothing))]
    [Trait("Invariant", "WSP-01")]
    [InlineData(null)]
    [InlineData("notTested")]
    public async Task AddEvidence_StatusNotTested_Returns409TestStatusConflictAndStoresNothing(string? status)
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.EditorA);
        if (status is not null)
        {
            (await SetStatusAsync(client, candidate.Id, status)).StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        using var response = await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("softwareTest"), Cancellation);

        await ShouldBeTestStatusConflictAsync(response, evidenceCount: 0);
        (await CountAsync<TestEvidence>()).ShouldBe(0);
        (await CountAsync<TestEvidencePrivateNote>()).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(AddEvidence_SecondOfTheSameProcedure_Returns400AtTestTypeId))]
    [Trait("Invariant", "WSP-01")]
    public async Task AddEvidence_SecondOfTheSameProcedure_Returns400AtTestTypeId()
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.EditorA);
        (await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("softwareTest"), Cancellation)).StatusCode.ShouldBe(HttpStatusCode.Created);

        using var response = await client.PostAsJsonAsync(
            EvidencesPath(candidate.Id), Procedure("softwareTest") with { TestedBy = "Andere Stelle" }, Cancellation);

        await ShouldBeValidationProblemAsync(response, "testTypeId");
        (await CountAsync<TestEvidence>()).ShouldBe(1);
    }

    /// <summary>The unique index holds the rule also below the endpoint (WSP-01, ARCHITECTURE §5.4).</summary>
    [Fact(DisplayName = "INV:WSP-01 " + nameof(Database_TwoRowsOfTheSameProcedure_AreRefused))]
    [Trait("Invariant", "WSP-01")]
    public async Task Database_TwoRowsOfTheSameProcedure_AreRefused()
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        var facts = new TestEvidenceFacts(TestKind.Procedure, "softwareTest", null, "Prüfstelle", TestDate, null, null);
        var duplicate = new TestEvidenceFacts(TestKind.Procedure, "bikWeb", null, "Prüfstelle", TestDate, null, null);
        await using var db = Workspace(Fixture.A.Id);
        var stored = await db.CandidateProducts.Include(c => c.TestEvidences).SingleAsync(c => c.Id == candidate.Id, Cancellation);
        stored.AddEvidence(facts, Clock.GetUtcNow());
        var second = stored.AddEvidence(duplicate, Clock.GetUtcNow()).Evidence!;
        await db.SaveChangesAsync(Cancellation);

        // Past the aggregate: the second row is set to the first one's procedure in the database.
        await Should.ThrowAsync<Npgsql.PostgresException>(() => db.Database.ExecuteSqlAsync(
            $"""UPDATE workspace."TestEvidences" SET "TestTypeKey" = 'softwareTest' WHERE "Id" = {second.Id}""", Cancellation));
    }

    [Theory(DisplayName = "INV:WSP-01 " + nameof(ChangeStatus_AwayFromTestedWithEvidences_Returns409WithTheCountAndChangesNothing))]
    [Trait("Invariant", "WSP-01")]
    [InlineData("notTested", null)]
    [InlineData("unknown", false)]
    public async Task ChangeStatus_AwayFromTestedWithEvidences_Returns409WithTheCountAndChangesNothing(string status, bool? removeEvidences)
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.EditorA);
        await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("softwareTest"), Cancellation);
        await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("appTest"), Cancellation);

        using var response = await client.PutAsJsonAsync(
            StatusPath(candidate.Id), new { testStatus = status, removeEvidences }, Cancellation);

        await ShouldBeTestStatusConflictAsync(response, evidenceCount: 2);
        var read = await ReadAsync(client, candidate.Id);
        read.TestStatus.ShouldBe("tested");
        read.TestEvidences.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(ChangeStatus_AwayFromTestedWithRemoval_RemovesTheEvidencesAndTheirNotes))]
    [Trait("Invariant", "WSP-01")]
    public async Task ChangeStatus_AwayFromTestedWithRemoval_RemovesTheEvidencesAndTheirNotes()
    {
        var (project, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.EditorA);
        await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("softwareTest") with { QualificationNote = "Notiz" }, Cancellation);
        Clock.Advance(TimeSpan.FromMinutes(5));

        using var response = await client.PutAsJsonAsync(
            StatusPath(candidate.Id), new { testStatus = "notTested", removeEvidences = true }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        (await response.Content.ReadFromJsonAsync<CandidateResponse>(Cancellation)).ShouldNotBeNull().ShouldSatisfyAllConditions(
            c => c.TestStatus.ShouldBe("notTested"),
            c => c.TestEvidences.ShouldBeEmpty(),
            c => c.ModifiedAt.ShouldBe(Clock.GetUtcNow()));
        (await CountAsync<TestEvidence>()).ShouldBe(0);
        (await CountAsync<TestEvidencePrivateNote>()).ShouldBe(0);
        (await ReadProjectAsync(client, project.Id)).ModifiedAt.ShouldBe(Clock.GetUtcNow());
    }

    [Fact]
    public async Task ChangeStatus_WithoutEvidences_ChangesItAndTheSameStatusChangesNothing()
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.EditorA);

        using var notTested = await SetStatusAsync(client, candidate.Id, "notTested");
        Clock.Advance(TimeSpan.FromMinutes(5));
        using var same = await SetStatusAsync(client, candidate.Id, "notTested");

        (await notTested.Content.ReadFromJsonAsync<CandidateResponse>(Cancellation)).ShouldNotBeNull().TestStatus.ShouldBe("notTested");
        (await same.Content.ReadFromJsonAsync<CandidateResponse>(Cancellation)).ShouldNotBeNull()
            .ModifiedAt.ShouldBe(Clock.GetUtcNow() - TimeSpan.FromMinutes(5));
    }

    [Theory(DisplayName = "INV:API-01 " + nameof(ChangeStatus_MissingOrUnknownStatus_Returns400AtTestStatus))]
    [Trait("Invariant", "API-01")]
    [InlineData(null)]
    [InlineData("Tested")]
    [InlineData("ja")]
    public async Task ChangeStatus_MissingOrUnknownStatus_Returns400AtTestStatus(string? status)
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PutAsJsonAsync(StatusPath(candidate.Id), new { testStatus = status }, Cancellation);

        await ShouldBeValidationProblemAsync(response, "testStatus");
        (await ReadAsync(client, candidate.Id)).TestStatus.ShouldBe("unknown");
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(ChangeEvidence_NewProductVersion_UpdatesTheSameRow))]
    [Trait("Invariant", "WSP-01")]
    public async Task ChangeEvidence_NewProductVersion_UpdatesTheSameRow()
    {
        var (project, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.EditorA);
        using var added = await client.PostAsJsonAsync(
            EvidencesPath(candidate.Id), Procedure("softwareTest") with { TestedVersion = "1.0", QualificationNote = "Alt" }, Cancellation);
        var evidence = (await added.Content.ReadFromJsonAsync<TestEvidenceResponse>(Cancellation)).ShouldNotBeNull();
        Clock.Advance(TimeSpan.FromDays(1));

        using var response = await client.PutAsJsonAsync(
            EvidencePath(evidence.Id),
            Procedure("softwareTest") with { TestedVersion = "2.0", TestDate = "2026-02-15", QualificationNote = "Neu" },
            Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        var read = await ReadAsync(client, candidate.Id);
        read.TestEvidences.ShouldHaveSingleItem().ShouldBe(
            new TestEvidenceResponse(evidence.Id, "procedure", "softwareTest", null, "Prüfstelle", new DateOnly(2026, 2, 15), "2.0", null, "Neu"));
        read.ModifiedAt.ShouldBe(Clock.GetUtcNow());
        (await ReadProjectAsync(client, project.Id)).ModifiedAt.ShouldBe(Clock.GetUtcNow());
        (await CountAsync<TestEvidence>()).ShouldBe(1);
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(ChangeEvidence_ToAProcedureAnotherEvidenceHas_Returns400AtTestTypeId))]
    [Trait("Invariant", "WSP-01")]
    public async Task ChangeEvidence_ToAProcedureAnotherEvidenceHas_Returns400AtTestTypeId()
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.EditorA);
        await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("softwareTest"), Cancellation);
        // Evidences are listed as they were added; the same time would leave the order to their ids.
        Clock.Advance(TimeSpan.FromSeconds(1));
        using var added = await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("bikWeb"), Cancellation);
        var second = (await added.Content.ReadFromJsonAsync<TestEvidenceResponse>(Cancellation)).ShouldNotBeNull();

        using var response = await client.PutAsJsonAsync(EvidencePath(second.Id), Procedure("softwareTest"), Cancellation);

        await ShouldBeValidationProblemAsync(response, "testTypeId");
        (await ReadAsync(client, candidate.Id)).TestEvidences.Select(e => e.TestTypeId).ShouldBe(["softwareTest", "bikWeb"]);
    }

    [Fact]
    public async Task RemoveEvidence_OneOfTwo_Returns204AndKeepsTheOther()
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.EditorA);
        using var added = await client.PostAsJsonAsync(
            EvidencesPath(candidate.Id), Procedure("softwareTest") with { QualificationNote = "Notiz" }, Cancellation);
        var first = (await added.Content.ReadFromJsonAsync<TestEvidenceResponse>(Cancellation)).ShouldNotBeNull();
        await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("bikWeb"), Cancellation);

        using var response = await client.DeleteAsync(EvidencePath(first.Id), Cancellation);
        using var again = await client.DeleteAsync(EvidencePath(first.Id), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        again.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadAsync(client, candidate.Id)).TestEvidences.ShouldHaveSingleItem().TestTypeId.ShouldBe("bikWeb");
        (await CountAsync<TestEvidencePrivateNote>()).ShouldBe(1);
    }

    [Theory(DisplayName = "INV:API-01 " + nameof(AddEvidence_InvalidField_Returns400AtTheField))]
    [Trait("Invariant", "API-01")]
    [InlineData("testKind", null)]
    [InlineData("testKind", "Procedure")]
    [InlineData("testTypeId", null)]
    [InlineData("testTypeId", "unbekannt")]
    [InlineData("testedBy", null)]
    [InlineData("testedBy", "  ")]
    [InlineData("testedBy", "201")]
    [InlineData("testDate", null)]
    [InlineData("testDate", "01.10.2026")]
    [InlineData("testedVersion", "201")]
    [InlineData("evidenceUrl", "http://bericht.example")]
    [InlineData("qualificationNote", "2001")]
    public async Task AddEvidence_InvalidField_Returns400AtTheField(string field, string? value)
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.EditorA);
        var body = EvidenceBody(Procedure("softwareTest"));
        body[field] = LongOrValue(value);

        using var response = await client.PostAsJsonAsync(EvidencesPath(candidate.Id), body, Cancellation);

        await ShouldBeValidationProblemAsync(response, field);
        (await CountAsync<TestEvidence>()).ShouldBe(0);
    }

    [Theory(DisplayName = "INV:API-01 " + nameof(AddEvidence_NamesThatDoNotFitTheKind_Returns400))]
    [Trait("Invariant", "API-01")]
    [InlineData("other", null, null, "otherTestTypeName")]
    [InlineData("other", null, "121", "otherTestTypeName")]
    [InlineData("other", "softwareTest", "Hausintern", "testTypeId")]
    [InlineData("own", "softwareTest", null, "testTypeId")]
    [InlineData("own", null, "Hausintern", "otherTestTypeName")]
    [InlineData("procedure", "softwareTest", "Hausintern", "otherTestTypeName")]
    public async Task AddEvidence_NamesThatDoNotFitTheKind_Returns400(string kind, string? testTypeId, string? otherName, string field)
    {
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.EditorA);
        var body = Procedure(testTypeId) with { TestKind = kind, OtherTestTypeName = otherName == "121" ? new string('a', 121) : otherName };

        using var response = await client.PostAsJsonAsync(EvidencesPath(candidate.Id), body, Cancellation);

        await ShouldBeValidationProblemAsync(response, field);
    }

    /// <summary>The date may be today in Europe/Berlin, never later (owner decision S6-15 2 (a)), also when UTC is a day behind.</summary>
    [Fact]
    public async Task AddEvidence_TodayInBerlinAndTheDayAfter_AcceptsOnlyToday()
    {
        // 23:30 UTC is already the next day in Berlin, in summer and in winter.
        var now = Clock.GetUtcNow();
        var lateEvening = new DateTimeOffset(now.UtcDateTime.Date.AddHours(23).AddMinutes(30), TimeSpan.Zero);
        Clock.Advance((lateEvening > now ? lateEvening : lateEvening.AddDays(1)) - now);
        var berlinToday = DateOnly.FromDateTime(Clock.GetUtcNow().UtcDateTime).AddDays(1);
        var (_, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.EditorA);

        using var tomorrow = await client.PostAsJsonAsync(
            EvidencesPath(candidate.Id), Procedure("softwareTest") with { TestDate = berlinToday.AddDays(1).ToString("yyyy-MM-dd") }, Cancellation);
        using var today = await client.PostAsJsonAsync(
            EvidencesPath(candidate.Id), Procedure("softwareTest") with { TestDate = berlinToday.ToString("yyyy-MM-dd") }, Cancellation);

        await ShouldBeValidationProblemAsync(tomorrow, "testDate");
        today.StatusCode.ShouldBe(HttpStatusCode.Created, await today.Content.ReadAsStringAsync(Cancellation));
    }

    /// <summary>R-PRJ-05: nothing of an archived project changes, its candidates included; reading stays possible.</summary>
    [Fact]
    public async Task Changes_ArchivedProject_Return409ProjectArchivedAndChangeNothing()
    {
        var (project, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        using var client = CreateClient(TestUsers.OwnerA);
        using var added = await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("softwareTest"), Cancellation);
        var evidence = (await added.Content.ReadFromJsonAsync<TestEvidenceResponse>(Cancellation)).ShouldNotBeNull();
        (await client.PatchAsJsonAsync(ProjectPath(project.Id), new { status = "archived" }, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage[] responses =
        [
            await client.PostAsJsonAsync(CandidatesPath(project.Id), new { name = "Neu" }, Cancellation),
            await client.PatchAsJsonAsync(CandidatePath(candidate.Id), new { name = "Umbenannt" }, Cancellation),
            await client.PutAsJsonAsync(StatusPath(candidate.Id), new { testStatus = "unknown", removeEvidences = true }, Cancellation),
            await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("bikWeb"), Cancellation),
            await client.PutAsJsonAsync(EvidencePath(evidence.Id), Procedure("softwareTest") with { TestedVersion = "2" }, Cancellation),
            await client.DeleteAsync(EvidencePath(evidence.Id), Cancellation),
        ];

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.ProjectArchived);
            response.Dispose();
        }

        var read = await ReadAsync(client, candidate.Id);
        read.ShouldSatisfyAllConditions(
            c => c.Name.ShouldBe("Produkt"),
            c => c.TestStatus.ShouldBe("tested"),
            c => c.TestEvidences.ShouldHaveSingleItem().ShouldBe(evidence));
        (await CountAsync<CandidateProduct>()).ShouldBe(1);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Changes_UnconfirmedAddress_Return403EmailUnconfirmedAndChangeNothing))]
    [Trait("Invariant", "SEC-10")]
    public async Task Changes_UnconfirmedAddress_Return403EmailUnconfirmedAndChangeNothing()
    {
        await StoreAsync(OrganizationMembership.Create(Fixture.A.Id, TestUsers.ReaderB.Id, OrganizationRole.Editor, Clock.GetUtcNow(), Clock.GetUtcNow()));
        var (project, candidate) = await StoreCandidateAsync(Fixture.A.Id, TestStatus.Tested);
        var evidence = await StoreEvidenceAsync(candidate.Id);
        using var client = CreateClient(TestUsers.ReaderB);

        HttpResponseMessage[] responses =
        [
            await client.PostAsJsonAsync(CandidatesPath(project.Id), new { name = "Neu" }, Cancellation),
            await client.PatchAsJsonAsync(CandidatePath(candidate.Id), new { name = "Umbenannt" }, Cancellation),
            await client.PutAsJsonAsync(StatusPath(candidate.Id), new { testStatus = "unknown", removeEvidences = true }, Cancellation),
            await client.PostAsJsonAsync(EvidencesPath(candidate.Id), Procedure("bikWeb"), Cancellation),
            await client.PutAsJsonAsync(EvidencePath(evidence), Procedure("softwareTest") with { TestedVersion = "2" }, Cancellation),
            await client.DeleteAsync(EvidencePath(evidence), Cancellation),
        ];
        // Reading needs no confirmed address.
        var read = await ReadAsync(client, candidate.Id);

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);
            response.Dispose();
        }

        read.ShouldSatisfyAllConditions(
            c => c.Name.ShouldBe("Produkt"),
            c => c.TestStatus.ShouldBe("tested"),
            c => c.TestEvidences.ShouldHaveSingleItem().Id.ShouldBe(evidence));
        (await CountAsync<CandidateProduct>()).ShouldBe(1);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(Requests_CandidateAndEvidenceOfAnotherOrganization_Return404AndChangeNothing))]
    [Trait("Invariant", "TEN-01")]
    public async Task Requests_CandidateAndEvidenceOfAnotherOrganization_Return404AndChangeNothing()
    {
        var (projectOfB, candidateOfB) = await StoreCandidateAsync(Fixture.B.Id, TestStatus.Tested);
        var evidenceOfB = await StoreEvidenceAsync(candidateOfB.Id, Fixture.B.Id);
        using var client = CreateClient(TestUsers.OwnerA);
        // Naming B in a header changes nothing: the organisation comes from the stored rows.
        client.DefaultRequestHeaders.Add("X-Organization-Id", Fixture.B.Id.ToString());

        HttpResponseMessage[] responses =
        [
            await client.GetAsync(CandidatePath(candidateOfB.Id), Cancellation),
            await client.PostAsJsonAsync(CandidatesPath(projectOfB.Id), new { name = "Neu" }, Cancellation),
            await client.PatchAsJsonAsync(CandidatePath(candidateOfB.Id), new { name = "Übernommen" }, Cancellation),
            await client.PutAsJsonAsync(StatusPath(candidateOfB.Id), new { testStatus = "unknown", removeEvidences = true }, Cancellation),
            await client.PostAsJsonAsync(EvidencesPath(candidateOfB.Id), Procedure("bikWeb"), Cancellation),
            await client.PutAsJsonAsync(EvidencePath(evidenceOfB), Procedure("softwareTest") with { TestedVersion = "2" }, Cancellation),
            await client.DeleteAsync(EvidencePath(evidenceOfB), Cancellation),
        ];

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            response.Dispose();
        }

        await using var db = Workspace(Fixture.B.Id);
        (await db.CandidateProducts.Include(c => c.TestEvidences).SingleAsync(Cancellation)).ShouldSatisfyAllConditions(
            c => c.Name.ShouldBe("Produkt"),
            c => c.TestStatus.ShouldBe(TestStatus.Tested),
            c => c.TestEvidences.ShouldHaveSingleItem().TestedVersion.ShouldBeNull());
    }

    [Fact]
    public async Task Read_UnknownOrMalformedId_Returns404Or400()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        using var unknown = await client.GetAsync(CandidatePath(Guid.NewGuid()), Cancellation);
        using var malformed = await client.GetAsync("/api/v1/candidates/keine-id", Cancellation);

        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        malformed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static string ProjectPath(Guid id) => $"/api/v1/projects/{id}";

    private static string CandidatesPath(Guid projectId) => $"/api/v1/projects/{projectId}/candidates";

    private static string CandidatePath(Guid id) => $"/api/v1/candidates/{id}";

    private static string StatusPath(Guid candidateId) => $"/api/v1/candidates/{candidateId}/test-status";

    private static string EvidencesPath(Guid candidateId) => $"/api/v1/candidates/{candidateId}/test-evidences";

    private static string EvidencePath(Guid id) => $"/api/v1/test-evidences/{id}";

    /// <summary>"201" and "2001" stand for texts of that length, so the theory data stays short.</summary>
    private static string? LongOrValue(string? value) => value switch
    {
        "121" => new string('a', 121),
        "201" => new string('a', 201),
        "2001" => new string('a', 2001),
        _ => value,
    };

    /// <summary>A complete evidence body of a procedure (or, with <c>null</c>, without a test type).</summary>
    private static EvidenceRequest Procedure(string? testTypeId) =>
        new("procedure", testTypeId, null, "Prüfstelle", TestDate.ToString("yyyy-MM-dd"), null, null, null);

    private static Dictionary<string, string?> EvidenceBody(EvidenceRequest request) => new()
    {
        ["testKind"] = request.TestKind,
        ["testTypeId"] = request.TestTypeId,
        ["otherTestTypeName"] = request.OtherTestTypeName,
        ["testedBy"] = request.TestedBy,
        ["testDate"] = request.TestDate,
        ["testedVersion"] = request.TestedVersion,
        ["evidenceUrl"] = request.EvidenceUrl,
        ["qualificationNote"] = request.QualificationNote,
    };

    private async Task<HttpResponseMessage> SetStatusAsync(HttpClient client, Guid candidateId, string status) =>
        await client.PutAsJsonAsync(StatusPath(candidateId), new { testStatus = status }, Cancellation);

    private static async Task<CandidateResponse> ReadAsync(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<CandidateResponse>(CandidatePath(id), Cancellation)).ShouldNotBeNull();

    private static async Task<ProjectResponse> ReadProjectAsync(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<ProjectResponse>(ProjectPath(id), Cancellation)).ShouldNotBeNull();

    private async Task<ProcurementProject> StoreProjectAsync(Guid organizationId)
    {
        var project = ProcurementProject.Create(organizationId, "Projekt", TestUsers.OwnerA.Id, Clock.GetUtcNow());
        await StoreAsync(project);
        return project;
    }

    /// <summary>
    /// Stores a project with one candidate (name „Produkt“, vendor „Anbieter“, website, summary
    /// „Stichworte“, contacts „Kontakt“) at the clock's time, with <paramref name="status"/>.
    /// </summary>
    private async Task<(ProcurementProject Project, CandidateProduct Candidate)> StoreCandidateAsync(
        Guid organizationId, TestStatus status = TestStatus.Unknown)
    {
        var now = Clock.GetUtcNow();
        var project = ProcurementProject.Create(organizationId, "Projekt", TestUsers.OwnerA.Id, now);
        var candidate = CandidateProduct.Create(
            project, new CandidateFacts("Produkt", "Anbieter", "https://anbieter.example", "Stichworte"), now);
        candidate.ChangeTestStatus(status, removeEvidences: false, now);
        await StoreAsync(project, candidate, CandidatePrivateSection.Of(candidate, "Kontakt", null));
        return (project, candidate);
    }

    /// <summary>Stores a „softwareTest“ evidence with its private note on a stored „Ja“ candidate; returns its id.</summary>
    private async Task<Guid> StoreEvidenceAsync(Guid candidateId, Guid? organizationId = null)
    {
        await using var db = Workspace(organizationId ?? Fixture.A.Id);
        var candidate = await db.CandidateProducts.Include(c => c.TestEvidences).SingleAsync(c => c.Id == candidateId, Cancellation);
        var evidence = candidate.AddEvidence(
            new TestEvidenceFacts(TestKind.Procedure, "softwareTest", null, "Prüfstelle", TestDate, null, null), Clock.GetUtcNow()).Evidence!;
        db.TestEvidencePrivateNotes.Add(TestEvidencePrivateNote.Of(evidence, null));
        await db.SaveChangesAsync(Cancellation);
        return evidence.Id;
    }

    private async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        await using var db = Workspace(Fixture.A.Id, Fixture.B.Id);
        return await db.Set<TEntity>().CountAsync(Cancellation);
    }

    /// <summary>A context of its own outside any request that sees <paramref name="organizationIds"/>.</summary>
    private WorkspaceDbContext Workspace(params Guid[] organizationIds) =>
        new(new DbContextOptionsBuilder<WorkspaceDbContext>().UseNpgsql(Api.ConnectionString).Options,
            FixedOrganizationContext.Of(organizationIds));

    private static async Task ShouldBeValidationProblemAsync(HttpResponseMessage response, params string[] errorKeys)
    {
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, text);
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.Validation);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ShouldBe(errorKeys, ignoreOrder: true);
    }

    /// <summary>409 <c>test-status-conflict</c> with its field error at <c>testStatus</c> and the count (owner decision S6-15 3 (a)).</summary>
    private static async Task ShouldBeTestStatusConflictAsync(HttpResponseMessage response, int evidenceCount)
    {
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, text);
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.TestStatusConflict);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ShouldBe(["testStatus"]);
        body.RootElement.GetProperty("evidenceCount").GetInt32().ShouldBe(evidenceCount);
    }

    private static async Task<string> TypeOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return body.RootElement.GetProperty("type").GetString()!;
    }

    /// <summary>An evidence body as the client sends it; the date as text, so a malformed one can be sent too.</summary>
    private sealed record EvidenceRequest(
        string TestKind,
        string? TestTypeId,
        string? OtherTestTypeName,
        string? TestedBy,
        string? TestDate,
        string? TestedVersion,
        string? EvidenceUrl,
        string? QualificationNote);
}
