// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>
/// <c>catalog import &lt;version&gt;</c> (SPEC R-ADM-01, SD20; CAT-23 import part): validates the
/// release folder as a whole and stores it as a published, immutable release.
/// </summary>
/// <remarks>
/// <para><b>Atomic.</b> Everything is checked before anything is written, and the release is stored
/// with one <c>SaveChanges</c>, which PostgreSQL runs in one transaction: any error leaves the
/// database unchanged.</para>
/// <para><b>What is reused (CAT-07).</b> A criterion by its key; a revision when its content equals
/// the revision the previous release holds for that key (the release stored last); a standard
/// reference by standard, clause and URL; an exclusion rule by source, target and reason. Everything
/// else is new. The release is published at import time (owner, S3-12 decision 1) and stays
/// inactive until <c>catalog activate</c> (S3-17).</para>
/// <para><b>Successors.</b> Each key a criterion lists in <c>supersedes</c> must be a criterion of
/// the previous release; it gets <see cref="Criterion.SupersededByKey"/> (S3-16 decision 4).</para>
/// </remarks>
internal sealed class CatalogReleaseImporter(CatalogDbContext db, TimeProvider timeProvider)
{
    /// <summary>
    /// Imports the folder <paramref name="version"/> below <paramref name="releasesPath"/>. Returns
    /// every error found, or what was stored.
    /// </summary>
    public async Task<CatalogImportResult> ImportAsync(string releasesPath, string version, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releasesPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        if (version is "." or ".." || version.IndexOfAny(['/', '\\']) >= 0)
        {
            return Failed(new ReleaseFolderError(version, "$", "A version is a folder name, not a path."));
        }

        var values = await FilterValues.LoadAsync(db, cancellationToken);
        var folder = ReleaseFolderReader.ReadDirectory(Path.Combine(releasesPath, version), values.Keys);
        if (!folder.IsValid)
        {
            return new CatalogImportResult(null, folder.Errors);
        }

        var definition = folder.Release!;
        if (await db.Releases.AnyAsync(r => r.Version == definition.Version, cancellationToken))
        {
            return Failed(new ReleaseFolderError(
                ReleaseFolderReader.ReleaseFile, "$.version", $"Release {definition.Version} is already imported; a release never changes."));
        }

        var builder = new ReleaseBuilder(definition, values, await StoredCatalog.LoadAsync(db, definition, cancellationToken));
        var release = builder.Build();
        if (builder.Errors.Count > 0)
        {
            // Nothing was saved; drop the changes made in memory (e.g. SupersededByKey).
            db.ChangeTracker.Clear();
            return new CatalogImportResult(null, builder.Errors);
        }

        release.Publish(timeProvider.GetUtcNow());
        db.Releases.Add(release);
        await db.SaveChangesAsync(cancellationToken);

        return new CatalogImportResult(
            new CatalogImportSummary(
                release.Version,
                release.Criteria.Count,
                builder.NewCriteria,
                builder.NewRevisions,
                release.Criteria.Count - builder.NewRevisions,
                builder.PreviousVersion),
            []);
    }

    private static CatalogImportResult Failed(ReleaseFolderError error) => new(null, [error]);

    /// <summary>The seeded filter values, tracked, by layer and key.</summary>
    private sealed class FilterValues
    {
        private readonly Dictionary<(FilterLayer, string), FilterValue> _byKey;

        private FilterValues(List<FilterValue> values)
        {
            _byKey = values.ToDictionary(v => (v.Layer, v.Key));
            Keys = FilterValueKeys.From(values);
        }

        public FilterValueKeys Keys { get; }

        public static async Task<FilterValues> LoadAsync(CatalogDbContext db, CancellationToken cancellationToken)
        {
            var values = new List<FilterValue>();
            values.AddRange(await db.UseCaseTemplates.ToListAsync(cancellationToken));
            values.AddRange(await db.AppTypes.ToListAsync(cancellationToken));
            values.AddRange(await db.ProductParts.ToListAsync(cancellationToken));
            values.AddRange(await db.TestTypes.ToListAsync(cancellationToken));
            return new FilterValues(values);
        }

        public FilterValue Get(FilterValueReference reference) => _byKey[(reference.Layer, reference.Key)];

        public AppType AppType(string key) => (AppType)_byKey[(FilterLayer.AppType, key)];

        public ProductPart ProductPart(string key) => (ProductPart)_byKey[(FilterLayer.ProductPart, key)];

        public UseCaseTemplate UseCase(string key) => (UseCaseTemplate)_byKey[(FilterLayer.UseCase, key)];

        public TestType TestType(string key) => (TestType)_byKey[(FilterLayer.TestType, key)];
    }

    /// <summary>What the database already holds that the new release may reuse, tracked.</summary>
    private sealed record StoredCatalog(
        CatalogRelease? Previous,
        Dictionary<string, Criterion> Criteria,
        Dictionary<int, int> LastRevisionNumbers,
        List<StandardReference> StandardReferences,
        List<FilterExclusion> Exclusions)
    {
        public static async Task<StoredCatalog> LoadAsync(
            CatalogDbContext db,
            ReleaseDefinition definition,
            CancellationToken cancellationToken)
        {
            // The release stored last; the filter values are tracked already, so EF links them.
            var previous = await db.Releases
                .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Criterion)
                .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.StandardReferences)
                .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Applicability)
                .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Priorities)
                .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.ProcedureTexts)
                .AsSplitQuery()
                .OrderByDescending(r => r.PublishedAt).ThenByDescending(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var keys = definition.Criteria.SelectMany(c => c.Supersedes.Append(c.Key)).Distinct().ToList();
            var criteria = await db.Criteria.Where(c => keys.Contains(c.Key)).ToDictionaryAsync(c => c.Key, cancellationToken);
            var criterionIds = criteria.Values.Select(c => c.Id).ToList();
            var lastRevisionNumbers = await db.CriterionRevisions
                .Where(r => criterionIds.Contains(r.CriterionId))
                .GroupBy(r => r.CriterionId)
                .Select(g => new { CriterionId = g.Key, Last = g.Max(r => r.RevisionNumber) })
                .ToDictionaryAsync(x => x.CriterionId, x => x.Last, cancellationToken);

            var clauses = definition.Criteria.SelectMany(c => c.StandardReferences).Select(s => s.Clause).Distinct().ToList();
            var references = await db.StandardReferences.Where(s => clauses.Contains(s.Clause)).ToListAsync(cancellationToken);

            // A handful of rules per release: load them all and match in memory.
            var exclusions = await db.FilterExclusions.ToListAsync(cancellationToken);

            return new StoredCatalog(previous, criteria, lastRevisionNumbers, references, exclusions);
        }
    }

    /// <summary>Builds the release in memory and collects every error on the way.</summary>
    private sealed class ReleaseBuilder(ReleaseDefinition definition, FilterValues values, StoredCatalog stored)
    {
        private readonly Dictionary<string, CriterionRevision> _previousRevisions =
            (stored.Previous?.Criteria ?? []).ToDictionary(c => c.Revision.Criterion.Key, c => c.Revision);

        public List<ReleaseFolderError> Errors { get; } = [];

        public int NewCriteria { get; private set; }

        public int NewRevisions { get; private set; }

        public string? PreviousVersion => stored.Previous?.Version;

        public CatalogRelease Build()
        {
            var release = CatalogRelease.Create(definition.Version, definition.ReleaseNotes);
            foreach (var criterion in definition.Criteria)
            {
                var revision = RevisionOf(criterion);
                if (revision is not null)
                {
                    release.Include(revision);
                }

                MarkPredecessors(criterion);
            }

            foreach (var exclusion in definition.Exclusions)
            {
                release.AddExclusion(ExclusionOf(exclusion));
            }

            foreach (var procedure in definition.Procedures)
            {
                release.MapProcedure(values.AppType(procedure.AppTypeKey), values.TestType(procedure.TestTypeKey));
            }

            return release;
        }

        private CriterionRevision? RevisionOf(CriterionDefinition definition)
        {
            var file = FileOf(definition.Key);
            if (!stored.Criteria.TryGetValue(definition.Key, out var criterion))
            {
                criterion = Criterion.Create(
                    definition.Key,
                    definition.Kind,
                    definition.Category,
                    testType: definition.TestTypeKey is null ? null : values.TestType(definition.TestTypeKey));
                stored.Criteria.Add(definition.Key, criterion);
                NewCriteria++;
            }
            else if (!SameIdentity(criterion, definition, file))
            {
                return null;
            }

            if (_previousRevisions.TryGetValue(definition.Key, out var previous) && RevisionContent.Matches(definition, previous))
            {
                return previous;
            }

            var lastNumber = criterion.Id == 0 ? 0 : stored.LastRevisionNumbers.GetValueOrDefault(criterion.Id);
            var revision = CriterionRevision.Create(
                criterion,
                lastNumber + 1,
                definition.TitleDe,
                definition.PlainLanguageDe,
                definition.RationaleDe,
                definition.SortOrder,
                definition.ShortDescriptionDe);
            foreach (var reference in definition.StandardReferences)
            {
                revision.AddStandardReference(ReferenceOf(reference));
            }

            foreach (var applicability in definition.Applicability)
            {
                revision.AddApplicability(
                    values.AppType(applicability.AppTypeKey),
                    applicability.ProductPartKey is null ? null : values.ProductPart(applicability.ProductPartKey));
            }

            foreach (var priority in definition.Priorities)
            {
                revision.AddPriority(
                    priority.AppTypeKey is null ? null : values.AppType(priority.AppTypeKey),
                    priority.UseCaseKey is null ? null : values.UseCase(priority.UseCaseKey),
                    priority.Level,
                    priority.RationaleDe,
                    priority.Weight);
            }

            foreach (var text in definition.ProcedureTexts)
            {
                revision.AddProcedureText(values.TestType(text.TestTypeKey), text.Content);
            }

            NewRevisions++;
            return revision;
        }

        /// <summary>Key, kind and test type never change (ARCHITECTURE §5.1); nor does the category, which belongs to the criterion.</summary>
        private bool SameIdentity(Criterion criterion, CriterionDefinition definition, string file)
        {
            var same = true;
            if (criterion.Kind != definition.Kind)
            {
                Errors.Add(new ReleaseFolderError(file, "$.kind", $"Criterion '{definition.Key}' is stored as {criterion.Kind}; its kind never changes."));
                same = false;
            }
            else if (criterion.TestType?.Key != definition.TestTypeKey)
            {
                Errors.Add(new ReleaseFolderError(file, "$", $"Criterion '{definition.Key}' is stored for test type '{criterion.TestType?.Key}'; its test type never changes."));
                same = false;
            }

            if (criterion.Category != definition.Category)
            {
                Errors.Add(new ReleaseFolderError(
                    file, "$.category", $"Criterion '{definition.Key}' is stored with category '{criterion.Category ?? "(none)"}'; the category of a stored criterion never changes."));
                same = false;
            }

            return same;
        }

        private void MarkPredecessors(CriterionDefinition definition)
        {
            for (var i = 0; i < definition.Supersedes.Count; i++)
            {
                var oldKey = definition.Supersedes[i];
                var path = $"$.supersedes[{i}]";
                if (!_previousRevisions.TryGetValue(oldKey, out var oldRevision))
                {
                    Errors.Add(new ReleaseFolderError(
                        FileOf(definition.Key),
                        path,
                        stored.Previous is null
                            ? $"'{oldKey}' cannot be replaced: no release is stored yet."
                            : $"'{oldKey}' is not a criterion of the previous release {stored.Previous.Version}."));
                    continue;
                }

                try
                {
                    oldRevision.Criterion.MarkSupersededBy(definition.Key);
                }
                catch (PublishedCatalogChangeException e)
                {
                    Errors.Add(new ReleaseFolderError(FileOf(definition.Key), path, e.Message));
                }
            }
        }

        private StandardReference ReferenceOf(StandardReferenceDefinition definition)
        {
            var reference = stored.StandardReferences.FirstOrDefault(s =>
                s.Standard == definition.Standard && s.Clause == definition.Clause && s.Url == definition.Url);
            if (reference is null)
            {
                reference = StandardReference.Create(definition.Standard, definition.Clause, definition.Url);
                stored.StandardReferences.Add(reference);
            }

            return reference;
        }

        private FilterExclusion ExclusionOf(ExclusionDefinition definition)
        {
            var source = values.Get(definition.Source);
            var target = values.Get(definition.Target);
            var exclusion = stored.Exclusions.FirstOrDefault(e =>
                ReferenceEquals(e.Source, source) && ReferenceEquals(e.Target, target) && e.ReasonDe == definition.ReasonDe);
            if (exclusion is null)
            {
                exclusion = FilterExclusion.Create(source, target, definition.ReasonDe);
                stored.Exclusions.Add(exclusion);
            }

            return exclusion;
        }

        private static string FileOf(string key) => $"{ReleaseFolderReader.CriteriaFolder}{key}.json";
    }
}

/// <summary>What <c>catalog import</c> stored, for the command's output.</summary>
internal sealed record CatalogImportSummary(
    string Version,
    int Criteria,
    int NewCriteria,
    int NewRevisions,
    int SharedRevisions,
    string? PreviousVersion);

/// <summary>Either what was stored or every error; never both.</summary>
internal sealed record CatalogImportResult(CatalogImportSummary? Summary, IReadOnlyList<ReleaseFolderError> Errors)
{
    public bool Succeeded => Summary is not null;
}
