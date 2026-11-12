// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.RegularExpressions;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>
/// The rules a release folder must meet before it may be imported (CAT-23, S3-15): file layout,
/// JSON shape, references to filter values, exclusion layers (CAT-13), procedure mapping, allowed
/// ratings, the rules of procedure criteria (C30), successors, and the length limits of the
/// database. It collects every error instead of stopping at the first, so one run shows the project
/// team everything to fix.
/// </summary>
/// <remarks>
/// <para>Consistency only. Completeness — a priority for every requirement criterion, a procedure
/// criterion for every test type — is checked when a release is activated (O-S3-5 Q2 (iii),
/// S3-17), so a release without priorities can be imported and reviewed.</para>
/// <para>Rules the domain also holds (layer order, one rule per pair …) are checked here too, so
/// that the import never meets a domain exception it cannot point to a file for.</para>
/// </remarks>
internal sealed partial class ReleaseFolderValidator
{
    private static readonly Dictionary<string, CriterionKind> Kinds = new(StringComparer.Ordinal)
    {
        ["requirement"] = CriterionKind.Requirement,
        ["testProcedure"] = CriterionKind.TestProcedure,
    };

    private static readonly Dictionary<string, FilterLayer> Layers = new(StringComparer.Ordinal)
    {
        ["useCase"] = FilterLayer.UseCase,
        ["appType"] = FilterLayer.AppType,
        ["productPart"] = FilterLayer.ProductPart,
        ["testType"] = FilterLayer.TestType,
    };

    private static readonly Dictionary<string, Standard> Standards = new(StringComparer.Ordinal)
    {
        ["en301549"] = Standard.En301549,
        ["wcag"] = Standard.Wcag,
        ["bitInklusiv"] = Standard.BitInklusiv,
        ["iso9241"] = Standard.Iso9241,
    };

    private static readonly Dictionary<string, PriorityLevel> Levels = new(StringComparer.Ordinal)
    {
        ["A"] = PriorityLevel.A,
        ["B"] = PriorityLevel.B,
        ["Info"] = PriorityLevel.Info,
    };

    private static readonly Dictionary<string, Rating> Ratings = new(StringComparer.Ordinal)
    {
        ["fulfilled"] = Rating.Fulfilled,
        ["slightRestriction"] = Rating.SlightRestriction,
        ["restriction"] = Rating.Restriction,
        ["barrier"] = Rating.Barrier,
        ["blockade"] = Rating.Blockade,
        ["notApplicable"] = Rating.NotApplicable,
    };

    private readonly string _folderName;
    private readonly IReadOnlyDictionary<string, string> _files;
    private readonly FilterValueKeys _filterValues;
    private readonly List<ReleaseFolderError> _errors = [];

    /// <summary>
    /// The URL of each cited (standard, clause) and the file that cited it first: the database keeps
    /// one row per (standard, clause), shared by every revision that cites it.
    /// </summary>
    private readonly Dictionary<(Standard Standard, string Clause), (string? Url, string File)> _citedReferences = [];

    public ReleaseFolderValidator(string folderName, IReadOnlyDictionary<string, string> files, FilterValueKeys filterValues)
    {
        _folderName = folderName;
        _files = files;
        _filterValues = filterValues;
    }

    public ReleaseFolderResult Validate()
    {
        CheckLayout();
        var release = ValidateRelease();
        var exclusions = ValidateExclusions();
        var (procedures, mappingIsValid) = ValidateProcedureMapping();
        var criteria = ValidateCriteria(procedures, mappingIsValid);

        if (_errors.Count > 0 || release is null)
        {
            return new ReleaseFolderResult(null, _errors);
        }

        var definition = new ReleaseDefinition(release.Version!, release.ReleaseNotes, criteria, exclusions, procedures);
        return new ReleaseFolderResult(definition, []);
    }

    /// <summary>
    /// EN 301 549 numbers as Kriterienkatalog Teil A/B writes them (<c>5.2</c>,
    /// <c>11.1.3.1c</c>), PDF/UA-1 clauses with a hyphen (<c>UA1-7.10</c>) and procedure criteria
    /// <c>PROC-&lt;testTypeKey&gt;</c> (owner, S3-15 decision 1a).
    /// </summary>
    [GeneratedRegex(@"^(?:\d+(?:\.\d+)*[a-z]?|UA1-\d+(?:\.\d+)*|PROC-[A-Za-z]+)$", RegexOptions.CultureInvariant)]
    private static partial Regex CriterionKeyPattern();

    private void CheckLayout()
    {
        foreach (var path in _files.Keys.Order(StringComparer.Ordinal))
        {
            var isKnown = path is ReleaseFolderReader.ReleaseFile or ReleaseFolderReader.ExclusionsFile
                or ReleaseFolderReader.ProcedureMappingFile;
            if (!isKnown && !IsCriterionFile(path))
            {
                Error(
                    path,
                    "$",
                    "Unexpected file. A release folder holds release.json, exclusions.json, procedure-mapping.json and criteria/<key>.json.");
            }
        }

        foreach (var required in new[] { ReleaseFolderReader.ReleaseFile, ReleaseFolderReader.ExclusionsFile, ReleaseFolderReader.ProcedureMappingFile })
        {
            if (!_files.ContainsKey(required))
            {
                Error(required, "$", "The file is missing.");
            }
        }

        if (!CriterionFiles().Any())
        {
            Error(ReleaseFolderReader.CriteriaFolder, "$", "The release has no criteria.");
        }
    }

    /// <summary>The criterion files, ordered by key (ordinal).</summary>
    private IEnumerable<string> CriterionFiles() => _files.Keys.Where(IsCriterionFile).Order(StringComparer.Ordinal);

    private static bool IsCriterionFile(string path) =>
        path.StartsWith(ReleaseFolderReader.CriteriaFolder, StringComparison.Ordinal)
        && path.EndsWith(".json", StringComparison.Ordinal)
        && !path[ReleaseFolderReader.CriteriaFolder.Length..].Contains('/', StringComparison.Ordinal);

    private ReleaseJson? ValidateRelease()
    {
        const string file = ReleaseFolderReader.ReleaseFile;
        var release = Parse<ReleaseJson>(file);
        if (release is null)
        {
            return null;
        }

        var version = RequiredText(file, "$.version", release.Version, CatalogRelease.VersionMaxLength);
        if (version is not null && version != _folderName)
        {
            Error(file, "$.version", $"The version '{version}' must equal the folder name '{_folderName}'.");
        }

        OptionalText(file, "$.releaseNotes", release.ReleaseNotes, maxLength: null);
        return version is null ? null : release;
    }

    private List<ExclusionDefinition> ValidateExclusions()
    {
        const string file = ReleaseFolderReader.ExclusionsFile;
        var result = new List<ExclusionDefinition>();
        var json = Parse<ExclusionsJson>(file);
        if (json is null)
        {
            return result;
        }

        if (json.Exclusions is null)
        {
            Error(file, "$.exclusions", "The list is missing; write [] for a release without exclusions.");
            return result;
        }

        var seen = new HashSet<(FilterValueReference, FilterValueReference)>();
        for (var i = 0; i < json.Exclusions.Count; i++)
        {
            var path = $"$.exclusions[{i}]";
            var exclusion = json.Exclusions[i];
            if (exclusion is null)
            {
                Error(file, path, "The entry is empty.");
                continue;
            }

            var source = FilterValue(file, $"{path}.source", exclusion.Source);
            var target = FilterValue(file, $"{path}.target", exclusion.Target);
            var reason = RequiredText(file, $"{path}.reasonDe", exclusion.ReasonDe, FilterExclusion.ReasonMaxLength);
            if (source is null || target is null || reason is null)
            {
                continue;
            }

            if (source.Layer == FilterLayer.TestType)
            {
                Error(file, $"{path}.source.layer", "A test type is the lowest layer and cannot exclude anything.");
            }
            else if (target.Layer <= source.Layer)
            {
                // CAT-13: the same rule as FilterExclusion.Create and the CHECK constraint.
                Error(
                    file,
                    $"{path}.target.layer",
                    $"An exclusion must point to a lower layer: {source.Layer} '{source.Key}' cannot exclude {target.Layer} '{target.Key}'.");
            }
            else if (!seen.Add((source, target)))
            {
                Error(file, path, $"The exclusion {source.Layer} '{source.Key}' → {target.Layer} '{target.Key}' is listed twice.");
            }
            else
            {
                result.Add(new ExclusionDefinition(source, target, reason));
            }
        }

        return result;
    }

    private FilterValueReference? FilterValue(string file, string path, FilterValueReferenceJson? value)
    {
        if (value is null)
        {
            Error(file, path, "The value is missing.");
            return null;
        }

        var layerName = RequiredText(file, $"{path}.layer", value.Layer, maxLength: null);
        var key = RequiredText(file, $"{path}.key", value.Key, maxLength: null);
        if (layerName is null || key is null)
        {
            return null;
        }

        if (!Layers.TryGetValue(layerName, out var layer))
        {
            Error(file, $"{path}.layer", $"Unknown layer '{layerName}'; allowed: {string.Join(", ", Layers.Keys)}.");
            return null;
        }

        return Known(file, $"{path}.key", layer, key) ? new FilterValueReference(layer, key) : null;
    }

    private (List<ProcedureMappingDefinition> Procedures, bool IsValid) ValidateProcedureMapping()
    {
        const string file = ReleaseFolderReader.ProcedureMappingFile;
        var result = new List<ProcedureMappingDefinition>();
        var errorsBefore = _errors.Count;
        var json = Parse<ProcedureMappingJson>(file);
        if (json is null)
        {
            return (result, false);
        }

        if (json.Procedures is null)
        {
            Error(file, "$.procedures", "The list is missing; write [] for a release without procedure mapping.");
            return (result, false);
        }

        var appTypes = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < json.Procedures.Count; i++)
        {
            var path = $"$.procedures[{i}]";
            var entry = json.Procedures[i];
            if (entry is null)
            {
                Error(file, path, "The entry is empty.");
                continue;
            }

            var appType = RequiredText(file, $"{path}.appType", entry.AppType, maxLength: null);
            if (appType is not null && Known(file, $"{path}.appType", FilterLayer.AppType, appType) && !appTypes.Add(appType))
            {
                Error(file, $"{path}.appType", $"The app type '{appType}' has more than one entry; list all its test types in one.");
            }

            var testTypes = KeyList(file, $"{path}.testTypes", entry.TestTypes, FilterLayer.TestType, required: true);
            if (appType is not null)
            {
                result.AddRange(testTypes.Select(testType => new ProcedureMappingDefinition(appType, testType)));
            }
        }

        return (result, _errors.Count == errorsBefore);
    }

    private List<CriterionDefinition> ValidateCriteria(List<ProcedureMappingDefinition> procedures, bool mappingIsValid)
    {
        var result = new List<CriterionDefinition>();
        var keys = CriterionFiles().Select(KeyOf).ToHashSet(StringComparer.Ordinal);
        var successors = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in CriterionFiles())
        {
            var criterion = ValidateCriterion(file, KeyOf(file), keys, successors, procedures, mappingIsValid);
            if (criterion is not null)
            {
                result.Add(criterion);
            }
        }

        return result;
    }

    private static string KeyOf(string file) =>
        file[ReleaseFolderReader.CriteriaFolder.Length..^".json".Length];

    private CriterionDefinition? ValidateCriterion(
        string file,
        string key,
        HashSet<string> releaseKeys,
        Dictionary<string, string> successors,
        List<ProcedureMappingDefinition> procedures,
        bool mappingIsValid)
    {
        var errorsBefore = _errors.Count;
        if (key.Length > Criterion.KeyMaxLength || !CriterionKeyPattern().IsMatch(key))
        {
            Error(
                file,
                "$",
                $"The file name '{key}' is not a criterion key: an EN 301 549 number such as 11.1.3.1c, a PDF/UA-1 clause such as UA1-7.10, or PROC-<testType>.");
        }

        var json = Parse<CriterionJson>(file);
        if (json is null)
        {
            return null;
        }

        var kind = Kind(file, json);
        var testType = ValidateTestType(file, key, kind, json);
        var category = OptionalText(file, "$.category", json.Category, Criterion.CategoryMaxLength);
        var supersedes = ValidateSupersedes(file, key, json.Supersedes, releaseKeys, successors);
        var title = RequiredText(file, "$.titleDe", json.TitleDe, maxLength: null);
        var plainLanguage = OptionalText(file, "$.plainLanguageDe", json.PlainLanguageDe, maxLength: null);
        var shortDescription = OptionalText(file, "$.shortDescriptionDe", json.ShortDescriptionDe, maxLength: null);
        var rationale = Lines(file, "$.rationaleDe", json.RationaleDe);
        if (json.SortOrder is null)
        {
            Error(file, "$.sortOrder", "The value is missing.");
        }

        var references = ValidateStandardReferences(file, json.StandardReferences);
        var errorsBeforeApplicability = _errors.Count;
        var applicability = ValidateApplicability(file, kind, json.Applicability);
        var applicabilityIsValid = _errors.Count == errorsBeforeApplicability;
        var priorities = ValidatePriorities(file, json.Priorities);

        // An unknown app type is already reported; checking the texts against the rest would add a misleading error.
        var texts = ValidateProcedureTexts(
            file, kind, json.ProcedureTexts, applicability, procedures, mappingIsValid && applicabilityIsValid);

        if (_errors.Count > errorsBefore || kind is null || title is null)
        {
            return null;
        }

        return new CriterionDefinition(
            key, kind.Value, testType, category, supersedes, title, plainLanguage, shortDescription, rationale,
            json.SortOrder!.Value, references, applicability, priorities, texts);
    }

    private CriterionKind? Kind(string file, CriterionJson json)
    {
        var name = RequiredText(file, "$.kind", json.Kind, maxLength: null);
        if (name is null)
        {
            return null;
        }

        if (Kinds.TryGetValue(name, out var kind))
        {
            return kind;
        }

        Error(file, "$.kind", $"Unknown kind '{name}'; allowed: {string.Join(", ", Kinds.Keys)}.");
        return null;
    }

    /// <summary>A procedure criterion names its test type and is keyed by it (ARCHITECTURE §5.1, C30); a requirement has none.</summary>
    private string? ValidateTestType(string file, string key, CriterionKind? kind, CriterionJson json)
    {
        var isProcedureKey = key.StartsWith("PROC-", StringComparison.Ordinal);
        if (kind == CriterionKind.Requirement)
        {
            if (json.TestType is not null)
            {
                Error(file, "$.testType", "Only a procedure criterion (kind testProcedure) names a test type.");
            }

            if (isProcedureKey)
            {
                Error(file, "$.kind", "A key PROC-<testType> belongs to a procedure criterion (kind testProcedure).");
            }

            return null;
        }

        if (kind != CriterionKind.TestProcedure)
        {
            return null;
        }

        var testType = RequiredText(file, "$.testType", json.TestType, maxLength: null);
        if (testType is null || !Known(file, "$.testType", FilterLayer.TestType, testType))
        {
            return null;
        }

        if (key != $"PROC-{testType}")
        {
            Error(file, "$", $"A procedure criterion of test type '{testType}' must be named PROC-{testType}.json.");
        }

        return testType;
    }

    /// <summary>
    /// Owner, S3-15 decision 3a: the new criterion names the ones it replaces. One old key has one
    /// successor (<c>Criterion.SupersededByKey</c>), so a split is refused until S3-14 P2 is decided.
    /// </summary>
    private List<string> ValidateSupersedes(
        string file,
        string key,
        List<string?>? supersedes,
        HashSet<string> releaseKeys,
        Dictionary<string, string> successors)
    {
        var result = new List<string>();
        if (supersedes is null)
        {
            return result;
        }

        for (var i = 0; i < supersedes.Count; i++)
        {
            var path = $"$.supersedes[{i}]";
            var old = RequiredText(file, path, supersedes[i], maxLength: null);
            if (old is null)
            {
                continue;
            }

            if (!CriterionKeyPattern().IsMatch(old) || old.Length > Criterion.KeyMaxLength)
            {
                Error(file, path, $"'{old}' is not a criterion key.");
            }
            else if (releaseKeys.Contains(old))
            {
                Error(file, path, $"'{old}' is a criterion of this release; a criterion can only replace one that the release no longer contains.");
            }
            else if (successors.TryGetValue(old, out var other))
            {
                Error(
                    file,
                    path,
                    other == key
                        ? $"'{old}' is listed twice."
                        : $"'{old}' is already replaced by '{other}'; a criterion has one successor (a split cannot be stored yet).");
            }
            else
            {
                successors[old] = key;
                result.Add(old);
            }
        }

        return result;
    }

    private List<StandardReferenceDefinition> ValidateStandardReferences(string file, List<StandardReferenceJson?>? references)
    {
        var result = new List<StandardReferenceDefinition>();
        for (var i = 0; i < (references?.Count ?? 0); i++)
        {
            var path = $"$.standardReferences[{i}]";
            var reference = references![i];
            if (reference is null)
            {
                Error(file, path, "The entry is empty.");
                continue;
            }

            var standardName = RequiredText(file, $"{path}.standard", reference.Standard, maxLength: null);
            var clause = RequiredText(file, $"{path}.clause", reference.Clause, StandardReference.ClauseMaxLength);
            var url = reference.Url is null ? null : Url(file, $"{path}.url", reference.Url, StandardReference.UrlMaxLength);
            Standard? standard = null;
            if (standardName is not null)
            {
                if (Standards.TryGetValue(standardName, out var known))
                {
                    standard = known;
                }
                else
                {
                    Error(file, $"{path}.standard", $"Unknown standard '{standardName}'; allowed: {string.Join(", ", Standards.Keys)}.");
                }
            }

            if (standard is null || clause is null)
            {
                continue;
            }

            var definition = new StandardReferenceDefinition(standard.Value, clause, url);
            if (result.Contains(definition))
            {
                Error(file, path, "The reference is listed twice.");
            }
            else if (_citedReferences.TryGetValue((standard.Value, clause), out var cited) && cited.Url != url)
            {
                Error(
                    file,
                    path,
                    $"{standardName} {clause} is cited with another URL in {cited.File} ('{cited.Url ?? "(none)"}'); one clause has one URL.");
            }
            else
            {
                _citedReferences.TryAdd((standard.Value, clause), (url, file));
                result.Add(definition);
            }
        }

        return result;
    }

    private List<ApplicabilityDefinition> ValidateApplicability(string file, CriterionKind? kind, List<ApplicabilityJson?>? entries)
    {
        var result = new List<ApplicabilityDefinition>();
        if (kind == CriterionKind.TestProcedure)
        {
            if (entries is { Count: > 0 })
            {
                Error(file, "$.applicability", "A procedure criterion has no applicability: it enters a selection through its test type.");
            }

            return result;
        }

        if (entries is null || entries.Count == 0)
        {
            if (kind == CriterionKind.Requirement)
            {
                Error(file, "$.applicability", "A requirement criterion needs at least one app type it applies to.");
            }

            return result;
        }

        for (var i = 0; i < entries.Count; i++)
        {
            var path = $"$.applicability[{i}]";
            var entry = entries[i];
            if (entry is null)
            {
                Error(file, path, "The entry is empty.");
                continue;
            }

            var appTypes = KeyList(file, $"{path}.appTypes", entry.AppTypes, FilterLayer.AppType, required: true);
            List<string?> productParts = entry.ProductParts is null
                ? [null]
                : [.. KeyList(file, $"{path}.productParts", entry.ProductParts, FilterLayer.ProductPart, required: true)];
            foreach (var appType in appTypes)
            {
                foreach (var productPart in productParts)
                {
                    var definition = new ApplicabilityDefinition(appType, productPart);
                    if (result.Contains(definition))
                    {
                        Error(file, path, $"App type '{appType}' with product part '{productPart ?? "(any)"}' is listed twice.");
                    }
                    else
                    {
                        result.Add(definition);
                    }
                }
            }
        }

        return result;
    }

    private List<PriorityDefinition> ValidatePriorities(string file, List<PriorityJson?>? priorities)
    {
        var result = new List<PriorityDefinition>();
        for (var i = 0; i < (priorities?.Count ?? 0); i++)
        {
            var path = $"$.priorities[{i}]";
            var priority = priorities![i];
            if (priority is null)
            {
                Error(file, path, "The entry is empty.");
                continue;
            }

            var appTypeIsKnown = priority.AppType is null || Known(file, $"{path}.appType", FilterLayer.AppType, priority.AppType);
            var useCaseIsKnown = priority.UseCase is null || Known(file, $"{path}.useCase", FilterLayer.UseCase, priority.UseCase);
            var levelName = RequiredText(file, $"{path}.level", priority.Level, maxLength: null);
            var rationale = RequiredText(file, $"{path}.rationaleDe", priority.RationaleDe, PriorityRecommendation.RationaleMaxLength);
            PriorityLevel? level = null;
            if (levelName is not null)
            {
                if (Levels.TryGetValue(levelName, out var known))
                {
                    level = known;
                }
                else
                {
                    Error(file, $"{path}.level", $"Unknown level '{levelName}'; allowed: {string.Join(", ", Levels.Keys)}.");
                }
            }

            if (!appTypeIsKnown || !useCaseIsKnown || level is null || rationale is null)
            {
                continue;
            }

            if (result.Any(p => p.AppTypeKey == priority.AppType && p.UseCaseKey == priority.UseCase))
            {
                Error(
                    file,
                    path,
                    $"There is already a rule for app type '{priority.AppType ?? "(any)"}' and use case '{priority.UseCase ?? "(any)"}'.");
                continue;
            }

            result.Add(new PriorityDefinition(priority.AppType, priority.UseCase, level.Value, priority.Weight, rationale));
        }

        return result;
    }

    private List<ProcedureTextDefinition> ValidateProcedureTexts(
        string file,
        CriterionKind? kind,
        List<ProcedureTextJson?>? texts,
        List<ApplicabilityDefinition> applicability,
        List<ProcedureMappingDefinition> procedures,
        bool canCheckMapping)
    {
        var result = new List<ProcedureTextDefinition>();
        if (kind == CriterionKind.TestProcedure && texts is { Count: > 0 })
        {
            Error(file, "$.procedureTexts", "A procedure criterion has no procedure texts.");
            return result;
        }

        for (var i = 0; i < (texts?.Count ?? 0); i++)
        {
            var path = $"$.procedureTexts[{i}]";
            var text = texts![i];
            if (text is null)
            {
                Error(file, path, "The entry is empty.");
                continue;
            }

            var errorsBefore = _errors.Count;
            var testType = RequiredText(file, $"{path}.testType", text.TestType, maxLength: null);
            if (testType is not null && Known(file, $"{path}.testType", FilterLayer.TestType, testType))
            {
                if (result.Any(t => t.TestTypeKey == testType))
                {
                    Error(file, $"{path}.testType", $"There is already a text for test type '{testType}'.");
                }
                else if (canCheckMapping && !procedures.Any(p => p.TestTypeKey == testType && applicability.Any(a => a.AppTypeKey == p.AppTypeKey)))
                {
                    // The text would never be shown: no app type of this criterion uses the procedure (CAT-21).
                    Error(
                        file,
                        $"{path}.testType",
                        $"No app type this criterion applies to is mapped to test type '{testType}' in procedure-mapping.json.");
                }
            }

            var number = RequiredText(file, $"{path}.originalNumber", text.OriginalNumber, CriterionProcedureText.OriginalNumberMaxLength);
            var title = RequiredText(file, $"{path}.originalTitleDe", text.OriginalTitleDe, CriterionProcedureText.OriginalTitleMaxLength);
            var sourceUrl = text.SourceUrl is null
                ? MissingValue(file, $"{path}.sourceUrl")
                : Url(file, $"{path}.sourceUrl", text.SourceUrl, CriterionProcedureText.SourceUrlMaxLength);
            var ratings = AllowedRatings(file, $"{path}.allowedRatings", text.AllowedRatings);
            var content = new ProcedureTextContent(
                number ?? string.Empty,
                title ?? string.Empty,
                ratings,
                sourceUrl ?? string.Empty,
                Lines(file, $"{path}.applicabilityDe", text.ApplicabilityDe),
                Lines(file, $"{path}.requirementDe", text.RequirementDe),
                Lines(file, $"{path}.rationaleWhyDe", text.RationaleWhyDe),
                Lines(file, $"{path}.positiveExamplesDe", text.PositiveExamplesDe),
                Lines(file, $"{path}.negativeExamplesDe", text.NegativeExamplesDe));
            if (_errors.Count == errorsBefore)
            {
                result.Add(new ProcedureTextDefinition(testType!, content));
            }
        }

        return result;
    }

    /// <summary>At least one of the six levels, each once (C31; „Nicht eingeschätzt“ is always allowed and never listed).</summary>
    private Rating AllowedRatings(string file, string path, List<string?>? names)
    {
        if (names is null || names.Count == 0)
        {
            Error(file, path, $"Name at least one allowed rating: {string.Join(", ", Ratings.Keys)}.");
            return Rating.None;
        }

        var ratings = Rating.None;
        for (var i = 0; i < names.Count; i++)
        {
            var name = names[i];
            if (name is null || !Ratings.TryGetValue(name, out var rating))
            {
                Error(file, $"{path}[{i}]", $"Unknown rating '{name}'; allowed: {string.Join(", ", Ratings.Keys)}.");
            }
            else if ((ratings & rating) != 0)
            {
                Error(file, $"{path}[{i}]", $"The rating '{name}' is listed twice.");
            }
            else
            {
                ratings |= rating;
            }
        }

        return ratings;
    }

    /// <summary>A list of filter-value keys of one layer, each known and listed once.</summary>
    private List<string> KeyList(string file, string path, List<string?>? keys, FilterLayer layer, bool required)
    {
        var result = new List<string>();
        if (keys is null || keys.Count == 0)
        {
            if (required)
            {
                Error(file, path, "Name at least one value.");
            }

            return result;
        }

        for (var i = 0; i < keys.Count; i++)
        {
            var key = RequiredText(file, $"{path}[{i}]", keys[i], maxLength: null);
            if (key is null || !Known(file, $"{path}[{i}]", layer, key))
            {
                continue;
            }

            if (result.Contains(key, StringComparer.Ordinal))
            {
                Error(file, $"{path}[{i}]", $"'{key}' is listed twice.");
            }
            else
            {
                result.Add(key);
            }
        }

        return result;
    }

    private bool Known(string file, string path, FilterLayer layer, string key)
    {
        if (_filterValues.Contains(layer, key))
        {
            return true;
        }

        Error(file, path, $"Unknown {layer} '{key}'. Filter values are seeded by the catalog migration; use one of their keys.");
        return false;
    }

    private T? Parse<T>(string file)
        where T : class
    {
        if (!_files.TryGetValue(file, out var content))
        {
            return null;
        }

        try
        {
            var value = JsonSerializer.Deserialize<T>(content, ReleaseFolderJson.Options);
            if (value is null)
            {
                Error(file, "$", "The file must hold a JSON object.");
            }

            return value;
        }
        catch (JsonException exception)
        {
            var line = exception.LineNumber is { } number ? $" (line {number + 1})" : string.Empty;
            Error(file, exception.Path ?? "$", $"Invalid JSON{line}: {FirstSentence(exception.Message)}");
            return null;
        }
    }

    /// <summary>System.Text.Json repeats path and position after the first sentence; the error names them already.</summary>
    private static string FirstSentence(string message)
    {
        var end = message.IndexOf(" Path:", StringComparison.Ordinal);
        return end > 0 ? message[..end] : message;
    }

    private string? RequiredText(string file, string path, string? value, int? maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Error(file, path, "The value is missing.");
            return null;
        }

        return OptionalText(file, path, value, maxLength);
    }

    private string? OptionalText(string file, string path, string? value, int? maxLength)
    {
        if (value is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            Error(file, path, "The text is empty; leave the property out instead.");
            return null;
        }

        if (value != value.Trim())
        {
            Error(file, path, "The text starts or ends with white space.");
            return null;
        }

        if (maxLength is not null && value.Length > maxLength)
        {
            Error(file, path, $"The text has {value.Length} characters; at most {maxLength} are allowed.");
            return null;
        }

        return value;
    }

    /// <summary>
    /// A long text as an array of Markdown lines, one line per element (paragraphs, list items), so
    /// a pull request shows changes line by line; joined with <c>\n</c>. Empty lines separate
    /// paragraphs, but a text neither starts nor ends with one.
    /// </summary>
    private string? Lines(string file, string path, List<string?>? lines)
    {
        if (lines is null)
        {
            return null;
        }

        if (lines.Count == 0)
        {
            Error(file, path, "The text has no lines; leave the property out instead.");
            return null;
        }

        var isValid = true;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line is null || line.Contains('\n', StringComparison.Ordinal) || line.Contains('\r', StringComparison.Ordinal))
            {
                Error(file, $"{path}[{i}]", "Each element is one line of text (no null, no line break).");
                isValid = false;
            }
        }

        if (!isValid)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(lines[0]) || string.IsNullOrWhiteSpace(lines[^1]))
        {
            Error(file, path, "The text starts or ends with an empty line.");
            return null;
        }

        return string.Join('\n', lines);
    }

    private string? Url(string file, string path, string value, int maxLength)
    {
        if (value != value.Trim() || !Uri.TryCreate(value, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            Error(file, path, $"'{value}' is not an absolute http(s) URL.");
            return null;
        }

        if (value.Length > maxLength)
        {
            Error(file, path, $"The URL has {value.Length} characters; at most {maxLength} are allowed.");
            return null;
        }

        return value;
    }

    private string? MissingValue(string file, string path)
    {
        Error(file, path, "The value is missing.");
        return null;
    }

    private void Error(string file, string path, string message) => _errors.Add(new ReleaseFolderError(file, path, message));
}
