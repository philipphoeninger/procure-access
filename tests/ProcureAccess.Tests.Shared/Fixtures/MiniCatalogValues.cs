// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;

namespace ProcureAccess.Tests.Shared.Fixtures;

/// <summary>
/// The filter values the <see cref="MiniCatalog"/> uses: a subset of the values the catalog
/// migration seeds (O-S3-5 D1), referred to by their seeded keys; all five test types, because
/// <c>catalog activate</c> needs a procedure criterion for each (S3-17).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="InMemory"/> creates new objects with the seeded key, name, flags and sort order, for
/// unit tests; product-part explanations are placeholders (the seeded ones quote BIT inklusiv).
/// <see cref="LoadAsync"/> reads the seeded rows, for integration tests: store the MiniCatalog
/// through the same context, or EF inserts the values again (S3-13 P5). Tests never add filter
/// values of their own, because Respawn keeps these tables between test classes.
/// </para>
/// <para>
/// The integration test <c>MiniCatalogPersistenceTests</c> checks that both sets agree.
/// </para>
/// </remarks>
internal sealed class MiniCatalogValues
{
    private MiniCatalogValues()
    {
    }

    /// <summary>Use case „HR-Anwendung“ (active).</summary>
    public UseCaseTemplate Hr { get; private init; } = null!;

    /// <summary>Use case „Datenbankgestützte Fachanwendung“: inactive, still resolvable (CAT-18).</summary>
    public UseCaseTemplate DatabaseApplication { get; private init; } = null!;

    /// <summary>App type „Website oder Webanwendung“ (sort order 1).</summary>
    public AppType Web { get; private init; } = null!;

    /// <summary>App type „Software (offen)“ (sort order 2).</summary>
    public AppType SoftwareOpen { get; private init; } = null!;

    /// <summary>App type „Mobile Anwendung (offen)“ (sort order 4).</summary>
    public AppType MobileOpen { get; private init; } = null!;

    /// <summary>App type „Hardware“: no requirement criterion and no procedure applies to it (CAT-20); HR excludes it.</summary>
    public AppType Hardware { get; private init; } = null!;

    /// <summary>Product part „Basisfunktionalität“, always selected (CAT-11).</summary>
    public ProductPart Base { get; private init; } = null!;

    /// <summary>Product part „Audio“.</summary>
    public ProductPart Audio { get; private init; } = null!;

    /// <summary>Product part „Video“.</summary>
    public ProductPart Video { get; private init; } = null!;

    /// <summary>Test type „BIT inklusiv Software-Test“.</summary>
    public TestType SoftwareTest { get; private init; } = null!;

    /// <summary>Test type „BIT inklusiv App-Test“.</summary>
    public TestType AppTest { get; private init; } = null!;

    /// <summary>Test type „BIT inklusiv PDF-Test“; no procedure is mapped to it, only its procedure criterion exists.</summary>
    public TestType PdfTest { get; private init; } = null!;

    /// <summary>Test type „BIK BITV-Test (Web)“; as <see cref="PdfTest"/>.</summary>
    public TestType BikWeb { get; private init; } = null!;

    /// <summary>Test type „BIK BITV-Test (App)“; as <see cref="PdfTest"/>.</summary>
    public TestType BikApp { get; private init; } = null!;

    /// <summary>Every seeded test type: a release needs a procedure criterion for each to be activated (S3-17).</summary>
    public IReadOnlyList<TestType> TestTypes => [SoftwareTest, AppTest, PdfTest, BikWeb, BikApp];

    /// <summary>Every value, in the order of the layers.</summary>
    public IReadOnlyList<FilterValue> All =>
        [Hr, DatabaseApplication, Web, SoftwareOpen, MobileOpen, Hardware, Base, Audio, Video, .. TestTypes];

    /// <summary>
    /// New objects with the seeded keys, names, flags and sort orders. Ids and foreign-key properties
    /// stay 0, so unit tests compare values by navigation or key, never by id.
    /// </summary>
    public static MiniCatalogValues InMemory() => new()
    {
        Hr = UseCaseTemplate.Create("hr", "HR-Anwendung"),
        DatabaseApplication = UseCaseTemplate.Create("databaseApplication", "Datenbankgestützte Fachanwendung", isActive: false),
        Web = AppType.Create("web", "Website oder Webanwendung", AppTypeFunctionality.NotApplicable, 1),
        SoftwareOpen = AppType.Create(
            "softwareOpen",
            "Software (offen)",
            AppTypeFunctionality.Open,
            2,
            "Eigene Hilfsmittel wie Screenreader lassen sich verwenden."),
        MobileOpen = AppType.Create(
            "mobileOpen",
            "Mobile Anwendung (offen)",
            AppTypeFunctionality.Open,
            4,
            "Eigene Hilfsmittel wie Screenreader lassen sich verwenden."),
        Hardware = AppType.Create("hardware", "Hardware", AppTypeFunctionality.NotApplicable, 7),
        Base = ProductPart.Create("base", "Basisfunktionalität", "Testerklärung Basisfunktionalität.", 1, isAlwaysSelected: true),
        Audio = ProductPart.Create("audio", "Audio", "Testerklärung Audio.", 2),
        Video = ProductPart.Create("video", "Video", "Testerklärung Video.", 3),
        SoftwareTest = TestType.Create("softwareTest", "BIT inklusiv Software-Test", "https://softwaretest.bit-inklusiv.de/", 1),
        AppTest = TestType.Create("appTest", "BIT inklusiv App-Test", "https://apptest.bit-inklusiv.de/", 2),
        PdfTest = TestType.Create("pdfTest", "BIT inklusiv PDF-Test", "https://pdftest.bit-inklusiv.de/", 3),
        BikWeb = TestType.Create("bikWeb", "BIK BITV-Test (Web)", "https://bitvtest.de/pruefverfahren/bitv-20-web", 4),
        BikApp = TestType.Create("bikApp", "BIK BITV-Test (App)", "https://bitvtest.de/pruefverfahren/bitv-20-app", 5),
    };

    /// <summary>The seeded rows, tracked by <paramref name="db"/>.</summary>
    public static async Task<MiniCatalogValues> LoadAsync(CatalogDbContext db, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        var useCases = await db.UseCaseTemplates
            .Where(u => u.Key == "hr" || u.Key == "databaseApplication")
            .ToDictionaryAsync(u => u.Key, cancellationToken);
        var appTypes = await db.AppTypes
            .Where(a => a.Key == "web" || a.Key == "softwareOpen" || a.Key == "mobileOpen" || a.Key == "hardware")
            .ToDictionaryAsync(a => a.Key, cancellationToken);
        var productParts = await db.ProductParts
            .Where(p => p.Key == "base" || p.Key == "audio" || p.Key == "video")
            .ToDictionaryAsync(p => p.Key, cancellationToken);
        var testTypes = await db.TestTypes.ToDictionaryAsync(t => t.Key, cancellationToken);

        return new MiniCatalogValues
        {
            Hr = useCases["hr"],
            DatabaseApplication = useCases["databaseApplication"],
            Web = appTypes["web"],
            SoftwareOpen = appTypes["softwareOpen"],
            MobileOpen = appTypes["mobileOpen"],
            Hardware = appTypes["hardware"],
            Base = productParts["base"],
            Audio = productParts["audio"],
            Video = productParts["video"],
            SoftwareTest = testTypes["softwareTest"],
            AppTest = testTypes["appTest"],
            PdfTest = testTypes["pdfTest"],
            BikWeb = testTypes["bikWeb"],
            BikApp = testTypes["bikApp"],
        };
    }
}
