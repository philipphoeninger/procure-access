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
}

/**
 * The only way to announce something to screen readers (CLAUDE §9, Handoff §3.4, §7): every
 * text is a UX copy key. Announcements wait for each other: CDK `LiveAnnouncer` drops a message
 * that is not yet in its live region when the next one comes (it sets each after 100 ms), so
 * without the queue the page title of a route change would swallow `common.loading` (S2-17).
 * The per-key throttle of Handoff §3.4 comes with its first user (autosave, counters).
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

  /** Announces the German text of `key` once the texts are loaded. */
  async announce(key: string, params?: InterpolationParameters, options: AnnounceOptions = {}): Promise<void> {
    const text = firstValueFrom(this.translate.get(key, params)) as Promise<string>;
    await this.enqueue(text, options.politeness ?? 'polite');
  }

  /**
   * Announces the document title after a route change (A11Y-03). The title is built from
   * `shell.pageTitle*` keys by `PageTitleStrategy`, or is the app name on a page without a key.
   */
  async announcePageTitle(title: string): Promise<void> {
    await this.enqueue(title, 'polite');
  }

  /** Queued in call order; a text that is still being translated holds the queue. */
  private enqueue(text: string | Promise<string>, politeness: AriaLivePoliteness): Promise<void> {
    const announced = this.queue.then(async () => this.liveAnnouncer.announce(await text, politeness));
    this.queue = announced.catch(() => undefined);
    return announced;
  }
}
