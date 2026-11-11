// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { recordThirdPartyRequests, thirdPartyLinksInDocument } from '../support/third-party-requests';

/**
 * J1, configurator without account (TESTING §5.4), on S10 and the active test release T.1 of the
 * E2E stack, with the keyboard only (A11Y-02): the test never clicks, taps or moves the pointer,
 * and it reaches every control with Tab from where the page put the focus. Axe runs on every state
 * in both appearances (A11Y-01, A11Y-08). J1 ends at the hand-over to S11 for now; the list, the
 * priority change and the export follow with S4-22 (S4-13).
 *
 * The selection meets an exclusion by each higher layer of T.1
 * (`tests/ProcureAccess.Tests.Shared/Fixtures/Releases/T.1/exclusions.json`): HR-Anwendung excludes
 * Hardware and Video, Website oder Webanwendung the App-Test, Audio the Software-Test.
 */

/**
 * The answer of T.1 for this selection, in catalog order: the requirement criteria that apply to Website
 * or Software (offen) with Basisfunktionalität and Audio (99.5 needs Video), then the procedure criterion
 * of the chosen test type.
 */
const EXPECTED_KEYS = ['99.1', '99.2', '99.3', '99.4', '99.6', '99.7', '99.8', '99.10a', '99.10b', 'PROC-bikWeb'];

const MAX_TABS = 60;

interface CriteriaAnswer {
  readonly release: string;
  readonly criteria: readonly {
    readonly key: string;
    readonly resolvedPriority: {
      readonly level: string;
      readonly weight: number | null;
      readonly rationale: string;
      readonly source: string;
      readonly matchedRule: { readonly kind: string; readonly appType: string | null; readonly useCase: string | null };
    };
  }[];
}

/**
 * Presses Tab until the target has the focus, as a keyboard user moves through the page. Fails when
 * the target is not reached, i.e. when it is not in the tab order after the current focus.
 */
async function tabTo(page: Page, target: Locator): Promise<void> {
  for (let presses = 0; presses <= MAX_TABS; presses++) {
    if (await target.evaluate((element) => element === document.activeElement)) {
      return;
    }
    await page.keyboard.press('Tab');
  }
  throw new Error(`${target.toString()} was not reached with ${MAX_TABS} Tab presses`);
}

/** Tab to the option, then Space, as a keyboard user checks it. */
async function check(page: Page, box: Locator): Promise<void> {
  await tabTo(page, box);
  await page.keyboard.press('Space');
  await expect(box).toBeChecked();
}

/** An excluded option is reached by Tab, names its reason, and Space changes nothing (CFG-02). */
async function expectExcluded(page: Page, box: Locator, reason: string): Promise<void> {
  await tabTo(page, box);
  await expect(box).toHaveAttribute('aria-disabled', 'true');
  await expect(box).toHaveAccessibleDescription(reason);
  await page.keyboard.press('Space');
  await expect(box).toBeFocused();
  await expect(box).not.toBeChecked();
}

/** Tab to „Weiter“ and press Enter; the next step focuses its heading. */
async function next(page: Page, heading: string | RegExp): Promise<void> {
  await tabTo(page, page.getByRole('button', { name: 'Weiter' }));
  await page.keyboard.press('Enter');
  await expect(page.getByRole('heading', { level: 2, name: heading })).toBeFocused();
}

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

test.describe('J1 configurator without account', () => {
  test('INV:A11Y-02 INV:A11Y-01 INV:A11Y-08 anonymous visitor chooses a selection on S10 by keyboard and gets the criteria of T.1', async ({ page, baseURL }, testInfo) => {
    // Seven states, each checked by axe in two appearances, take longer than the default 30 s.
    test.slow();
    // CLAUDE §8: fonts, icons and the API all come from the app's own origin (#43).
    const appOrigin = new URL(baseURL!).origin;
    const thirdPartyRequests = recordThirdPartyRequests(page, appOrigin);
    const checkbox = (name: string) => page.getByRole('checkbox', { name, exact: true });

    await test.step('start page: skip link, then the card „Kriterien zusammenstellen“', async () => {
      await page.goto('/');
      await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
      await expectNoAxeViolations(page, testInfo, 'j1 start page');

      await page.keyboard.press('Tab');
      await expect(page.getByRole('link', { name: 'Zum Inhalt springen' })).toBeFocused();
      await page.keyboard.press('Enter');
      await expect(page.getByRole('main')).toBeFocused();
      await tabTo(page, page.getByRole('main').getByRole('link', { name: 'Kriterien zusammenstellen' }));
      await page.keyboard.press('Enter');

      await expect(page).toHaveURL(/\/kriterien$/);
      await expect(page.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeFocused();
      await expect(page).toHaveTitle('Kriterien zusammenstellen – Schritt 1 von 4 – ProcureAccess');
    });

    await test.step('step 1: the use case from the combobox', async () => {
      const combobox = page.getByRole('combobox', { name: 'Verwendungszweck' });
      await tabTo(page, combobox);
      await page.keyboard.type('HR');
      // ArrowDown takes the first option: wait until the list is filtered.
      await expect(page.getByRole('listbox').getByRole('option').first()).toHaveAccessibleName('HR-Anwendung');
      await settled(page.getByRole('listbox'));
      await expectNoAxeViolations(page, testInfo, 'j1 step 1, list filtered');

      await page.keyboard.press('ArrowDown');
      await page.keyboard.press('Enter');
      await expect(combobox).toHaveValue('HR-Anwendung');
      await next(page, 'Was möchten Sie beschaffen?');
    });

    await test.step('step 2: two app types; Hardware is excluded by the use case', async () => {
      await check(page, checkbox('Website oder Webanwendung'));
      await check(page, checkbox('Software (offen)'));
      await expectExcluded(page, checkbox('Hardware'), 'Passt nicht zum Verwendungszweck HR-Anwendung');
      await expectNoAxeViolations(page, testInfo, 'j1 step 2');
      await next(page, 'Welche Funktionen hat das Produkt?');
    });

    await test.step('step 3: Audio next to the locked Basisfunktionalität; Video is excluded', async () => {
      const base = checkbox('Basisfunktionalität');
      await expect(base).toBeChecked();
      await expect(base).toHaveAttribute('aria-disabled', 'true');
      await check(page, checkbox('Audio'));
      await expectExcluded(page, checkbox('Video'), 'Passt nicht zum Verwendungszweck HR-Anwendung');
      await expectNoAxeViolations(page, testInfo, 'j1 step 3');
      await next(page, /Soll das Produkt nach einem bestimmten Verfahren geprüft sein/);
    });

    await test.step('step 4: one test procedure; two are excluded by higher layers', async () => {
      await expectExcluded(page, checkbox('BIT inklusiv Software-Test'), 'Passt nicht zum Funktionsbereich Audio');
      await expectExcluded(page, checkbox('BIT inklusiv App-Test'), 'Passt nicht zum Anwendungstyp Website oder Webanwendung');
      await check(page, checkbox('BIK BITV-Test (Web)'));

      // „Bisherige Auswahl“ names the three earlier steps (R-CON-08).
      const summary = page.getByRole('main').getByRole('listitem');
      await expect(summary.filter({ hasText: 'Verwendungszweck: HR-Anwendung' })).toHaveCount(1);
      await expect(summary.filter({ hasText: 'Anwendungstyp: Website oder Webanwendung und Software (offen)' })).toHaveCount(1);
      await expect(summary.filter({ hasText: 'Funktionsbereiche: Basisfunktionalität und Audio' })).toHaveCount(1);
      await expectNoAxeViolations(page, testInfo, 'j1 step 4');
    });

    await test.step('„Kriterienliste anzeigen“ hands the selection over to S11', async () => {
      const answered = page.waitForResponse((response) => new URL(response.url()).pathname === '/api/v1/catalog/criteria');
      await tabTo(page, page.getByRole('button', { name: 'Kriterienliste anzeigen' }));
      await page.keyboard.press('Enter');

      const response = await answered;
      expect(response.status()).toBe(200);
      // The request names the release the filter options pinned (ADR-002) and exactly the choices.
      const sent = new URL(response.url()).searchParams;
      expect(sent.get('release')).toBe('T.1');
      expect(sent.getAll('useCase')).toEqual(['hr']);
      expect(sent.getAll('appTypeIds[]').sort()).toEqual(['softwareOpen', 'web']);
      expect(sent.getAll('productPartIds[]').sort()).toEqual(['audio', 'base']);
      expect(sent.getAll('testTypeIds[]')).toEqual(['bikWeb']);

      const answer = (await response.json()) as CriteriaAnswer;
      expect(answer.release).toBe('T.1');
      expect(answer.criteria.map((criterion) => criterion.key)).toEqual(EXPECTED_KEYS);
      // The strictest app type decides and the priority explains itself (CAT-02, CAT-14).
      expect(answer.criteria[0].resolvedPriority).toEqual({
        level: 'A',
        weight: null,
        rationale: 'Testbegründung A für web und hr.',
        source: 'expertJudgement',
        matchedRule: { kind: 'appTypeAndUseCase', appType: 'web', useCase: 'hr' },
      });

      await expect(page).toHaveURL(/\/kriterien\/ergebnis$/);
      await expect(page.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeFocused();
      await expectNoAxeViolations(page, testInfo, 'j1 s11');
    });

    await test.step('no request and no link went to a third-party origin', async () => {
      expect(thirdPartyRequests).toEqual([]);
      expect(await thirdPartyLinksInDocument(page, appOrigin)).toEqual([]);
    });
  });
});
