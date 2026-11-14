// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { TextKey } from '@app/core/errors/error-summary-item';
import { problemDetails, problemTypeName } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { Notice } from '@app/shared/components/notice/notice';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';

/**
 * The notice of an unconfirmed address (SPEC R-AUTH-02, SEC-10; Handoff §5.3 „E-mail
 * confirmation“; UX copy §5.2): `pa-notice` attention with `auth.confirm.required` and the button
 * „Bestätigungs-E-Mail erneut senden“, shown where a gated action was refused with 403
 * `email-unconfirmed` (S5-16 is the first) and later on S61.
 *
 * The button asks the server for a new link (`withIdentity().resendConfirmation()`) with the button
 * spinner of Handoff §6.2. Its result shows as a line inside the notice and is announced:
 * `auth.confirm.sent` with the signed-in address, `errors.email.sendFailed` when the server could
 * not send it (503 `email-delivery`), or `errors.rateLimit` (429). Other failures keep the system
 * states of the error interceptor (a 401 opens the login overlay and repeats the request).
 *
 * @spec docs/SPEC_v2.md R-AUTH-02; docs/design_handoff_procureaccess_v1.md §5.3
 * @invariants SEC-10
 */
@Component({
  selector: 'pa-confirm-required-notice',
  imports: [MatButtonModule, MatProgressSpinnerModule, Notice, TranslatePipe],
  templateUrl: './confirm-required-notice.html',
  styleUrl: './confirm-required-notice.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmRequiredNotice {
  private readonly store = inject(ProcureAccessStore);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);

  protected readonly busy = signal(false);
  /** The result of the last resend; null before the first. */
  protected readonly result = signal<TextKey | null>(null);

  protected async resend(): Promise<void> {
    if (this.busy()) {
      return;
    }
    this.busy.set(true);
    let result: TextKey | null;
    try {
      await this.store.resendConfirmation();
      result = { key: 'auth.confirm.sent', params: { email: this.store.session()?.email ?? '' } };
    } catch (error: unknown) {
      result = this.failure(error);
    } finally {
      this.busy.set(false);
    }
    this.result.set(result);
    if (result) {
      void this.announcer.announce(result.key, result.params);
    }
  }

  /** The line for a failure the notice shows; null for one the error interceptor shows itself. */
  private failure(error: unknown): TextKey | null {
    switch (problemTypeName(error)) {
      case 'email-delivery':
        return { key: 'errors.email.sendFailed' };
      case 'rate-limit': {
        const minutes = problemDetails(error)?.retryAfterMinutes ?? 1;
        return { key: pluralKey(this.translate, 'errors.rateLimit', minutes), params: { minutes } };
      }
      default:
        return null;
    }
  }
}
