// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { AriaLivePoliteness, LiveAnnouncer } from '@angular/cdk/a11y';
import { inject, Injectable } from '@angular/core';
import { InterpolationParameters, TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

export interface AnnounceOptions {
  /** `polite` (default) waits until the screen reader is idle; `assertive` interrupts it. */
  politeness?: AriaLivePoliteness;
  /**
   * At most one announcement of this key per `throttleMs` (Handoff §3.4: counters 1 s, autosave
   * 5 s). A call inside the interval waits for its end; a later call replaces it, so only the
   * newest text is read. The promise of a replaced call resolves without an announcement.
   */
  throttleMs?: number;
}

interface Throttle {
  /** When the key was last announced (`Date.now()`). */
  last: number;
  timer?: ReturnType<typeof setTimeout>;
  pending?: { send: () => Promise<void>; done: () => void };
}

/**
 * The only way to announce something to screen readers (CLAUDE §9, Handoff §3.4, §7): every
 * text is a UX copy key. Announcements wait for each other: CDK `LiveAnnouncer` drops a message
 * that is not yet in its live region when the next one comes (it sets each after 100 ms), so
 * without the queue the page title of a route change would swallow `common.loading` (S2-17).
 * Counters and autosave pass `throttleMs` (Handoff §3.4); the first user is the match count of
 * the use-case combobox in S10 (S4-10).
 *
 * @spec docs/design_handoff_procureaccess_v1.md §3.4
 * @invariants A11Y-03
 */
@Injectable({ providedIn: 'root' })
export class A11yAnnouncerService {
  private readonly liveAnnouncer = inject(LiveAnnouncer);
  private readonly translate = inject(TranslateService);
  /** Settles when the last queued message is in the live region. */
  private queue: Promise<void> = Promise.resolve();
  private readonly throttles = new Map<string, Throttle>();

  /** Announces the German text of `key` once the texts are loaded. */
  async announce(key: string, params?: InterpolationParameters, options: AnnounceOptions = {}): Promise<void> {
    const politeness = options.politeness ?? 'polite';
    const send = () => this.enqueue(firstValueFrom(this.translate.get(key, params)) as Promise<string>, politeness);
    await (options.throttleMs ? this.throttle(key, options.throttleMs, send) : send());
  }

  /**
   * Announces the document title after a route change (A11Y-03). The title is built from
   * `shell.pageTitle*` keys by `PageTitleStrategy`, or is the app name on a page without a key.
   */
  async announcePageTitle(title: string): Promise<void> {
    await this.enqueue(title, 'polite');
  }

  private throttle(key: string, throttleMs: number, send: () => Promise<void>): Promise<void> {
    let state = this.throttles.get(key);
    if (!state) {
      state = { last: Number.NEGATIVE_INFINITY };
      this.throttles.set(key, state);
    }
    const now = Date.now();
    if (!state.timer && now - state.last >= throttleMs) {
      state.last = now;
      return send();
    }
    state.pending?.done();
    const throttled = state;
    return new Promise<void>((resolve) => {
      throttled.pending = { send, done: resolve };
      throttled.timer ??= setTimeout(() => {
        const pending = throttled.pending;
        throttled.pending = undefined;
        throttled.timer = undefined;
        throttled.last = Date.now();
        void pending?.send().catch(() => undefined).finally(pending.done);
      }, throttled.last + throttleMs - now);
    });
  }

  /** Queued in call order; a text that is still being translated holds the queue. */
  private enqueue(text: string | Promise<string>, politeness: AriaLivePoliteness): Promise<void> {
    const announced = this.queue.then(async () => this.liveAnnouncer.announce(await text, politeness));
    this.queue = announced.catch(() => undefined);
    return announced;
  }
}
