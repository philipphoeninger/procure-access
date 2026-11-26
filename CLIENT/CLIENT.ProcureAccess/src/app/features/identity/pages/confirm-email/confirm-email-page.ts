// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { distinctUntilChanged, map, skip } from 'rxjs';

import { PAGE_TITLE_KEY, PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import type { ConfirmEmailResult } from './confirm-email.routing';

const TEXTS: Record<ConfirmEmailResult, { title: string; body: string }> = {
  'pageTitle.confirm.done': { title: 'auth.confirm.done.title', body: 'auth.confirm.done' },
  'pageTitle.confirm.invalid': { title: 'auth.confirm.invalid.title', body: 'auth.confirm.invalid' },
  'pageTitle.server': { title: 'errors.server.title', body: 'errors.server.body' },
};

/**
 * The confirmation page of S02 at `/registrieren/bestaetigen` (SPEC R-AUTH-02; UX copy §5.2, U118;
 * Handoff §5.3 „result pages as S80“): H1, text and „Zur Startseite“. The route's guard has taken
 * the token out of the URL and its resolver has redeemed it; the result is the page title key in
 * the route data, so title and H1 have the same wording. A failure that says nothing about the
 * link shows the S80 server text. A second link opened on the page brings its result at the same
 * address; the page then sets title, focus and announcement itself (`changePageInPlace`, TD-68).
 *
 * @spec docs/SPEC_v2.md R-AUTH-02; docs/design_handoff_procureaccess_v1.md §5.3, §6.5
 * @invariants A11Y-03, A11Y-09
 */
@Component({
  selector: 'pa-confirm-email-page',
  imports: [MatButtonModule, RouterLink, TranslatePipe],
  templateUrl: './confirm-email-page.html',
  styleUrl: './confirm-email-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmEmailPage {
  private readonly result = inject(ActivatedRoute).data.pipe(map((data) => data[PAGE_TITLE_KEY] as ConfirmEmailResult));

  /** The texts of the result the resolver put into the route data; a second link opened here brings a new one. */
  protected readonly texts = toSignal(this.result.pipe(map((result) => TEXTS[result])), { requireSync: true });

  constructor() {
    // A second link opened here ends at the same address, so the router sees no new page: title,
    // focus on the H1 and the announcement follow the new result here (TD-68, S5-12b R4).
    const pageTitle = inject(PageTitleStrategy);
    this.result
      .pipe(skip(1), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((result) => pageTitle.changePageInPlace(result));
  }
}
