// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using static ProcureAccess.Tests.Shared.Builders.Catalog.CatalogData;

namespace ProcureAccess.Tests.Shared.Fixtures;

/// <summary>
/// The canonical test catalog of migration step 2 (TESTING §6.2): two published, inactive releases
/// <c>T.1</c> and <c>T.2</c> with 12 and 11 criteria on the seeded filter values. Made-up texts;
/// every rule of ARCHITECTURE §5.1 that the catalog tests need is reachable with it.
/// </summary>
/// <remarks>
/// <para>What it holds, by the case it serves:</para>
/// <list type="bullet">
/// <item>Priority resolution (CAT-01, CAT-02): <see cref="Keys.PriorityAllLevels"/> has a rule on
/// each of the four levels, plus a level-3 rule for the inactive use case (CAT-18).</item>
/// <item>Several app types (CAT-14): <see cref="Keys.PriorityAppTypes"/> decides by level, by
/// weight and, at equal level and weight, by <c>AppType.SortOrder</c>.</item>
/// <item>Product parts: <see cref="Keys.DefaultOnly"/> applies with Basisfunktionalität,
/// <see cref="Keys.AudioOnly"/> and <see cref="Keys.VideoOnly"/> only with their part.</item>
/// <item>No requirement criterion applies to Hardware, and no procedure is mapped to it (CAT-20).
/// HR excludes Hardware, so a Hardware selection has no use case or the inactive one.</item>
/// <item>Exclusions on all six pairs of layers (CAT-12, CAT-13); <c>T.2</c> drops HR → Video and
/// adds HR → Audio (CAT-19), so in <c>T.2</c> Audio has two reasons (HR, Mobile offen) for
/// <c>excludedBy[]</c>.</item>
/// <item>Procedure mapping with one app type on two procedures; <c>T.2</c> drops Mobile (offen) →
/// Software-Test (CAT-21). Procedure texts with different allowed ratings per test type (CAT-15).</item>
/// <item>One procedure criterion per test type, catalog default B (CAT-16).</item>
/// <item>From <c>T.1</c> to <c>T.2</c> (CAT-03, CAT-06, CAT-07): one criterion changed (revision 2),
/// one added, one removed, one renamed 1:1 and two merged into one (the non-1:1
/// <c>SupersededByKey</c>; a split cannot be stored with one successor key, S3-14). The seven
/// unchanged revisions are shared, so <c>T.2</c> brings exactly four new revisions.</item>
/// </list>
/// <para>
/// The same content is in the test release folders <c>Fixtures/Releases/T.1</c> and <c>T.2</c>
/// (<see cref="TestReleaseFolders"/>, S3-15); a change here needs the same change there.
/// </para>
/// <para>
/// <c>SupersededByKey</c> is set when the old criterion is created, because a <see cref="Criterion"/>
/// cannot change afterwards; both releases are built at once.
/// </para>
/// </remarks>
internal sealed class MiniCatalog
{
    public const string T1Version = "T.1";
    public const string T2Version = "T.2";

    /// <summary><c>PublishedAt</c> of <c>T.1</c>.</summary>
    public static readonly DateTimeOffset T1PublishedAt = DefaultPublishedAt;

    /// <summary><c>PublishedAt</c> of <c>T.2</c>.</summary>
    public static readonly DateTimeOffset T2PublishedAt = DefaultPublishedAt.AddMonths(1);

    private MiniCatalog(MiniCatalogValues values, CatalogRelease t1, CatalogRelease t2)
    {
        Values = values;
        T1 = t1;
        T2 = t2;
    }

    public MiniCatalogValues Values { get; }

    public CatalogRelease T1 { get; }

    public CatalogRelease T2 { get; }

    public IReadOnlyList<CatalogRelease> Releases => [T1, T2];

    /// <summary>
    /// Builds both releases on <paramref name="values"/>: <see cref="MiniCatalogValues.InMemory"/>
    /// in unit tests, <see cref="MiniCatalogValues.LoadAsync"/> in integration tests.
    /// </summary>
    public static MiniCatalog Build(MiniCatalogValues values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var v = values;
        var en301549 = StandardReference.Create(Standard.En301549, "T.9.1.1", "https://example.org/en301549#T.9.1.1");

        // Requirement criteria whose revision both releases share.
        var priorityAllLevels = ARevision().WithKey(Keys.PriorityAllLevels).WithSortOrder(10)
            .WithTitle("Testkriterium mit Regeln auf allen vier Ebenen")
            .AppliesTo(v.Web).AppliesTo(v.SoftwareOpen).AppliesTo(v.MobileOpen)
            .WithPriority(v.Web, v.Hr, PriorityLevel.A)
            .WithPriority(v.SoftwareOpen, null, PriorityLevel.Info)
            .WithPriority(null, v.Hr, PriorityLevel.B, weight: 5)
            .WithPriority(null, v.DatabaseApplication, PriorityLevel.A)
            .WithDefaultPriority(PriorityLevel.B)
            .WithProcedureText(v.SoftwareTest, Rating.All & ~Rating.NotApplicable)
            .WithProcedureText(v.AppTest, Rating.Fulfilled | Rating.Barrier | Rating.Blockade | Rating.NotApplicable)
            .Build();
        var priorityAppTypes = ARevision().WithKey(Keys.PriorityAppTypes).WithSortOrder(20)
            .WithTitle("Testkriterium für mehrere Anwendungstypen")
            .AppliesTo(v.Web).AppliesTo(v.SoftwareOpen).AppliesTo(v.MobileOpen)
            .WithPriority(v.Web, v.Hr, PriorityLevel.B, weight: 2)
            .WithPriority(v.SoftwareOpen, v.Hr, PriorityLevel.B, weight: 2)
            .WithPriority(v.Web, null, PriorityLevel.B, weight: 1)
            .WithPriority(v.SoftwareOpen, null, PriorityLevel.B, weight: 3)
            .WithPriority(v.MobileOpen, null, PriorityLevel.A)
            .WithDefaultPriority(PriorityLevel.Info)
            .Build();
        var defaultOnly = ARevision().WithKey(Keys.DefaultOnly).WithSortOrder(30)
            .WithTitle("Testkriterium der Basisfunktionalität")
            .AppliesTo(v.Web, v.Base).AppliesTo(v.SoftwareOpen, v.Base).AppliesTo(v.MobileOpen, v.Base)
            .WithDefaultPriority(PriorityLevel.B)
            .WithProcedureText(v.SoftwareTest, Rating.All)
            .Citing(en301549)
            .Build();
        var audioOnly = ARevision().WithKey(Keys.AudioOnly).WithSortOrder(40)
            .WithTitle("Testkriterium für Audio")
            .AppliesTo(v.Web, v.Audio).AppliesTo(v.SoftwareOpen, v.Audio)
            .WithDefaultPriority(PriorityLevel.A)
            .WithProcedureText(v.SoftwareTest, Rating.Fulfilled | Rating.Restriction | Rating.Blockade)
            .Build();
        var videoOnly = ARevision().WithKey(Keys.VideoOnly).WithSortOrder(50)
            .WithTitle("Testkriterium für Video")
            .AppliesTo(v.Web, v.Video).AppliesTo(v.MobileOpen, v.Video)
            .WithDefaultPriority(PriorityLevel.B)
            .Build();
        var procedureSoftwareTest = ARevision().Of(ACriterion().ProcedureFor(v.SoftwareTest)).WithSortOrder(900)
            .WithTitle($"Geprüft nach {v.SoftwareTest.NameDe}")
            .WithDefaultPriority(PriorityLevel.B)
            .Build();
        var procedureAppTest = ARevision().Of(ACriterion().ProcedureFor(v.AppTest)).WithSortOrder(910)
            .WithTitle($"Geprüft nach {v.AppTest.NameDe}")
            .WithDefaultPriority(PriorityLevel.B)
            .Build();

        // Requirement criteria that differ between the releases.
        var changedCriterion = ACriterion().WithKey(Keys.Changed).Build();
        var changedV1 = ARevision().Of(changedCriterion).WithSortOrder(60)
            .WithTitle("Testkriterium, Fassung 1")
            .AppliesTo(v.Web)
            .WithDefaultPriority(PriorityLevel.B)
            .WithProcedureText(v.SoftwareTest, Rating.Fulfilled | Rating.Blockade)
            .Citing(en301549)
            .Build();
        var changedV2 = ARevision().Of(changedCriterion).WithRevisionNumber(2).WithSortOrder(60)
            .WithTitle("Testkriterium, Fassung 2")
            .AppliesTo(v.Web)
            .WithDefaultPriority(PriorityLevel.A)
            .WithProcedureText(v.SoftwareTest, Rating.All)
            .Citing(en301549)
            .Build();
        var removed = WebCriterion(ACriterion().WithKey(Keys.Removed), 70, PriorityLevel.Info);
        var renamedOld = WebCriterion(ACriterion().WithKey(Keys.RenamedOld).SupersededBy(Keys.RenamedNew), 80, PriorityLevel.B);
        var renamedNew = WebCriterion(ACriterion().WithKey(Keys.RenamedNew), 80, PriorityLevel.B);
        var mergedA = WebCriterion(ACriterion().WithKey(Keys.MergedA).SupersededBy(Keys.Merged), 90, PriorityLevel.B);
        var mergedB = WebCriterion(ACriterion().WithKey(Keys.MergedB).SupersededBy(Keys.Merged), 91, PriorityLevel.B);
        var merged = WebCriterion(ACriterion().WithKey(Keys.Merged), 90, PriorityLevel.A);
        var added = ARevision().WithKey(Keys.Added).WithSortOrder(100)
            .WithTitle($"Testkriterium {Keys.Added}")
            .AppliesTo(v.SoftwareOpen)
            .WithDefaultPriority(PriorityLevel.B)
            .Build();

        // Exclusions: one per pair of layers; T.2 swaps the use case → product part rule.
        var hrHardware = FilterExclusion.Create(v.Hr, v.Hardware, "Testgrund: HR-Anwendung schließt Hardware aus.");
        var hrVideo = FilterExclusion.Create(v.Hr, v.Video, "Testgrund: HR-Anwendung schließt Video aus.");
        var hrAudio = FilterExclusion.Create(v.Hr, v.Audio, "Testgrund: HR-Anwendung schließt Audio aus.");
        var databaseAppTest = FilterExclusion.Create(
            v.DatabaseApplication, v.AppTest, "Testgrund: Datenbankgestützte Fachanwendung schließt den App-Test aus.");
        var mobileAudio = FilterExclusion.Create(v.MobileOpen, v.Audio, "Testgrund: Mobile Anwendung schließt Audio aus.");
        var webAppTest = FilterExclusion.Create(v.Web, v.AppTest, "Testgrund: Webanwendung schließt den App-Test aus.");
        var audioSoftwareTest = FilterExclusion.Create(v.Audio, v.SoftwareTest, "Testgrund: Audio schließt den Software-Test aus.");

        var t1 = ACatalogRelease().WithVersion(T1Version).WithReleaseNotes("Testkatalog, erste Fassung")
            .Including(
                priorityAllLevels, priorityAppTypes, defaultOnly, audioOnly, videoOnly, changedV1, removed, renamedOld,
                mergedA, mergedB, procedureSoftwareTest, procedureAppTest)
            .WithExclusion(hrHardware).WithExclusion(hrVideo).WithExclusion(databaseAppTest)
            .WithExclusion(mobileAudio).WithExclusion(webAppTest).WithExclusion(audioSoftwareTest)
            .WithProcedure(v.Web, v.SoftwareTest).WithProcedure(v.SoftwareOpen, v.SoftwareTest)
            .WithProcedure(v.MobileOpen, v.AppTest).WithProcedure(v.MobileOpen, v.SoftwareTest)
            .PublishedAt(T1PublishedAt)
            .Build();
        var t2 = ACatalogRelease().WithVersion(T2Version).WithReleaseNotes("Testkatalog, zweite Fassung")
            .Including(
                priorityAllLevels, priorityAppTypes, defaultOnly, audioOnly, videoOnly, changedV2, renamedNew, merged,
                added, procedureSoftwareTest, procedureAppTest)
            .WithExclusion(hrHardware).WithExclusion(hrAudio).WithExclusion(databaseAppTest)
            .WithExclusion(mobileAudio).WithExclusion(webAppTest).WithExclusion(audioSoftwareTest)
            .WithProcedure(v.Web, v.SoftwareTest).WithProcedure(v.SoftwareOpen, v.SoftwareTest)
            .WithProcedure(v.MobileOpen, v.AppTest)
            .PublishedAt(T2PublishedAt)
            .Build();

        return new MiniCatalog(values, t1, t2);

        CriterionRevision WebCriterion(Criterion criterion, int sortOrder, PriorityLevel level) =>
            ARevision().Of(criterion).WithSortOrder(sortOrder)
                .WithTitle($"Testkriterium {criterion.Key}")
                .AppliesTo(v.Web)
                .WithDefaultPriority(level)
                .Build();
    }

    /// <summary>The revision of <paramref name="key"/> in <paramref name="release"/>.</summary>
    public static CriterionRevision RevisionOf(CatalogRelease release, string key)
    {
        ArgumentNullException.ThrowIfNull(release);
        return release.Criteria.Single(c => c.Revision.Criterion.Key == key).Revision;
    }

    /// <summary>
    /// The criterion keys of the MiniCatalog. They follow the key scheme of release folders (S3-15
    /// decision 1a) in chapter 99, which EN 301 549 does not have, so they never look like real criteria.
    /// </summary>
    public static class Keys
    {
        /// <summary>Priority rules on all four levels: (Web, HR) A; (Software offen, –) Info; (–, HR) B weight 5; (–, Datenbank) A; default B.</summary>
        public const string PriorityAllLevels = "99.1";

        /// <summary>
        /// Several app types: (Web, HR) and (Software offen, HR) B weight 2 (tie, Web wins by sort
        /// order); (Web, –) B weight 1; (Software offen, –) B weight 3; (Mobile offen, –) A; default Info.
        /// </summary>
        public const string PriorityAppTypes = "99.2";

        /// <summary>Only the catalog default B; applies with Basisfunktionalität.</summary>
        public const string DefaultOnly = "99.3";

        /// <summary>Applies only with Audio (Web, Software offen); default A.</summary>
        public const string AudioOnly = "99.4";

        /// <summary>Applies only with Video (Web, Mobile offen); default B.</summary>
        public const string VideoOnly = "99.5";

        /// <summary>Revision 1 in T.1 (default B), revision 2 in T.2 (default A, other allowed ratings).</summary>
        public const string Changed = "99.6";

        /// <summary>Only in T.1.</summary>
        public const string Removed = "99.7";

        /// <summary>Only in T.1; superseded 1:1 by <see cref="RenamedNew"/>.</summary>
        public const string RenamedOld = "99.8";

        /// <summary>Only in T.2; replaces <see cref="RenamedOld"/>.</summary>
        public const string RenamedNew = "99.9";

        /// <summary>Only in T.1; merged with <see cref="MergedB"/> into <see cref="Merged"/>.</summary>
        public const string MergedA = "99.10a";

        /// <summary>Only in T.1; merged with <see cref="MergedA"/> into <see cref="Merged"/>.</summary>
        public const string MergedB = "99.10b";

        /// <summary>Only in T.2; the successor of two criteria (not 1:1).</summary>
        public const string Merged = "99.10";

        /// <summary>Only in T.2; new criterion without a predecessor.</summary>
        public const string Added = "99.11";

        /// <summary>Procedure criterion of the Software-Test.</summary>
        public const string ProcedureSoftwareTest = "PROC-softwareTest";

        /// <summary>Procedure criterion of the App-Test.</summary>
        public const string ProcedureAppTest = "PROC-appTest";
    }
}
