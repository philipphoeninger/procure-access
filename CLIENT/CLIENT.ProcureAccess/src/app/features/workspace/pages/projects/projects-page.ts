// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, signal, untracked } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ORGANIZATION_PATH } from '@app/features/organization/models/organization';
import { injectWideLayout } from '@app/layout/shell/wide-layout';
import { ErrorPage } from '@app/pages/error/error-page';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';

import { CreateProjectDialog } from '../../dialogs/create-project-dialog/create-project-dialog';
import { projectPath, ProjectSort, ProjectSummary, statusKey } from '../../models/projects';

/** What S30 shows below its H1. */
type ProjectsPageStatus = 'loading' | 'loaded' | 'noOrganization' | 'failed';

/** The options of „Sortieren nach“ in their order; the first is preselected (UX copy U162). */
export const PROJECT_SORTS: readonly ProjectSort[] = ['modified', 'name'];

/**
 * The projects in the chosen order: the most recently changed first, or by name (German
 * collation), each with the other as the tie-breaker. Never by anything else (Handoff §5.3: name or
 * date; never by a count, SIG-01).
 */
export function sortProjects(projects: readonly ProjectSummary[], sort: ProjectSort, collator: Intl.Collator): ProjectSummary[] {
  const byModified = (a: ProjectSummary, b: ProjectSummary) => Date.parse(b.modifiedAt) - Date.parse(a.modifiedAt);
  const byName = (a: ProjectSummary, b: ProjectSummary) => collator.compare(a.name, b.name);
  return [...projects].sort(sort === 'name' ? (a, b) => byName(a, b) || byModified(a, b) : (a, b) => byModified(a, b) || byName(a, b));
}

/**
 * S30 „Beschaffungsprojekte“ at `/projekte` (R-PRJ-01; UX copy §5.8, U162–U166; Handoff §5.3: no
 * Figma frame, the list as S14), signed in, reached from „Projekte“ in the header (R-SHL-02).
 *
 * - The projects of the organisation the header shows (`withWorkspace()`), loaded when the page
 *   opens and again when the header switches the organisation (R-SHL-03, TEN-01). After every load
 *   `common.loaded` announces the number shown.
 * - Archived projects show only with „Archivierte Projekte anzeigen“ (R-PRJ-05), off when the page
 *   opens; a change announces `projects.shown` with the number shown.
 * - „Sortieren nach“ above the list in every width: „Zuletzt geändert“ (preselected, newest first)
 *   or „Name“; a change announces `projects.sort.updated`.
 * - Below 600 px a list of cards (name as H2 link, then „Stand“, „Produkte“ and „Zuletzt geändert“
 *   as a description list), from 600 px a table with the four columns, without sortable headers.
 *   The name opens the project (S32).
 * - Editor and Owner see „Projekt anlegen“, which opens S31 (R-PRJ-02); Readers do not.
 * - No project: `projects.empty`. Only archived ones while the filter is off:
 *   `projects.empty.archivedOnly`. No membership: `projects.needOrg` with „Organisation anlegen oder
 *   beitreten“ to S20. A failed load shows the S80 server text in place.
 *
 * @spec docs/SPEC_v2.md R-PRJ-01, R-PRJ-05, R-SHL-03; docs/design_handoff_procureaccess_v1.md §5.3
 * @invariants TEN-01, A11Y-09
 */
@Component({
  selector: 'pa-projects-page',
  imports: [ErrorPage, MatButtonModule, MatCheckboxModule, MatFormFieldModule, MatSelectModule, MatTableModule, RouterLink, TranslatePipe],
  templateUrl: './projects-page.html',
  styleUrl: './projects-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectsPage {
  private readonly store = inject(ProcureAccessStore);
  private readonly dialog = inject(MatDialog);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly language = inject(DOCUMENT).documentElement.lang || 'de';
  private readonly collator = new Intl.Collator(this.language);
  private readonly dateFormat = new Intl.DateTimeFormat(this.language, { dateStyle: 'medium' });

  protected readonly wide = injectWideLayout();
  protected readonly sorts = PROJECT_SORTS;
  protected readonly columns = ['name', 'status', 'products', 'modified'];
  protected readonly organizationPath = `/${ORGANIZATION_PATH}`;
  protected readonly titleId = 'pa-projects-title';

  /** The organisation the header shows; null without a membership. */
  private readonly organizationId = computed(() => this.store.activeOrganization()?.id ?? null);
  private readonly projects = computed(() => this.store.workspace.projects());

  protected readonly status = computed<ProjectsPageStatus>(() => {
    const organizationId = this.organizationId();
    if (organizationId === null) return 'noOrganization';
    const projects = this.projects();
    if (projects.organizationId !== organizationId) return 'loading';
    return projects.status === 'idle' ? 'loading' : projects.status;
  });

  protected readonly sort = signal<ProjectSort>('modified');
  protected readonly showArchived = signal(false);
  /** Every project of the organisation, also the archived ones. */
  protected readonly all = computed(() => this.projects().items);
  /** The projects shown: without the archived ones unless the filter asks for them, in the chosen order. */
  protected readonly shown = computed(() =>
    sortProjects(
      this.all().filter((project) => this.showArchived() || project.status !== 'archived'),
      this.sort(),
      this.collator,
    ),
  );

  /** Editor or Owner of the organisation the header shows may create a project (R-PRJ-02, R-ORG-05). */
  protected readonly canCreate = computed(() => {
    const role = this.store.activeOrganization()?.role;
    return role === 'owner' || role === 'editor';
  });

  private destroyed = false;

  constructor() {
    inject(DestroyRef).onDestroy(() => (this.destroyed = true));

    // The organisation the header shows; a switch loads its projects (R-SHL-03).
    effect(() => {
      const id = this.organizationId();
      if (id !== null) untracked(() => void this.load(id));
    });
  }

  protected path(project: ProjectSummary): string {
    return projectPath(project.id);
  }

  protected statusKey(project: ProjectSummary): string {
    return statusKey(project.status);
  }

  protected date(value: string): string {
    return this.dateFormat.format(new Date(value));
  }

  protected sortKey(sort: ProjectSort): string {
    return `projects.sort.${sort}`;
  }

  /** „Sortieren nach“: the new order is announced (U162). */
  protected chooseSort(sort: ProjectSort): void {
    this.sort.set(sort);
    void this.announcer.announce('projects.sort.updated', { sort: this.translate.instant(this.sortKey(sort)) as string });
  }

  /** „Archivierte Projekte anzeigen“: the number now shown is announced (U164). */
  protected chooseArchived(show: boolean): void {
    this.showArchived.set(show);
    const count = this.shown().length;
    void this.announcer.announce(pluralKey(this.translate, 'projects.shown', count), { count });
  }

  /** „Projekt anlegen“: S31; on success it opens S32 itself. */
  protected create(): void {
    CreateProjectDialog.open(this.dialog);
  }

  private async load(organizationId: string): Promise<void> {
    const shownAnswer = await this.store.loadProjects(organizationId);
    if (!shownAnswer || this.destroyed || this.status() !== 'loaded') return;
    const count = this.shown().length;
    void this.announcer.announce(pluralKey(this.translate, 'common.loaded', count), { count });
  }
}
