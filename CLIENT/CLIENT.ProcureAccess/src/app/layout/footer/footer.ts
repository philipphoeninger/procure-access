// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import {
  CRITERIA_SUGGESTION_CHANNEL,
  parseSuggestionChannel,
  suggestionHref,
} from '@app/core/contact/criteria-suggestion';

/** The public repository of the project (CLAUDE §1). */
export const SOURCE_CODE_URL = 'https://github.com/philipphoeninger/procure-access';

/**
 * Footer (R-SHL-06, Handoff §5.3: not designed, a link list on `surface-container`).
 *
 * Only links with a target so far (owner, S2-15): „Über ProcureAccess“ (no page) and „Projekt
 * WERTE.IT“ (no URL) follow when their target exists. „Kriterium vorschlagen“ (R-SUG-01, S3-21)
 * opens the configured channel; a hidden suffix says whether it opens the mail program or an
 * external page.
 *
 * @spec docs/design_handoff_procureaccess_v1.md §5.3
 */
@Component({
  selector: 'pa-footer',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './footer.html',
  styleUrl: './footer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Footer {
  protected readonly sourceCodeUrl = SOURCE_CODE_URL;
  protected readonly suggestChannel = parseSuggestionChannel(inject(CRITERIA_SUGGESTION_CHANNEL));

  private readonly mailSubject = toSignal(inject(TranslateService).stream('suggest.mailSubject'), {
    initialValue: '',
  });
  protected readonly suggestHref = computed(() =>
    suggestionHref(this.suggestChannel, String(this.mailSubject())),
  );
}
