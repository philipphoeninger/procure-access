// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  DOCUMENT,
  effect,
  inject,
  Injector,
  input,
  viewChild,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';
import { Notice } from '../notice/notice';

/**
 * Error summary at the top of a form or step (Handoff §6.4, UX copy E11, E12): `pa-notice` error
 * with the number of errors as `<h2>`, one line per error. It takes the focus each time a failed
 * submit gives it new lines, so screen readers announce it (API-02). A line with a field is a link
 * that moves the focus to the field; the whole line is the link (owner, S2-11 Q3). An optional
 * `note` follows the list inside the same notice, so it is read with the errors but not counted as
 * one (S01's lockout note, owner decision S5-13b 2 (a)).
 *
 * @spec docs/design_handoff_procureaccess_v1.md §6.4
 * @invariants API-02
 */
@Component({
  selector: 'pa-error-summary',
  imports: [Notice, TranslatePipe, TranslatePluralPipe],
  templateUrl: './error-summary.html',
  styleUrl: './error-summary.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ErrorSummary {
  /** Replace the array on every failed submit; the summary does not change while the user types. */
  readonly items = input.required<ErrorSummaryItem[]>();
  /** A text after the list, shown only with lines; not an error, so not in the count of the title. */
  readonly note = input<TextKey | null>(null);

  private readonly notice = viewChild(Notice);
  private readonly document = inject(DOCUMENT);
  private readonly injector = inject(Injector);

  constructor() {
    // The notice focuses itself when it first appears; this also covers a second failed submit
    // while the summary is still shown.
    effect(() => {
      if (this.items().length) {
        afterNextRender(() => this.notice()?.focus(), { injector: this.injector });
      }
    });
  }

  protected focusField(event: Event, id: string): void {
    event.preventDefault();
    this.document.getElementById(id)?.focus();
  }
}
