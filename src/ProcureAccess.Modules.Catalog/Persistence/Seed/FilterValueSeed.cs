// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
// SPDX-FileCopyrightText: BIT inklusiv
//
// SPDX-License-Identifier: MIT AND LicenseRef-AllRightsReserved
//
// The code is MIT. The product-part explanations are quoted verbatim from the
// Standardanforderungskatalog of BIT inklusiv, used with permission of Detlef Girke,
// and are not MIT (see REUSE.toml).

using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Seed;

/// <summary>
/// The values of the four selection layers, seeded by the catalog migration (O-S3-5 D1; values from
/// Kriterienkatalog §2, keys as decided by the owner for S3-13). Release folders refer to them by key.
/// </summary>
/// <remarks>
/// Only migrations write these tables, so the ids are fixed here and the tables generate none. A
/// new value or a changed text is a change here plus a new migration; never change a key, because
/// releases, configurations and entries refer to it. Respawn leaves the tables alone in the
/// integration tests.
/// </remarks>
internal static class FilterValueSeed
{
    /// <summary><c>CreatedAt</c> of every seeded row (the day of S3-13).</summary>
    public static readonly DateTimeOffset CreatedAt = new(2026, 11, 6, 0, 0, 0, TimeSpan.Zero);

    private const string OpenHint = "Eigene Hilfsmittel wie Screenreader lassen sich verwenden.";

    public static IReadOnlyList<object> UseCases { get; } =
    [
        UseCase(1, "hr", "HR-Anwendung"),
        UseCase(2, "reporting", "Reporting-Anwendung"),
        UseCase(3, "trainingManagement", "Fortbildungsmanagement"),
        UseCase(4, "travelRequests", "Dienstreiseanträge"),
        UseCase(5, "payroll", "Gehaltsabrechnungen"),
        UseCase(6, "internalCommunication", "Interne Kommunikations- und Datenaustauschplattform"),

        // Not offered for now (Kriterienkatalog §2.1, ARCHITECTURE C36).
        UseCase(7, "databaseApplication", "Datenbankgestützte Fachanwendung", isActive: false),
    ];

    public static IReadOnlyList<object> AppTypes { get; } =
    [
        AppType(1, "web", "Website oder Webanwendung", AppTypeFunctionality.NotApplicable, null),
        AppType(2, "softwareOpen", "Software (offen)", AppTypeFunctionality.Open, OpenHint),
        AppType(
            3,
            "softwareClosed",
            "Software (geschlossen)",
            AppTypeFunctionality.Closed,
            "Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden, etwa bei Kiosk-Terminals."),
        AppType(4, "mobileOpen", "Mobile Anwendung (offen)", AppTypeFunctionality.Open, OpenHint),
        AppType(
            5,
            "mobileClosed",
            "Mobile Anwendung (geschlossen)",
            AppTypeFunctionality.Closed,
            "Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden."),
        AppType(6, "document", "Dokument", AppTypeFunctionality.NotApplicable, null),
        AppType(7, "hardware", "Hardware", AppTypeFunctionality.NotApplicable, null),
        AppType(8, "relayEmergencyService", "Vermittlungs- oder Notrufdienst", AppTypeFunctionality.NotApplicable, null),
    ];

    public static IReadOnlyList<object> ProductParts { get; } =
    [
        ProductPart(
            1,
            "base",
            "Basisfunktionalität",
            "Basisfunktionalitäten sind alle Funktionalitäten, die erfahrungsgemäß in allen Anwendungen vorhanden sind. " +
            "Der Filter „Basisfunktionalität\" ist standardmäßig immer ausgewählt.",
            isAlwaysSelected: true),
        ProductPart(
            2,
            "audio",
            "Audio",
            "Audiofunktionalität ist die akustische oder klangliche Wiedergabe von Informationen."),
        ProductPart(
            3,
            "video",
            "Video",
            "Videofunktionalität ist die synchrone Wiedergabe von visuellen (sichtbaren) und optional akustischen " +
            "(hörbaren) Informationen."),
        ProductPart(
            4,
            "twoWayVoice",
            "Zweiwege-Sprachkommunikation",
            "Zweiwege-Sprachkommunikation ist der Austausch von Sprach- und/oder Videoinformationen in zwei Richtungen " +
            "(z. B. Telefonie und Videokonferenzsysteme)."),
        ProductPart(
            5,
            "rtt",
            "Echtzeittext (RTT)",
            "Echtzeittext, Englisch \"real-time-text\" (RTT), bezeichnet die unmittelbare Übertragung von einzelnen " +
            "schriftlichen Zeichen. Empfänger von Echtzeittext können somit den Schreibprozess direkt mitverfolgen. " +
            "Echtzeittext findet vor allem in Kommunikations- und Kollaborationslösungen Anwendung, z. B. in Form " +
            "einer Live-Untertitelung."),
        ProductPart(
            6,
            "biometrics",
            "Biometrie",
            "Biometrie ist die Verwendung von biologischen Merkmalen zur Benutzeridentifikation oder Steuerung, " +
            "z. B. Fingerabdruck oder Gesichtserkennung."),
        ProductPart(
            7,
            "authoringTool",
            "Autorenwerkzeug",
            "Ein Autorenwerkzeug ist jede Möglichkeit, Text in strukturierter Form zu erstellen oder zu bearbeiten, " +
            "z. B. Textverarbeitungsprogramme, White Boards, aber auch Texteingabefelder mit " +
            "Strukturierungsmöglichkeiten (z. B. Kommentarfunktion)."),
    ];

    public static IReadOnlyList<object> TestTypes { get; } =
    [
        TestType(1, "softwareTest", "BIT inklusiv Software-Test", "https://softwaretest.bit-inklusiv.de/"),
        TestType(2, "appTest", "BIT inklusiv App-Test", "https://apptest.bit-inklusiv.de/"),
        TestType(3, "pdfTest", "BIT inklusiv PDF-Test", "https://pdftest.bit-inklusiv.de/"),
        TestType(4, "bikWeb", "BIK BITV-Test (Web)", "https://bitvtest.de/pruefverfahren/bitv-20-web"),
        TestType(5, "bikApp", "BIK BITV-Test (App)", "https://bitvtest.de/pruefverfahren/bitv-20-app"),
    ];

    private static object UseCase(int id, string key, string nameDe, bool isActive = true) => new
    {
        Id = id,
        CreatedAt,
        Key = key,
        NameDe = nameDe,
        IsActive = isActive,
    };

    // The sort order follows the tables of Kriterienkatalog §2.2 – §2.4, so it equals the id.
    private static object AppType(int id, string key, string nameDe, AppTypeFunctionality functionality, string? hintDe) => new
    {
        Id = id,
        CreatedAt,
        Key = key,
        NameDe = nameDe,
        Functionality = functionality,
        HintDe = hintDe,
        SortOrder = id,
    };

    private static object ProductPart(int id, string key, string nameDe, string explanationDe, bool isAlwaysSelected = false) => new
    {
        Id = id,
        CreatedAt,
        Key = key,
        NameDe = nameDe,
        ExplanationDe = explanationDe,
        IsAlwaysSelected = isAlwaysSelected,
        SortOrder = id,
    };

    private static object TestType(int id, string key, string nameDe, string url) => new
    {
        Id = id,
        CreatedAt,
        Key = key,
        NameDe = nameDe,
        Url = url,
        SortOrder = id,
    };
}
