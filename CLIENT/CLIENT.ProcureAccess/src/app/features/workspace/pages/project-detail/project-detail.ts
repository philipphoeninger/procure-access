// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { distinctUntilChanged, skip } from 'rxjs';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { CatalogApiService } from '@app/features/configurator/services/api/catalog-api.service';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';
import { ErrorPage } from '@app/pages/error/error-page';
import { Breadcrumb, BreadcrumbItem } from '@app/shared/components/breadcrumb/breadcrumb';
import { Notice } from '@app/shared/components/notice/notice';
import { ASSESSMENT_VALUES } from '@app/shared/components/rating-legend/rating-legend';
import { pluralKey, TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { candidatePath, newCandidatePath, testStatusKey } from '../../models/candidates';
import { Project, ProjectCandidate, ProjectEvidenceType, projectCriteriaListPath, PROJECTS_PATH, statusKey } from '../../models/projects';
import { EditProjectDialog, RELOAD_PROJECT } from '../../dialogs/edit-project-dialog/edit-project-dialog';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { isProjectEditor } from '../candidate-detail/candidate-load';
import { loadProjectView, ProjectLoad, ProjectView } from './project-detail-load';

/**
 * S32 · Projektübersicht at `/projekte/:id` (R-PRJ-03, R-PRJ-05; Handoff §5.1 S32; Figma 360
 * `2148:4697`, desktop `2148:3877`; S6-14). Signed in; S31 opens it after creating a project, S30
 * links it by name.
 *
 * - Above the H1 the path „Projekte › {{project}}“ (`pa-breadcrumb`, owner decision S6-14 D6 (a)).
 *   The title row holds the H1 and, for Editor and Owner, the outlined „Projekt bearbeiten“, which
 *   opens the dialog for name and status (R-PRJ-05; D1 (a)); then the line `projects.detail.meta`
 *   and, for an archived project, `projects.archived.note`.
 * - Card „Kriterienliste“: a `<dl>` with the list (a link to S11 of the project's copy, R-PRJ-06),
 *   its release, the scope (`.scope.value`: the requirement criteria by level, the „Geprüft nach …“
 *   criteria named apart and never counted, or `.scope.value.noProcedures`), use case, app types
 *   and — only with a selection — test types; then `copyNote`, `projects.detail.criteriaAdded` after
 *   S11 took criteria in (shown and announced once, WSP-03) and `projects.detail.criteriaLocked`
 *   once the server reports the lock (R-PRJ-04). „Kriterienliste anpassen“ for Editor and Owner of a
 *   project that is not archived. Without a list `projects.detail.noCriteria`; assigning a list from
 *   S32 is not built (owner decision S6-14 D2 (a)).
 * - Card „Produkte in diesem Projekt“: the menu „Produkt hinzufügen“ (`aria-haspopup="menu"`, the
 *   item „Neu erfassen“ → S33; „Aus dem Produktverzeichnis übernehmen“ is hidden until M6, D3 (a))
 *   for Editor and Owner of a project that is not archived; `projects.detail.orderNote`; a `<ul>`
 *   with the last added product first: its name (H3) is the only link to S34, then the vendor line
 *   and a `<dl>` „Auf Barrierefreiheit geprüft“, „Einschätzung“, „Abgleich“ and „Veröffentlichung“.
 *   „Abgleich“ is a list of all seven counts in the fixed order of the scale, label first, zeros
 *   included, no total (SIG-08, SIG-13). Nothing sorts or filters by the counts (D3). Progress and
 *   counts are left out while the project has no list. No product: `projects.detail.empty`.
 * - „Neue Fassung laden“ of the conflict dialog S37 over „Projekt bearbeiten“ (R-CAN-11, S6-21)
 *   reads the project again in place, with the loading bar, and moves the focus to the H1.
 * - Archived: read-only (R-PRJ-05) — no menu, no „Kriterienliste anpassen“; „Projekt bearbeiten“
 *   stays for the status.
 * - When the header switches to another organisation, S30 of that organisation opens (S6-13 open
 *   point 2). A project the user cannot see shows „Seite nicht gefunden“ in place, a failed read
 *   the S80 server text. No `<section>`: Firefox crashes axe on named regions (TESTING V229).
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2148-4697
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2148-3877
 * @spec docs/SPEC_v2.md R-PRJ-03, R-PRJ-05, R-PRJ-06; docs/design_handoff_procureaccess_v1.md §5.1
 * @invariants SIG-08, SIG-13, TEN-01, WSP-03, A11Y-09
 */
@Component({
  selector: 'pa-project-detail',
  imports: [Breadcrumb, ErrorPage, MatButtonModule, MatIconModule, MatMenuModule, Notice, RouterLink, TranslatePipe, TranslatePluralPipe],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDetailPage {
  private readonly load = inject(ProjectLoad);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly store = inject(ProcureAccessStore);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);
  private readonly projectsApi = inject(ProjectsApiService);
  private readonly configuratorApi = inject(ConfiguratorApiService);
  private readonly catalogApi = inject(CatalogApiService);

  protected readonly view = signal<ProjectView>(this.load.view);
  /** The criteria S11 of the copy took into the list on the way here; 0 shows nothing. */
  protected readonly criteriaAdded = signal(0);

  private readonly project = computed(() => {
    const view = this.view();
    return view.kind === 'project' ? view.project : null;
  });

  /** Editor or Owner of the project's organisation: „Projekt bearbeiten“, also for an archived project. */
  protected readonly canEdit = computed(() => {
    const project = this.project();
    return project !== null && isProjectEditor(this.store.organizations(), project);
  });

  /** Changes inside the project — products, the list — need an editor and a project that is not archived (R-PRJ-05). */
  protected readonly canChange = computed(() => this.canEdit() && this.project()?.status !== 'archived');

  /** The products with the last added first (`projects.detail.orderNote`); the server lists them as added. */
  protected readonly products = computed(() => [...(this.project()?.candidates ?? [])].reverse());

  protected readonly breadcrumb = computed<BreadcrumbItem[]>(() => [
    { label: this.translate.instant('shell.nav.projects') as string, link: `/${PROJECTS_PATH}` },
    { label: this.project()?.name ?? '' },
  ]);

  /** The rows of „Abgleich“ in the fixed order of the scale (SIG-08); never sorted by value. */
  protected readonly countValues = ASSESSMENT_VALUES;

  constructor() {
    this.showAddedCriteria();
    // Angular keeps the page when only the id changes: take the view the resolver loaded again.
    inject(ActivatedRoute)
      .paramMap.pipe(skip(1), takeUntilDestroyed())
      .subscribe(() => {
        this.view.set(this.load.view);
        this.showAddedCriteria();
      });

    // Another organisation in the header: its projects (S30), not this one (R-SHL-03). No
    // organisation any more (sign-out, account deletion) is left to the flow that cleared it, which
    // navigates itself.
    toObservable(computed(() => this.store.activeOrganization()?.id ?? null))
      .pipe(distinctUntilChanged(), skip(1), takeUntilDestroyed())
      .subscribe((organizationId) => {
        const project = this.project();
        if (project !== null && organizationId !== null && organizationId !== project.organizationId) {
          void this.router.navigateByUrl(`/${PROJECTS_PATH}`);
        }
      });

    // The route change clears the title state; the name is known from the resolver.
    effect(() => {
      const project = this.project();
      if (project !== null) this.pageTitle.setPageTitleState({ params: { project: project.name } });
    });
  }

  protected statusKey = statusKey;
  protected testStatusKey = testStatusKey;
  protected criteriaListPath = projectCriteriaListPath;
  protected newCandidatePath = newCandidatePath;
  protected candidatePath = candidatePath;

  /** „BIT inklusiv Software-Test, BIK BITV-Test (Web)“ of `projects.detail.row.tested.evidences` (UX copy U60). */
  protected evidenceTypes(candidate: ProjectCandidate): string {
    return candidate.testEvidences.map((evidence) => this.evidenceTypeName(evidence)).join(', ');
  }

  /**
   * „Projekt bearbeiten“: the changed project replaces the shown one; the list facts stay. After
   * „Neue Fassung laden“ in S37 the project is read again.
   */
  protected edit(project: Project): void {
    EditProjectDialog.open(this.dialog, project)
      .afterClosed()
      .subscribe((result) => {
        if (result === RELOAD_PROJECT) {
          void this.reload(project.id);
          return;
        }
        const view = this.view();
        if (result !== undefined && view.kind === 'project') this.view.set({ ...view, project: result });
      });
  }

  /** Reads the project and the facts of its list again (S37 „Neue Fassung laden“), then the focus to the H1. */
  private async reload(id: string): Promise<void> {
    this.store.incrementLoadingCount();
    try {
      this.view.set(
        await loadProjectView({ projects: this.projectsApi, configurator: this.configuratorApi, catalog: this.catalogApi }, id),
      );
    } finally {
      this.store.decrementLoadingCount();
    }
    this.pageTitle.focusPageHeading();
  }

  private evidenceTypeName(evidence: ProjectEvidenceType): string {
    switch (evidence.testKind) {
      case 'procedure':
        return this.translate.instant(`testType.${evidence.testTypeId ?? ''}`) as string;
      case 'other':
        return evidence.otherTestTypeName ?? (this.translate.instant('testType.other') as string);
      default:
        return this.translate.instant('testType.own') as string;
    }
  }

  /** `projects.detail.criteriaAdded` once, for the project S11 of its copy reported (R-PRJ-06). */
  private showAddedCriteria(): void {
    const project = this.project();
    const count = project === null ? 0 : this.store.takeCriteriaAdded(project.id);
    this.criteriaAdded.set(count);
    if (count > 0) void this.announcer.announce(pluralKey(this.translate, 'projects.detail.criteriaAdded', count), { count });
  }
}
