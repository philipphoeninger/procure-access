// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { injectWideLayout } from '@app/layout/shell/wide-layout';

import { CANDIDATE_TABS, CandidateTab, candidatePath } from '../../models/candidates';

/**
 * The tabs of a candidate as routes (R-CAN-03; Handoff §5.1 S34/S34a, §5.3 S33): a
 * `mat-tab-nav-bar` with Überblick, Prüfnachweis and Einschätzung nach Kriterien, and the tab's
 * content (projected) in its `mat-tab-nav-panel`.
 *
 * - Material 22 requires the panel (`[tabPanel]`; without it the bar throws), so the links are ARIA
 *   tabs: `role="tab"` with `aria-selected` and `aria-controls`, arrow keys between them, the
 *   content a `tabpanel` named by the selected tab — not plain links with `aria-current="page"` as
 *   Handoff §5.1 says (deviation recorded in the protocol of S6-16). Each tab is still a route with
 *   its own page title.
 * - The selected tab is the one whose address is shown (exact match), or `selected` when the page
 *   is not a tab's address: S33 edits the facts of Überblick and selects it.
 * - Not stretched (Figma 360: `stretchTabs=false`); below 600 px „Einschätzung nach Kriterien“
 *   shows its short label `candidate.tabs.assessment.short` (U31) — the page title keeps the full
 *   name. Tabs that do not fit scroll inside the bar, never the page (A11Y-09).
 * - The `<nav>` has no accessible name of its own: with the panel Material makes it the
 *   `tablist`, which its tabs name.
 */
@Component({
  selector: 'pa-candidate-tabs',
  imports: [MatTabsModule, RouterLink, RouterLinkActive, TranslatePipe],
  template: `
    <nav mat-tab-nav-bar class="pa-candidate-tabs" mat-stretch-tabs="false" [tabPanel]="panel">
      @for (tab of tabs; track tab) {
        <a
          mat-tab-link
          [routerLink]="link(tab)"
          routerLinkActive
          #current="routerLinkActive"
          [routerLinkActiveOptions]="{ exact: true }"
          [active]="selected() === null ? current.isActive : selected() === tab"
        >
          {{ labelKey(tab) | translate }}
        </a>
      }
    </nav>
    <mat-tab-nav-panel #panel class="pa-candidate-tabs__panel">
      <ng-content />
    </mat-tab-nav-panel>
  `,
  styles: `
    :host {
      display: flex;
      flex-direction: column;
      gap: var(--pa-space-md);
    }

    // The bar's own divider across the content column in outline, as Figma S34 shows it
    .pa-candidate-tabs {
      --mat-tab-divider-color: var(--mat-sys-outline);
    }

    // Below 600 px 16 px label padding, so the labels fit where they can (Handoff §2.2, as S11's
    // tabs, #312); where they cannot (320 px), the bar scrolls inside itself, never the page
    .pa-candidate-tabs .mat-mdc-tab-link.mdc-tab {
      padding-inline: var(--pa-space-md);

      @media (min-width: 37.5em) {
        padding-inline: 1.5rem;
      }
    }

    .pa-candidate-tabs__panel {
      display: block;
      padding-block-start: var(--pa-space-xs);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateTabs {
  readonly projectId = input.required<string>();
  readonly candidateId = input.required<string>();
  /** The tab to select on a page that is not a tab's address (S33: Überblick); null = by the address. */
  readonly selected = input<CandidateTab | null>(null);

  protected readonly tabs = CANDIDATE_TABS;
  private readonly wide = injectWideLayout();

  protected link(tab: CandidateTab): string {
    return candidatePath(this.projectId(), this.candidateId(), tab);
  }

  /** `candidate.tabs.<tab>`; below 600 px the short label of „Einschätzung nach Kriterien“ (U31). */
  protected labelKey(tab: CandidateTab): string {
    return tab === 'assessment' && !this.wide() ? 'candidate.tabs.assessment.short' : `candidate.tabs.${tab}`;
  }
}
