// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed, DestroyRef, effect, inject, Injectable, Injector, signal, untracked } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';

import { ProcureAccessStore } from '@app/core/state/app.store';
import type { SessionWarningKind } from '@features/identity/dialogs/session-warning-dialog/session-warning-dialog';
import { LoginOverlayService } from './login-overlay.service';

/** When the warning shows and which limit ends the session, in epoch milliseconds. */
interface SessionPlan {
  /** The earlier of the two limits. */
  deadline: number;
  /** `deadline` minus the server's lead time. */
  warnAt: number;
  kind: SessionWarningKind;
}

const MINUTE = 60_000;

/**
 * Warns before the session ends (SPEC R-AUTH-06; ARCHITECTURE §8.3; A11Y-04, WCAG 2.2.1).
 *
 * It follows the limits the store shows (`sessionExpiresAt`, `sessionAbsoluteExpiresAt`, moved by
 * every answer to a signed-in request) and the server's lead time (`sessionWarningLeadSeconds`):
 *
 * 1. At the lead time before the earlier limit it asks the probe, which does not renew, whether the
 *    session is still there — another tab may have extended or ended it. If it is, the alert dialog
 *    `SessionWarningDialog` opens: for the idle limit with „Angemeldet bleiben“, for the absolute
 *    limit with the request to save and sign in again. A failed probe changes nothing: the warning
 *    shows.
 * 2. An answer that moves the limit out of the warning time closes the warning (after „Angemeldet
 *    bleiben“, or any other request that renewed the session).
 * 3. At the limit it asks the probe again. A session that ended replaces the warning by the login
 *    overlay over the page (owner decision S5-30 D1 (b)); a session still there is planned again; a
 *    failed probe only closes the warning, and the next request decides.
 *
 * Times are compared with the browser's clock; a wrong clock moves the warning (S5-30 open point).
 * A limit that has already passed when it is planned shows nothing: the next request answers 401
 * and opens the overlay, so a wrong clock can never make the probe run in a loop. `App` starts it.
 */
@Injectable({ providedIn: 'root' })
export class SessionWarningService {
  private readonly store = inject(ProcureAccessStore);
  private readonly dialog = inject(MatDialog);
  private readonly overlay = inject(LoginOverlayService);
  private readonly injector = inject(Injector);

  private readonly kind = signal<SessionWarningKind>('idle');
  private readonly minutes = signal(0);
  private readonly plan = computed<SessionPlan | null>(() => {
    const expiresAt = this.store.sessionExpiresAt();
    const lead = this.store.sessionWarningLeadSeconds();
    if (expiresAt === null || lead === null) {
      return null;
    }
    const idle = Date.parse(expiresAt);
    const absoluteText = this.store.sessionAbsoluteExpiresAt();
    const absolute = absoluteText === null ? Number.POSITIVE_INFINITY : Date.parse(absoluteText);
    if (Number.isNaN(idle) || Number.isNaN(absolute)) {
      return null;
    }
    const deadline = Math.min(idle, absolute);
    return { deadline, warnAt: deadline - lead * 1000, kind: absolute <= idle ? 'absolute' : 'idle' };
  });

  private timers: ReturnType<typeof setTimeout>[] = [];
  private minuteTimer: ReturnType<typeof setTimeout> | undefined;
  private dialogRef: MatDialogRef<unknown> | null = null;
  /** True from the decision to show the warning until it closes, also while its code loads. */
  private wanted = false;
  private started = false;
  /** The user whose ended session already asked for the login overlay; the warning and the limit may both find it. */
  private endedUserId: string | null = null;

  constructor() {
    inject(DestroyRef).onDestroy(() => {
      this.clearTimers();
      this.closeWarning();
    });
  }

  /** Starts following the session; a second call does nothing. */
  start(): void {
    if (this.started) {
      return;
    }
    this.started = true;
    effect(
      () => {
        const plan = this.plan();
        untracked(() => this.schedule(plan));
      },
      { injector: this.injector },
    );
  }

  private schedule(plan: SessionPlan | null): void {
    this.clearTimers();
    const now = Date.now();
    if (plan === null || now >= plan.deadline) {
      this.closeWarning();
      return;
    }
    this.endedUserId = null;
    this.later(plan.deadline - now, () => void this.atLimit());
    if (now < plan.warnAt) {
      this.closeWarning();
      this.later(plan.warnAt - now, () => void this.beforeWarning());
    } else {
      this.showWarning(plan);
    }
  }

  private async beforeWarning(): Promise<void> {
    const previousUserId = this.store.session()?.id ?? null;
    if ((await this.store.refreshSession()) === 'signedOut') {
      this.sessionEnded(previousUserId);
      return;
    }
    // Moved limits are planned again by the effect; unchanged ones show the warning now.
    const plan = this.plan();
    const now = Date.now();
    if (plan !== null && now >= plan.warnAt && now < plan.deadline) {
      this.showWarning(plan);
    }
  }

  private async atLimit(): Promise<void> {
    const previousUserId = this.store.session()?.id ?? null;
    if ((await this.store.refreshSession()) === 'signedOut') {
      this.sessionEnded(previousUserId);
      return;
    }
    const plan = this.plan();
    if (plan === null || Date.now() >= plan.deadline) {
      this.closeWarning();
    }
  }

  /** The session ended: the login overlay replaces the warning, the page stays underneath (D1 (b)). */
  private sessionEnded(previousUserId: string | null): void {
    this.clearTimers();
    this.closeWarning();
    if (previousUserId !== null && previousUserId !== this.endedUserId) {
      this.endedUserId = previousUserId;
      void this.overlay.requestSignIn(previousUserId);
    }
  }

  private showWarning(plan: SessionPlan): void {
    if (this.dialogRef !== null && this.kind() !== plan.kind) {
      // The other limit is next now: the dialog opens again with the focus on its own action.
      this.closeWarning();
    }
    this.kind.set(plan.kind);
    this.countDown(plan.deadline);
    if (this.wanted) {
      return;
    }
    this.wanted = true;
    void this.openDialog();
  }

  private async openDialog(): Promise<void> {
    const { SessionWarningDialog } = await import(
      '@features/identity/dialogs/session-warning-dialog/session-warning-dialog'
    );
    if (!this.wanted || this.dialogRef !== null) {
      return;
    }
    const ref = SessionWarningDialog.open(this.dialog, { kind: this.kind.asReadonly(), minutes: this.minutes.asReadonly() });
    this.dialogRef = ref;
    ref.afterClosed().subscribe(() => {
      if (this.dialogRef === ref) {
        this.dialogRef = null;
        this.wanted = false;
        clearTimeout(this.minuteTimer);
      }
    });
  }

  /** Whole minutes until `deadline`, rounded up, updated when the next whole minute is reached. */
  private countDown(deadline: number): void {
    clearTimeout(this.minuteTimer);
    const remaining = deadline - Date.now();
    if (remaining <= 0) {
      return;
    }
    this.minutes.set(Math.ceil(remaining / MINUTE));
    this.minuteTimer = setTimeout(() => this.countDown(deadline), remaining % MINUTE || MINUTE);
  }

  private closeWarning(): void {
    this.wanted = false;
    clearTimeout(this.minuteTimer);
    const ref = this.dialogRef;
    this.dialogRef = null;
    ref?.close();
  }

  private later(delay: number, action: () => void): void {
    this.timers.push(setTimeout(action, delay));
  }

  private clearTimers(): void {
    this.timers.forEach((timer) => clearTimeout(timer));
    this.timers = [];
  }
}
