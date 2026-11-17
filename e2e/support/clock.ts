// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { test, type Page } from '@playwright/test';

/** Cookie with the offset in seconds that the E2E stack's API adds to its clock (S5-30; TESTING §3.3). */
const TEST_CLOCK_COOKIE = 'pa-test-clock';

const MINUTE = 60_000;

/**
 * The browser's and the API's clock of one browser context, moved together (J5; S5-30, owner
 * decisions D4 (a), Q2 (a)). The browser runs on Playwright's `page.clock`; the API adds the offset
 * of the cookie `pa-test-clock` to the time of each request that carries it, which only this
 * context does — the other tests on the same stack keep the real time. The offset only grows, so a
 * session never goes back in time.
 */
export class SessionClock {
  private offsetMs = 0;

  private constructor(private readonly page: Page) {}

  /** Installs the browser clock at the API's time `startMinutes` ahead; call it before the first `goto`. */
  static async install(page: Page, startMinutes = 0): Promise<SessionClock> {
    const clock = new SessionClock(page);
    await clock.moveApi(startMinutes);
    await page.clock.install({ time: Date.now() + clock.offsetMs });
    return clock;
  }

  /** Moves both clocks: the page's timers that fall due run once, the next request sees the new time. */
  async move(minutes: number): Promise<void> {
    await this.moveApi(minutes);
    await this.page.clock.fastForward(minutes * MINUTE);
  }

  /** Moves the API's clock only, as when a laptop slept and the page's timers did not run. */
  async moveApi(minutes: number): Promise<void> {
    this.offsetMs += minutes * MINUTE;
    const baseURL = test.info().project.use.baseURL ?? 'http://localhost:4200';
    await this.page.context().addCookies([
      { name: TEST_CLOCK_COOKIE, value: String(Math.round(this.offsetMs / 1000)), url: baseURL },
    ]);
  }
}
