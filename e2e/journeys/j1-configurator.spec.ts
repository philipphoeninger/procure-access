// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { readFile } from 'node:fs/promises';
import { expect, test, type Locator, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { recordThirdPartyRequests, thirdPartyLinksInDocument } from '../support/third-party-requests';
import { chooseFirstOption } from '../support/combobox';
import { readXlsxSheet } from '../support/xlsx';

/**
 * J1, configurator without account (TESTING §5.4), on S10 and the active test release T.1 of the
 * E2E stack, with the keyboard only (A11Y-02): the test never clicks, taps or moves the pointer,
 * and it reaches every control with Tab from where the page put the focus. Axe runs on every state
 * in both appearances (A11Y-01, A11Y-08). After the hand-over (S4-13) it goes on on S11 — the
 * rationale of a priority, a priority change, a note, an excluded criterion and the „Geprüft nach …“
 * card — and exports the list as XLSX through S13, then opens the downloaded file and checks it
 * (S4-22). The second part (save after sign-in) comes with step 3.
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

/** The criteria J1 changes on S11, by their catalog title in T.1. */
const CHANGED = {
  priority: {
    key: '99.1',
    title: 'Testkriterium mit Regeln auf allen vier Ebenen',
    shortDescription: 'Testkurzbeschreibung: Das Kriterium hat Regeln auf allen vier Ebenen.',
  },
  note: { key: '99.2', title: 'Testkriterium für mehrere Anwendungstypen' },
  excluded: { key: '99.7', title: 'Testkriterium 99.7' },
} as const;

const NOTE = 'Mit der Vergabestelle abstimmen: gilt auch für das Bewerberportal.';

/** The header row of the sheet „Kriterien“ (SPEC §8.3, UX copy `exportFile.column.*`). */
const XLSX_HEADERS = [
  'Nr.',
  'Kriterium',
  'Nummer',
  'Titel',
  'Originaltitel',
  'Kurzbeschreibung',
  'Funktionsbereich',
  'Priorität (empfohlen)',
  'Priorität (Ihre Wahl)',
  'Gewichtung',
  'Begründung der Empfehlung',
  'Ihre Notiz',
  'Link zum Prüfschritt',
];

// S11 with an open detail panel has more tab stops than S10.
const MAX_TABS = 80;

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

/**
 * The texts an element's `aria-labelledby` points to, i.e. its accessible name. Used for the include
 * checkbox of an S11 card: Playwright's own name computation of that checkbox crashes the page in
 * Firefox once axe has run on S11 (S4-22; the class of TESTING V129 and S4-16 P5).
 */
function labelledBy(locator: Locator): Promise<string> {
  return locator.evaluate((element) =>
    (element.getAttribute('aria-labelledby') ?? '')
      .split(/\s+/)
      .map((id) => document.getElementById(id)?.textContent?.trim() ?? '')
      .join(' '),
  );
}

/** The S11 card whose H2 is the title. */
function card(page: Page, title: string): Locator {
  return page
    .getByRole('main')
    .getByRole('listitem')
    .filter({ has: page.getByRole('heading', { level: 2, name: title, exact: true }) });
}

test.describe('J1 configurator without account', () => {
  test('INV:A11Y-02 INV:A11Y-01 INV:A11Y-08 anonymous visitor chooses a selection on S10 by keyboard and gets the criteria of T.1', async ({ page, baseURL }, testInfo) => {
    // Twelve states, each checked by axe in two appearances, take far longer than the default 30 s.
    test.setTimeout(240_000);
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
      // Wait for the filtered list, so axe checks it.
      await expect(page.getByRole('listbox').getByRole('option').first()).toHaveAccessibleName('HR-Anwendung');
      await settled(page.getByRole('listbox'));
      await expectNoAxeViolations(page, testInfo, 'j1 step 1, list filtered');

      await chooseFirstOption(page, combobox, 'HR-Anwendung');
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
      await expect(page.getByRole('main').getByRole('heading', { level: 2 })).toHaveCount(EXPECTED_KEYS.length);
      await settled(page.getByRole('main'));
      await expectNoAxeViolations(page, testInfo, 'j1 s11');
    });

    await test.step('S11: change the priority of 99.1 from A to Hinweis with the K5 menu (R-LST-03)', async () => {
      const priority = card(page, CHANGED.priority.title);
      const levelButton = priority.getByRole('button', { name: 'Priorität: A – Mindestanforderung' });
      await tabTo(page, levelButton);
      await page.keyboard.press('Enter');

      const menu = page.getByRole('menu', { name: 'Priorität für dieses Kriterium' });
      await expect(menu.getByRole('menuitemradio', { name: 'A – Mindestanforderung' })).toBeFocused();
      await settled(menu);
      await expectNoAxeViolations(page, testInfo, 'j1 s11 priority menu');
      await page.keyboard.press('ArrowDown');
      await page.keyboard.press('ArrowDown');
      await expect(menu.getByRole('menuitemradio', { name: 'Hinweis' })).toBeFocused();
      await page.keyboard.press('Enter');

      // The menu closes, the focus returns to the level button, and the card names the recommendation.
      await expect(menu).toHaveCount(0);
      await expect(priority.getByRole('button', { name: 'Priorität: Hinweis' })).toBeFocused();
      await expect(priority).toContainText('Von Ihnen geändert. Empfohlen war: A – Mindestanforderung.');
      await expect(priority.getByRole('button', { name: 'Empfehlung wiederherstellen' })).toBeVisible();
    });

    await test.step('S11: the detail panel of 99.1 explains the recommended priority (CAT-02)', async () => {
      const priority = card(page, CHANGED.priority.title);
      const details = priority.getByRole('button', { name: 'Details anzeigen' });
      await tabTo(page, details);
      await page.keyboard.press('Enter');

      await expect(priority.getByRole('button', { name: 'Details ausblenden' })).toHaveAttribute('aria-expanded', 'true');
      await expect(priority.getByRole('heading', { level: 3, name: 'Warum diese Priorität?' })).toBeVisible();
      await expect(priority).toContainText('Testbegründung A für web und hr.');
      await expect(priority).toContainText('Diese Empfehlung gilt für Website oder Webanwendung mit dem Verwendungszweck HR-Anwendung.');
      await expect(priority).toContainText('Grundlage: Empfehlung des Projektteams WERTE.IT');
      await settled(page.getByRole('main'));
      await expectNoAxeViolations(page, testInfo, 'j1 s11 details and override');
    });

    await test.step('S11: a note on 99.2 (R-LST-05)', async () => {
      const noted = card(page, CHANGED.note.title);
      await tabTo(page, noted.getByRole('button', { name: 'Notiz hinzufügen' }));
      await page.keyboard.press('Enter');

      const field = noted.getByRole('textbox', { name: 'Ihre Notiz zu diesem Kriterium (optional)' });
      await expect(field).toBeFocused();
      await page.keyboard.type(NOTE);
      await expect(field).toHaveValue(NOTE);
    });

    await test.step('S11: 99.7 left out of the list (R-LST-04)', async () => {
      const excluded = card(page, CHANGED.excluded.title);
      // The card has one checkbox; its name is read through `labelledBy` (Firefox, see there).
      const include = excluded.getByRole('checkbox');
      await tabTo(page, include);
      expect(await labelledBy(include)).toBe(`In Liste aufnehmen ${CHANGED.excluded.title}`);
      await expect(include).toBeChecked();
      await page.keyboard.press('Space');

      await expect(include).not.toBeChecked();
      await expect(excluded).toContainText('Nicht in Ihrer Liste');
      // The excluded card stays in the list; the counter of the action bar counts it out.
      await expect(page.getByRole('main').getByRole('heading', { level: 2 })).toHaveCount(EXPECTED_KEYS.length);
      await expect(page.getByRole('main')).toContainText(`${EXPECTED_KEYS.length - 1} von ${EXPECTED_KEYS.length} Kriterien in Ihrer Liste`);
      await settled(page.getByRole('main'));
      await expectNoAxeViolations(page, testInfo, 'j1 s11 note and excluded');
    });

    await test.step('S11: the procedure criterion „Geprüft nach BIK BITV-Test (Web)“ (CAT-17)', async () => {
      const testedBy = card(page, 'Geprüft nach BIK BITV-Test (Web)');
      await expect(testedBy).toContainText('Der Anbieter oder eine unabhängige Stelle hat das Produkt nach dem BIK BITV-Test (Web) geprüft');
      const include = testedBy.getByRole('checkbox');
      expect(await labelledBy(include)).toBe('In Liste aufnehmen Geprüft nach BIK BITV-Test (Web)');
      await expect(include).toBeChecked();
    });

    await test.step('S13: „Liste exportieren“ opens the dialog on the preselected XLSX format', async () => {
      await tabTo(page, page.getByRole('button', { name: 'Liste exportieren' }));
      await page.keyboard.press('Enter');

      const dialog = page.getByRole('dialog', { name: 'Kriterienliste exportieren' });
      await expect(dialog.getByRole('radio', { name: 'Excel-Datei (.xlsx) – für Vergabeunterlagen' })).toBeFocused();
      await expect(dialog.getByRole('radio', { name: 'Excel-Datei (.xlsx) – für Vergabeunterlagen' })).toBeChecked();
      await expect(dialog).toContainText(`Exportiert werden ${EXPECTED_KEYS.length - 1} Kriterien aus Ihrer Liste`);
      await settled(dialog);
      await expectNoAxeViolations(page, testInfo, 'j1 s13');
    });

    await test.step('S13: „Datei herunterladen“ downloads the XLSX, which holds the list as changed on S11 (EXP-01)', async () => {
      const dialog = page.getByRole('dialog', { name: 'Kriterienliste exportieren' });
      await tabTo(page, dialog.getByRole('button', { name: 'Datei herunterladen' }));
      const downloaded = page.waitForEvent('download');
      await page.keyboard.press('Enter');
      const download = await downloaded;

      // The dialog closes and MatDialog returns the focus to the button that opened it.
      await expect(dialog).toHaveCount(0);
      await expect(page.getByRole('button', { name: 'Liste exportieren' })).toBeFocused();

      expect(download.suggestedFilename()).toMatch(/^ProcureAccess_Kriterien_T\.1_\d{4}-\d{2}-\d{2}\.xlsx$/);
      const rows = readXlsxSheet(await readFile(await download.path()), 'Kriterien');
      const [header, ...criteria] = rows;
      expect(header).toEqual(XLSX_HEADERS);

      // One row per criterion in the list, in catalog order, without the excluded one.
      const column = (name: string) => XLSX_HEADERS.indexOf(name);
      expect(criteria.map((row) => row[column('Kriterium')])).toEqual(EXPECTED_KEYS.filter((key) => key !== CHANGED.excluded.key));
      const row = (key: string) => criteria.find((cells) => cells[column('Kriterium')] === key)!;
      expect(row(CHANGED.priority.key)[column('Kurzbeschreibung')]).toBe(CHANGED.priority.shortDescription);
      expect(row(CHANGED.note.key)[column('Kurzbeschreibung')]).toBe('');
      expect(row(CHANGED.priority.key)[column('Priorität (empfohlen)')]).toBe('A – Mindestanforderung');
      expect(row(CHANGED.priority.key)[column('Priorität (Ihre Wahl)')]).toBe('Hinweis');
      expect(row(CHANGED.priority.key)[column('Begründung der Empfehlung')]).toBe('Testbegründung A für web und hr.');
      expect(row(CHANGED.note.key)[column('Ihre Notiz')]).toBe(NOTE);
      expect(row('PROC-bikWeb')[column('Titel')]).toBe('Geprüft nach BIK BITV-Test (Web)');
    });

    await test.step('no request and no link went to a third-party origin', async () => {
      expect(thirdPartyRequests).toEqual([]);
      expect(await thirdPartyLinksInDocument(page, appOrigin)).toEqual([]);
    });
  });
});
