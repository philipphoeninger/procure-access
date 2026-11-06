// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// The catalog migration seeds the four selection layers with the values of Kriterienkatalog §2 and
/// the keys the owner chose for S3-13 (O-S3-5 D1); Respawn keeps them between test classes. The
/// expected values are written out here, not read from the seed class, so a changed seed fails.
/// </summary>
public sealed class FilterValueSeedTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task Seed_UseCases_AreTheSixOfferedAndOneInactive()
    {
        var useCases = await ReadAsync(db => db.UseCaseTemplates.OrderBy(u => u.Id));

        useCases.Select(u => (u.Key, u.NameDe, u.IsActive)).ShouldBe(
        [
            ("hr", "HR-Anwendung", true),
            ("reporting", "Reporting-Anwendung", true),
            ("trainingManagement", "Fortbildungsmanagement", true),
            ("travelRequests", "Dienstreiseanträge", true),
            ("payroll", "Gehaltsabrechnungen", true),
            ("internalCommunication", "Interne Kommunikations- und Datenaustauschplattform", true),
            ("databaseApplication", "Datenbankgestützte Fachanwendung", false),
        ]);
        useCases.ShouldAllBe(u => u.Description == null);
    }

    [Fact]
    public async Task Seed_AppTypes_AreTheEightInOrderWithFunctionalityAndHints()
    {
        var appTypes = await ReadAsync(db => db.AppTypes.OrderBy(a => a.SortOrder));

        appTypes.Select(a => (a.Key, a.NameDe, a.Functionality)).ShouldBe(
        [
            ("web", "Website oder Webanwendung", AppTypeFunctionality.NotApplicable),
            ("softwareOpen", "Software (offen)", AppTypeFunctionality.Open),
            ("softwareClosed", "Software (geschlossen)", AppTypeFunctionality.Closed),
            ("mobileOpen", "Mobile Anwendung (offen)", AppTypeFunctionality.Open),
            ("mobileClosed", "Mobile Anwendung (geschlossen)", AppTypeFunctionality.Closed),
            ("document", "Dokument", AppTypeFunctionality.NotApplicable),
            ("hardware", "Hardware", AppTypeFunctionality.NotApplicable),
            ("relayEmergencyService", "Vermittlungs- oder Notrufdienst", AppTypeFunctionality.NotApplicable),
        ]);

        // Only the open and closed values carry a hint (Kriterienkatalog §2.2).
        appTypes.Where(a => a.HintDe is not null).Select(a => a.Key)
            .ShouldBe(["softwareOpen", "softwareClosed", "mobileOpen", "mobileClosed"]);
        appTypes.Single(a => a.Key == "softwareClosed").HintDe
            .ShouldBe("Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden, etwa bei Kiosk-Terminals.");
    }

    [Fact]
    public async Task Seed_ProductParts_AreTheSevenWithOnlyBaseAlwaysSelected()
    {
        var productParts = await ReadAsync(db => db.ProductParts.OrderBy(p => p.SortOrder));

        productParts.Select(p => (p.Key, p.NameDe)).ShouldBe(
        [
            ("base", "Basisfunktionalität"),
            ("audio", "Audio"),
            ("video", "Video"),
            ("twoWayVoice", "Zweiwege-Sprachkommunikation"),
            ("rtt", "Echtzeittext (RTT)"),
            ("biometrics", "Biometrie"),
            ("authoringTool", "Autorenwerkzeug"),
        ]);
        productParts.Where(p => p.IsAlwaysSelected).Select(p => p.Key).ShouldBe(["base"]);
        productParts.ShouldAllBe(p => !string.IsNullOrWhiteSpace(p.ExplanationDe));
        productParts.Single(p => p.Key == "audio").ExplanationDe
            .ShouldBe("Audiofunktionalität ist die akustische oder klangliche Wiedergabe von Informationen.");
    }

    [Fact]
    public async Task Seed_TestTypes_AreTheFiveProceduresWithTheirLinks()
    {
        var testTypes = await ReadAsync(db => db.TestTypes.OrderBy(t => t.SortOrder));

        testTypes.Select(t => (t.Key, t.NameDe, t.Url)).ShouldBe(
        [
            ("softwareTest", "BIT inklusiv Software-Test", "https://softwaretest.bit-inklusiv.de/"),
            ("appTest", "BIT inklusiv App-Test", "https://apptest.bit-inklusiv.de/"),
            ("pdfTest", "BIT inklusiv PDF-Test", "https://pdftest.bit-inklusiv.de/"),
            ("bikWeb", "BIK BITV-Test (Web)", "https://bitvtest.de/pruefverfahren/bitv-20-web"),
            ("bikApp", "BIK BITV-Test (App)", "https://bitvtest.de/pruefverfahren/bitv-20-app"),
        ]);
    }

    [Fact]
    public async Task ResetDatabase_FilterValues_AreKept()
    {
        await Api.ResetDatabaseAsync();

        (await ReadAsync(db => db.UseCaseTemplates)).Count.ShouldBe(7);
        (await ReadAsync(db => db.AppTypes)).Count.ShouldBe(8);
        (await ReadAsync(db => db.ProductParts)).Count.ShouldBe(7);
        (await ReadAsync(db => db.TestTypes)).Count.ShouldBe(5);
    }

    private async Task<List<T>> ReadAsync<T>(Func<CatalogDbContext, IQueryable<T>> query)
        where T : class
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        return await query(db).AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
    }
}
