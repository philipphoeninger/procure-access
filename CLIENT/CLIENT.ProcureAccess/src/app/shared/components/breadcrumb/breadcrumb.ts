// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

/** One step of the path: a page above with its address, or — the last — the page shown. */
export interface BreadcrumbItem {
  label: string;
  /** The address of a page above; the last item has none. */
  link?: string;
}

/**
 * pa-breadcrumb · the path to the page shown, above its H1 (Figma S32 `2148:3877`, S34 `2134:3184`;
 * owner decision S6-14 D6 (a)): a navigation region named `common.breadcrumb` „Sie sind hier“ with an
 * ordered list; the pages above are links, the last item is the page itself as text with
 * `aria-current="page"`. The separator „›“ is drawn by CSS and has no text for screen readers. The
 * list wraps at 320 px; no truncation (CLAUDE §9).
 *
 * @spec docs/design_handoff_procureaccess_v1.md §5.1 (S32, S34)
 */
@Component({
  selector: 'pa-breadcrumb',
  imports: [RouterLink, TranslatePipe],
  template: `
    <nav class="pa-breadcrumb" [attr.aria-label]="'common.breadcrumb' | translate">
      <ol class="pa-breadcrumb__list">
        @for (item of items(); track $index; let last = $last) {
          <li class="pa-breadcrumb__item">
            @if (last || item.link === undefined) {
              <span [attr.aria-current]="last ? 'page' : null">{{ item.label }}</span>
            } @else {
              <a [routerLink]="item.link">{{ item.label }}</a>
            }
          </li>
        }
      </ol>
    </nav>
  `,
  styleUrl: './breadcrumb.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Breadcrumb {
  readonly items = input.required<readonly BreadcrumbItem[]>();
}
