// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { map } from 'rxjs';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
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
 * link shows the S80 server text.
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
  /** The texts of the result the resolver put into the route data; a second link opened here brings a new one. */
  protected readonly texts = toSignal(
    inject(ActivatedRoute).data.pipe(map((data) => TEXTS[data[PAGE_TITLE_KEY] as ConfirmEmailResult])),
    { requireSync: true },
  );
}
