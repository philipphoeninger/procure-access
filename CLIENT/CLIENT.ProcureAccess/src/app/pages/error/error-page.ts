// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

export type ErrorPageKind = 'notFound' | 'forbidden' | 'server';

/** Route data key: which S80 page a route shows. */
export const ERROR_PAGE_KEY = 'errorPage';

/**
 * S80 Fehlerseiten (R-SHL-08): H1, text and „Zur Startseite“. The page title is the H1
 * (`pageTitle.notFound` / `.forbidden` / `.server` in the route data). `**` shows „not found“ in
 * place, `/unauthorized` „forbidden“, and the error interceptor shows „server“ for every 5xx
 * response without changing the URL. A page that loads its own data shows it in place through the
 * `kind` input (S21 for an unknown organisation, S5-16b).
 *
 * @spec docs/SPEC_v2.md R-SHL-08; docs/design_handoff_procureaccess_v1.md §5.3 (S80, no Figma frame), §6.5
 */
@Component({
  selector: 'pa-error-page',
  imports: [MatButtonModule, RouterLink, TranslatePipe],
  templateUrl: './error-page.html',
  styleUrl: './error-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ErrorPage {
  /** The page to show when it is not the route's (a page that embeds S80 in place). */
  readonly kind = input<ErrorPageKind>();

  private readonly routeKind = inject(ActivatedRoute).snapshot.data[ERROR_PAGE_KEY] as ErrorPageKind;
  protected readonly page = computed(() => this.kind() ?? this.routeKind);
}
