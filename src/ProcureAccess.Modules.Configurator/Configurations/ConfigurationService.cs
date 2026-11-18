// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Drafts;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Configurator.Drafts;
using ProcureAccess.Modules.Configurator.Persistence;

namespace ProcureAccess.Modules.Configurator.Configurations;

/// <summary>
/// Saves, reads and lists the organisation's criteria lists (R-SAV-01, R-SAV-02; S5-20a), changes a
/// decision or the selection, deletes a list and copies it to the active release (R-SAV-03,
/// R-SAV-06, R-LST-14; S5-21a) for <see cref="ConfigurationEndpoints"/>. Every read runs through the
/// organisation filter of the request (TEN-01); the endpoints check the role on the list's
/// organisation before they call it.
/// </summary>
internal sealed class ConfigurationService(
    ConfiguratorDbContext db,
    IConfigurationDraftCheck drafts,
    ICatalogSelectionResolver catalog,
    ICatalogReleaseCarryOver carryOver,
    PriorityWeightRange weights,
    TimeProvider clock)
{
    private const string SelectionField = "selection";
    private const string ReleaseField = "selection.releaseVersion";
    private const string UseCaseField = "selection.useCaseTemplateId";

    /// <summary>The draft check for a new list: the export's rules, and an inactive use case is refused (CAT-18).</summary>
    public Task<DraftCheckResult> CheckAsync(CriteriaDraft draft, CancellationToken cancellationToken) =>
        drafts.CheckAsync(draft, DraftPurpose.Save, cancellationToken);

    /// <summary>
    /// Stores a checked draft as a new list of <paramref name="organizationId"/> and returns its id.
    /// The recommended level of each criterion is the one the catalog resolved now, never the
    /// client's (EXP-07); it stays as it is from here on (CAT-08).
    /// </summary>
    public async Task<Guid> CreateAsync(
        Guid organizationId, string name, string createdByUserId, CheckedDraft draft, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var list = CriteriaConfiguration.Create(
            organizationId,
            name,
            createdByUserId,
            clock.GetUtcNow(),
            draft.ReleaseVersion,
            draft.Selection.UseCase?.Key,
            [.. draft.Selection.AppTypes.Select(v => v.Key)],
            [.. draft.Selection.ProductParts.Select(v => v.Key)],
            [.. draft.Selection.TestTypes.Select(v => v.Key)],
            [.. draft.Criteria.Select(c => new ConfigurationCriterionDraft(
                c.Criterion.RevisionId,
                c.Criterion.RecommendedPriority.Level,
                c.IsIncluded,
                c.OverrideLevel,
                c.OverrideWeight,
                c.Note))]);
        db.CriteriaConfigurations.Add(list);
        await db.SaveChangesAsync(cancellationToken);
        return list.Id;
    }

    /// <summary>
    /// The list with this id, or <c>null</c> when it does not exist for the request (also another
    /// organisation's: 404, TEN-01). The decisions come by revision id; the order of the list is the
    /// catalog's, which S11 takes from the criteria endpoint of the pinned release.
    /// </summary>
    public async Task<ConfigurationResponse?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var list = await db.CriteriaConfigurations.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        return list is null
            ? null
            : new ConfigurationResponse(
                list.Id,
                list.OrganizationId,
                list.Name,
                list.ReleaseVersion,
                list.CreatedAt,
                list.ModifiedAt,
                list.ProcurementProjectId,
                SelectionOf(list),
                DecisionsOf(list));
    }

    /// <summary>The selection of a list by the value keys of its release, as S11 reads it.</summary>
    internal static ConfigurationSelectionResponse SelectionOf(CriteriaConfiguration list) =>
        new(
            list.UseCaseKey,
            [.. list.AppTypes.Select(v => v.AppTypeKey)],
            [.. list.ProductParts.Select(v => v.ProductPartKey)],
            [.. list.TestTypes.Select(v => v.TestTypeKey)]);

    /// <summary>The decision on each criterion of a list, by revision id, as S11 reads them.</summary>
    internal static IReadOnlyList<ConfigurationCriterionResponse> DecisionsOf(CriteriaConfiguration list) =>
        [.. list.Criteria.OrderBy(c => c.CriterionRevisionId).Select(c => new ConfigurationCriterionResponse(
            c.CriterionRevisionId,
            c.IsIncluded,
            LevelName(c.ResolvedLevel),
            c.OverrideLevel is { } level ? LevelName(level) : null,
            c.OverrideWeight,
            c.NoteDe))];

    /// <summary>
    /// The lists of <paramref name="organizationId"/>, the most recently changed first; the client
    /// sorts them (S14). Only the columns of the list, none of its rows. The copies the projects own
    /// are not listed (R-SAV-02, R-PRJ-02: never offered as a source, WSP-02).
    /// </summary>
    public async Task<IReadOnlyList<ConfigurationSummaryResponse>> ListAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await db.CriteriaConfigurations.AsNoTracking()
                .Where(c => c.OrganizationId == organizationId && c.ProcurementProjectId == null)
                .OrderByDescending(c => c.ModifiedAt)
                .ThenBy(c => c.Id)
                .Select(c => new ConfigurationSummaryResponse(c.Id, c.Name, c.ReleaseVersion, c.CreatedAt, c.ModifiedAt))
                .ToListAsync(cancellationToken);

    /// <summary>The organisation of the list, or <c>null</c> when it does not exist for the request (TEN-01).</summary>
    public async Task<Guid?> OrganizationOfAsync(Guid id, CancellationToken cancellationToken) =>
        (await OwnerOfAsync(id, cancellationToken))?.OrganizationId;

    /// <summary>
    /// The organisation of the list and whether it is a project's copy, or <c>null</c> when it does
    /// not exist for the request (TEN-01).
    /// </summary>
    public async Task<ListOwner?> OwnerOfAsync(Guid id, CancellationToken cancellationToken) =>
        await db.CriteriaConfigurations.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new ListOwner(c.OrganizationId, c.ProcurementProjectId != null))
                .SingleOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Replaces the decision on one criterion of the list (R-SAV-03): the rules of a saved row —
    /// level name, the 🔧 weight range and B only, the note limit —, keyed by the body's members.
    /// <see cref="EditOutcome.NotFound"/> for an unknown list or a criterion that is not on it.
    /// Also on a list of an older release: its criteria stay editable (owner decision S5-21 5a), and
    /// on a project's copy (C51, WSP-03).
    /// </summary>
    public async Task<EditResult<bool>> DecideAsync(
        Guid id, int criterionRevisionId, ConfigurationDecisionBody body, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);
        var list = await db.CriteriaConfigurations.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        var row = list?.Criteria.SingleOrDefault(c => c.CriterionRevisionId == criterionRevisionId);
        if (list is null || row is null)
        {
            return EditResult<bool>.NotFound();
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (body.IsIncluded is null)
        {
            errors["isIncluded"] = ["Say whether the criterion is included."];
        }

        PriorityLevel? level = null;
        if (body.OverrideLevel is not null && (level = ConfigurationDraftCheck.LevelOf(body.OverrideLevel)) is null)
        {
            errors["overrideLevel"] = ["The level must be A, B or Info."];
        }

        if (body.OverrideWeight is { } weight)
        {
            if (!weights.Contains(weight))
            {
                errors["overrideWeight"] = [$"The weight must be between {weights.Min} and {weights.Max}."];
            }
            else if (!errors.ContainsKey("overrideLevel") && (level ?? row.ResolvedLevel) != PriorityLevel.B)
            {
                errors["overrideWeight"] = ["A weight applies only while the level is B."];
            }
        }

        if (body.Note is { Length: > ConfigurationCriterion.NoteMaxLength } note)
        {
            errors["note"] = [$"The note has {note.Length} characters; at most {ConfigurationCriterion.NoteMaxLength} are allowed."];
        }

        if (errors.Count > 0)
        {
            return EditResult<bool>.Invalid(errors);
        }

        list.Decide(criterionRevisionId, body.IsIncluded!.Value, level, body.OverrideWeight, body.Note, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return EditResult<bool>.Done(true);
    }

    /// <summary>
    /// Replaces the selection of the list under its pinned release („Auswahl ändern“, R-SAV-03; owner
    /// decision S5-21 3a). Only while that release is active (owner decision S5-21 5a: an older list is
    /// copied instead) — else <see cref="EditOutcome.ReleaseMismatch"/>. The selection is checked as
    /// for saving (CAT-11, CAT-12, CAT-18); every criterion gets the recommendation of the new
    /// selection, a criterion that stays keeps its decision (<see cref="DecisionCarryOver"/>), and the
    /// decisions of criteria that leave are named in the answer (owner decision S5-21 4a). The four
    /// selection layers of a project's copy are fixed: <see cref="EditOutcome.ProjectCopy"/> (WSP-03).
    /// </summary>
    public async Task<EditResult<ChangedSelection>> ChangeSelectionAsync(
        Guid id, DraftSelection? selection, CancellationToken cancellationToken)
    {
        var list = await db.CriteriaConfigurations.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (list is null)
        {
            return EditResult<ChangedSelection>.NotFound();
        }

        if (list.IsProjectCopy)
        {
            return ProjectCopyRefused<ChangedSelection>("The selection of a project's criteria list is fixed.");
        }

        if (selection is null)
        {
            return EditResult<ChangedSelection>.Invalid(ErrorAt(SelectionField, "The selection is required."));
        }

        if (string.IsNullOrEmpty(selection.ReleaseVersion))
        {
            return EditResult<ChangedSelection>.Invalid(ErrorAt(ReleaseField, "The catalog release is required."));
        }

        if (selection.ReleaseVersion != list.ReleaseVersion)
        {
            return EditResult<ChangedSelection>.Refused(
                EditOutcome.ReleaseMismatch,
                $"The list is pinned to catalog release '{list.ReleaseVersion}'.",
                ErrorAt(ReleaseField, $"The list is pinned to catalog release '{list.ReleaseVersion}'; load it again."));
        }

        var resolved = await catalog.ResolveAsync(
            list.ReleaseVersion,
            new CatalogSelectionQuery(
                selection.UseCaseTemplateId, selection.AppTypeIds ?? [], selection.ProductPartIds ?? [], selection.TestTypeIds ?? []),
            cancellationToken);
        if (resolved.Release is not { } release)
        {
            // Stored releases never change (CAT-05), so the release of a stored list is always there.
            throw new InvalidOperationException($"The release '{list.ReleaseVersion}' of saved list {list.Id} is not stored or not complete.");
        }

        if (!release.IsActive)
        {
            return EditResult<ChangedSelection>.Refused(
                EditOutcome.ReleaseMismatch,
                $"Catalog release '{release.Version}' is no longer the active release.",
                ErrorAt(ReleaseField, $"Catalog release '{release.Version}' is no longer the active release; copy the list to the active release instead."));
        }

        var selectionErrors = resolved.Errors.ToDictionary(
            e => ConfigurationDraftCheck.SelectionKey(e.Key), e => e.Value, StringComparer.Ordinal);
        if (resolved.Outcome == CatalogSelectionOutcome.ExcludedSelection)
        {
            return EditResult<ChangedSelection>.Refused(
                EditOutcome.SelectionExcluded, "The selection contains an excluded value.", selectionErrors);
        }

        if (resolved.Selection is not { } valid)
        {
            return EditResult<ChangedSelection>.Invalid(selectionErrors);
        }

        if (!valid.UseCaseIsActive)
        {
            return EditResult<ChangedSelection>.Invalid(
                ErrorAt(UseCaseField, $"The use case '{valid.UseCase?.Key}' is no longer offered; choose another one."));
        }

        var previous = await catalog.ResolveAsync(list.ReleaseVersion, QueryOf(list), cancellationToken);
        var rows = list.Criteria.ToDictionary(c => c.CriterionRevisionId);
        var kept = valid.Criteria.Select(c => c.RevisionId).ToHashSet();
        var dropped = (previous.Selection?.Criteria ?? [])
            .Where(c => !kept.Contains(c.RevisionId) && rows.TryGetValue(c.RevisionId, out var row) && row.HasDecision)
            .Select(NameOf)
            .ToList();

        list.ChangeSelection(
            valid.UseCase?.Key,
            [.. valid.AppTypes.Select(v => v.Key)],
            [.. valid.ProductParts.Select(v => v.Key)],
            [.. valid.TestTypes.Select(v => v.Key)],
            [.. valid.Criteria.Select(c => DecisionCarryOver.Carry(
                c.RevisionId, c.RecommendedPriority.Level, rows.GetValueOrDefault(c.RevisionId)))],
            clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return EditResult<ChangedSelection>.Done(new ChangedSelection(dropped));
    }

    /// <summary>
    /// Deletes the list with its rows (R-SAV-06). Project copies of it are separate lists (D1) and
    /// stay; a project's copy itself is not deleted here — it goes only when the project's list is
    /// replaced (<see cref="EditOutcome.ProjectCopy"/>). The endpoint deletes the list's share links
    /// in Platform afterwards (S5-28a); until then they answer 404, because the read finds no list.
    /// </summary>
    public async Task<EditResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var list = await db.CriteriaConfigurations.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (list is null)
        {
            return EditResult<bool>.NotFound();
        }

        if (list.IsProjectCopy)
        {
            return ProjectCopyRefused<bool>("A project's criteria list is not deleted on its own.");
        }

        db.CriteriaConfigurations.Remove(list);
        await db.SaveChangesAsync(cancellationToken);
        return EditResult<bool>.Done(true);
    }

    /// <summary>
    /// „Neue Liste mit derselben Auswahl“ (R-LST-14, D6, C58; CAT-22): a new list of the same
    /// organisation with the same name (owner decision S5-21 6a) on the active release, created by
    /// <paramref name="userId"/>. Values the active release no longer allows are dropped and named;
    /// each decision moves to the criterion that continues it by key or successor key
    /// (<see cref="ICatalogReleaseCarryOver"/>, <see cref="DecisionCarryOver"/>), and the criteria
    /// whose decisions find no place are named. The source list is not changed (ADR-002). A project's
    /// copy is never a source (<see cref="EditOutcome.ProjectCopy"/>, WSP-02): a newer release means a
    /// new project with a copy of the re-pinned organisation list (ARCHITECTURE §5.4).
    /// </summary>
    public async Task<EditResult<CopiedConfiguration>> CopyToActiveReleaseAsync(
        Guid id, string userId, CancellationToken cancellationToken)
    {
        var list = await db.CriteriaConfigurations.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (list is null)
        {
            return EditResult<CopiedConfiguration>.NotFound();
        }

        if (list.IsProjectCopy)
        {
            return ProjectCopyRefused<CopiedConfiguration>("A project's criteria list is not a source for a copy.");
        }

        var result = await carryOver.CarryOverAsync(list.ReleaseVersion, QueryOf(list), cancellationToken);
        switch (result.Outcome)
        {
            case CatalogCarryOverOutcome.SourceReleaseNotFound:
                throw new InvalidOperationException($"The release '{list.ReleaseVersion}' of saved list {list.Id} is not stored or not complete.");
            case CatalogCarryOverOutcome.NoActiveRelease:
                return EditResult<CopiedConfiguration>.Refused(EditOutcome.NotFound, "No catalog release is active.", NoErrors());
            case CatalogCarryOverOutcome.NoSelectionLeft:
                return EditResult<CopiedConfiguration>.Refused(
                    EditOutcome.SelectionExcluded,
                    "The active catalog release leaves no complete selection of this list.",
                    result.Errors.ToDictionary(e => ConfigurationDraftCheck.SelectionKey(e.Key), e => e.Value, StringComparer.Ordinal));
        }

        var release = result.Release!;
        if (release.Version == list.ReleaseVersion)
        {
            return EditResult<CopiedConfiguration>.Invalid(
                ErrorAt("release", $"The list is already on the active catalog release '{release.Version}'."));
        }

        var previous = result.Previous!;
        var selection = result.Selection!;
        var rows = list.Criteria.ToDictionary(c => c.CriterionRevisionId);

        // Each new criterion takes the decision of one old criterion that continues it: the one with
        // its own key first, then one with a decision. Of two merged criteria with decisions, the
        // first in list order wins and the other is reported as not carried over.
        var sourceOf = new Dictionary<int, int>();
        var newKeys = selection.Criteria.ToDictionary(c => c.RevisionId, c => c.Key);
        var continued = previous.Criteria
            .Where(c => result.Successors.ContainsKey(c.RevisionId))
            .OrderBy(c => newKeys[result.Successors[c.RevisionId]] == c.Key ? 0 : 1)
            .ThenBy(c => rows.GetValueOrDefault(c.RevisionId)?.HasDecision == true ? 0 : 1);
        foreach (var old in continued)
        {
            sourceOf.TryAdd(result.Successors[old.RevisionId], old.RevisionId);
        }

        var carried = sourceOf.Values.ToHashSet();
        var notCarried = previous.Criteria
            .Where(c => !carried.Contains(c.RevisionId) && rows.TryGetValue(c.RevisionId, out var row) && row.HasDecision)
            .Select(NameOf)
            .ToList();

        var copy = CriteriaConfiguration.Create(
            list.OrganizationId,
            list.Name,
            userId,
            clock.GetUtcNow(),
            release.Version,
            selection.UseCase?.Key,
            [.. selection.AppTypes.Select(v => v.Key)],
            [.. selection.ProductParts.Select(v => v.Key)],
            [.. selection.TestTypes.Select(v => v.Key)],
            [.. selection.Criteria.Select(c => DecisionCarryOver.Carry(
                c.RevisionId,
                c.RecommendedPriority.Level,
                sourceOf.TryGetValue(c.RevisionId, out var old) ? rows.GetValueOrDefault(old) : null))]);
        db.CriteriaConfigurations.Add(copy);
        await db.SaveChangesAsync(cancellationToken);
        return EditResult<CopiedConfiguration>.Done(
            new CopiedConfiguration(copy.Id, copy.ReleaseVersion, [.. result.DroppedValues.Select(v => v.Name)], notCarried));
    }

    /// <summary>The level names of SPEC §8.2 and the UX keys.</summary>
    internal static string LevelName(PriorityLevel level) => level switch
    {
        PriorityLevel.A => "A",
        PriorityLevel.B => "B",
        PriorityLevel.Info => "Info",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown level."),
    };

    private static CatalogSelectionQuery QueryOf(CriteriaConfiguration list) => new(
        list.UseCaseKey,
        [.. list.AppTypes.Select(v => v.AppTypeKey)],
        [.. list.ProductParts.Select(v => v.ProductPartKey)],
        [.. list.TestTypes.Select(v => v.TestTypeKey)]);

    private static ConfigurationCriterionName NameOf(ResolvedCatalogCriterion criterion) =>
        new(criterion.RevisionId, criterion.Number, criterion.PlainLanguageTitle ?? criterion.Title);

    private static EditResult<T> ProjectCopyRefused<T>(string detail) =>
        EditResult<T>.Refused(EditOutcome.ProjectCopy, detail, NoErrors());

    private static Dictionary<string, string[]> ErrorAt(string field, string message) =>
        new(StringComparer.Ordinal) { [field] = [message] };

    private static Dictionary<string, string[]> NoErrors() => new(StringComparer.Ordinal);
}

/// <summary>What became of a change to a saved list.</summary>
internal enum EditOutcome
{
    Done = 1,

    /// <summary>404 <c>not-found</c>: the list (or the named criterion, or an active release) does not exist for the request.</summary>
    NotFound = 2,

    /// <summary>400 <c>validation</c> with field errors.</summary>
    Invalid = 3,

    /// <summary>400 <c>selection-excluded</c> (CAT-12).</summary>
    SelectionExcluded = 4,

    /// <summary>400 <c>release-mismatch</c>: the list is not on the active release, or the body names another release.</summary>
    ReleaseMismatch = 5,

    /// <summary>409 <c>project-copy</c>: the action is for organisation lists, the list is a project's copy (C53, S6-12).</summary>
    ProjectCopy = 6,
}

/// <summary>The organisation of a stored list and whether a project owns it as its copy (C53).</summary>
internal sealed record ListOwner(Guid OrganizationId, bool IsProjectCopy);

/// <summary>The outcome of a change, its answer when it is done, else a detail and the field errors.</summary>
internal sealed record EditResult<T>(EditOutcome Outcome, T? Value, string? Detail, IReadOnlyDictionary<string, string[]> Errors)
{
    public static EditResult<T> Done(T value) => new(EditOutcome.Done, value, null, new Dictionary<string, string[]>());

    public static EditResult<T> NotFound() => new(EditOutcome.NotFound, default, null, new Dictionary<string, string[]>());

    public static EditResult<T> Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(EditOutcome.Invalid, default, "The request is not valid.", errors);

    public static EditResult<T> Refused(EditOutcome outcome, string detail, IReadOnlyDictionary<string, string[]> errors) =>
        new(outcome, default, detail, errors);
}
