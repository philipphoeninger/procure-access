// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { readFile } from 'node:fs/promises';
import { expect, test, type Locator, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { recordThirdPartyRequests, thirdPartyLinksInDocument } from '../support/third-party-requests';
import { chooseFirstOption } from '../support/combobox';
import { readXlsxSheet } from '../support/xlsx';
import { organizationOfAccount, registerConfirmedAccount } from '../support/accounts';
import { accountMenuButton } from '../support/session';
import { arrowTo, DOWNLOAD_TIMEOUT, settled, tabTo } from '../support/keyboard';

/**
 * J1, configurator without account (TESTING §5.4), on S10 and the active test release T.1 of the
 * E2E stack, with the keyboard only (A11Y-02): the test never clicks, taps or moves the pointer,
 * and it reaches every control with Tab from where the page put the focus. Axe runs on every state
 * in both appearances (A11Y-01, A11Y-08). After the hand-over (S4-13) it goes on on S11 — the
 * rationale of a priority, a priority change, a note, an excluded criterion and the „Geprüft nach …“
 * card — and exports the list as XLSX through S13, then opens the downloaded file and checks it
 * (S4-22). The second part (S5-20b) is a test of its own on a fresh page: a draft of the same
 * selection, built by keyboard and still signed out, goes to „Liste speichern“, which opens S01 with
 * the reason; before that the draft is exported once. After the sign-in S11 shows the draft
 * unchanged and says so, S12 saves it under a name, the page becomes the saved list (editable for
 * its Owner, S5-21b), whose export through S13 holds the same file as the draft's but for the
 * moment it was made (EXP-06; with it J1's second part and the M3 exit criterion, owner decision
 * S5-20 7a), „Kriterienlisten“ in the account menu opens S14 with it, and „Liste löschen“ deletes it
 * after asking. The account and its organisation are the test's own, built through the API (owner
 * decision S5-20 6a).
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

/** The password of the account J1 builds for its second part (test data, not a secret). */
const PASSWORD = 'e2e-Passwort-lang-genug';
const LIST_NAME = 'Vergabe Personalsoftware 2027';

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

/** The S11 card whose H2 is the title. */
function card(page: Page, title: string): Locator {
  return page
    .getByRole('main')
    .getByRole('listitem')
    .filter({ has: page.getByRole('heading', { level: 2, name: title, exact: true }) });
}

/**
 * „Liste exportieren“ → S13 → „Datei herunterladen“ by keyboard, on S11 or a saved list; returns
 * the XLSX after checking its name (by default the name of a draft, SPEC §8.3).
 */
async function exportThroughS13(page: Page, fileName = /^ProcureAccess_Kriterien_T\.1_\d{4}-\d{2}-\d{2}\.xlsx$/): Promise<Buffer> {
  await tabTo(page, page.getByRole('button', { name: 'Liste exportieren' }));
  await page.keyboard.press('Enter');
  const dialog = page.getByRole('dialog', { name: 'Kriterienliste exportieren' });
  const xlsx = dialog.getByRole('radio', { name: 'Excel-Datei (.xlsx) – für Vergabeunterlagen' });
  await expect(xlsx).toBeChecked();
  // The first focus (S13: the selected format) comes when the dialog has opened; tabbing before it
  // could reach „Datei herunterladen“ and lose the focus to the radio again, so Enter downloaded
  // nothing (CI of #362, S5-25b).
  await expect(xlsx).toBeFocused();
  await tabTo(page, dialog.getByRole('button', { name: 'Datei herunterladen' }));
  const downloaded = page.waitForEvent('download', { timeout: DOWNLOAD_TIMEOUT });
  await page.keyboard.press('Enter');
  const download = await downloaded;
  await expect(dialog).toHaveCount(0);
  expect(download.suggestedFilename()).toMatch(fileName);
  return readFile(await download.path());
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
      await expect(priority).toContainText('Geändert. Empfohlen war: A – Mindestanforderung.');
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
      const include = excluded.getByRole('checkbox', { name: `In Liste aufnehmen ${CHANGED.excluded.title}`, exact: true });
      await tabTo(page, include);
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
      const include = testedBy.getByRole('checkbox', { name: 'In Liste aufnehmen Geprüft nach BIK BITV-Test (Web)', exact: true });
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
      const downloaded = page.waitForEvent('download', { timeout: DOWNLOAD_TIMEOUT });
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

  test('INV:A11Y-02 INV:A11Y-01 INV:A11Y-08 INV:EXP-06 second part: the draft survives the sign-in from „Liste speichern“, is saved, opened, exported again with the same file, listed and deleted (S5-20b, S5-21b)', async ({ page, request }, testInfo) => {
    // Nine states, each checked by axe in two appearances, after S10 and S11 by keyboard.
    test.setTimeout(300_000);
    /** The file of the draft, exported before saving; the saved list's file must hold the same. */
    let draftFile: Buffer | undefined;
    const checkbox = (name: string) => page.getByRole('checkbox', { name, exact: true });
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const address = `j1-${suffix}@example.test`;
    const organization = `Vergabestelle ${suffix}`;

    await test.step('second part prepared: an own confirmed account with an organisation, through the API (S5-20 6a)', async () => {
      // The request context has its own cookies: the page stays signed out until S01.
      await registerConfirmedAccount(request, address, PASSWORD);
      await organizationOfAccount(request, address, PASSWORD, organization);
    });

    await test.step('a draft by keyboard: the selection of the first part on S10, then S11', async () => {
      // A direct load: the keyboard starts at the top of the document (the first part covers the start page).
      await page.goto('/kriterien');
      await expect(page.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeVisible();
      const combobox = page.getByRole('combobox', { name: 'Verwendungszweck' });
      await tabTo(page, combobox);
      await page.keyboard.type('HR');
      await chooseFirstOption(page, combobox, 'HR-Anwendung');
      await next(page, 'Was möchten Sie beschaffen?');
      await check(page, checkbox('Website oder Webanwendung'));
      await check(page, checkbox('Software (offen)'));
      await next(page, 'Welche Funktionen hat das Produkt?');
      await check(page, checkbox('Audio'));
      await next(page, /Soll das Produkt nach einem bestimmten Verfahren geprüft sein/);
      await check(page, checkbox('BIK BITV-Test (Web)'));
      await tabTo(page, page.getByRole('button', { name: 'Kriterienliste anzeigen' }));
      await page.keyboard.press('Enter');
      await expect(page.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeFocused();
      await expect(page.getByRole('main').getByRole('heading', { level: 2 })).toHaveCount(EXPECTED_KEYS.length);
    });

    await test.step('S11: 99.1 to Hinweis and 99.7 left out, as in the first part', async () => {
      const priority = card(page, CHANGED.priority.title);
      await tabTo(page, priority.getByRole('button', { name: 'Priorität: A – Mindestanforderung' }));
      await page.keyboard.press('Enter');
      const menu = page.getByRole('menu', { name: 'Priorität für dieses Kriterium' });
      await arrowTo(page, menu.getByRole('menuitemradio', { name: 'Hinweis' }));
      await page.keyboard.press('Enter');
      await expect(priority.getByRole('button', { name: 'Priorität: Hinweis' })).toBeFocused();

      const include = card(page, CHANGED.excluded.title).getByRole('checkbox', { name: `In Liste aufnehmen ${CHANGED.excluded.title}`, exact: true });
      await tabTo(page, include);
      await page.keyboard.press('Space');
      await expect(include).not.toBeChecked();
    });

    await test.step('S13: the draft exported as XLSX before saving (EXP-06, the file to compare)', async () => {
      draftFile = await exportThroughS13(page);
    });

    await test.step('S11 „Liste speichern“, signed out, opens S01 with the reason (R-SAV-01, R-AUTH-03)', async () => {
      await tabTo(page, page.getByRole('button', { name: 'Liste speichern' }));
      await page.keyboard.press('Enter');

      await expect(page.getByRole('heading', { level: 1, name: 'Anmelden' })).toBeFocused();
      await expect(page.getByRole('main')).toContainText('Melden Sie sich an, um Ihre Kriterienliste zu speichern. Ihre Auswahl bleibt dabei erhalten.');
      await settled(page.getByRole('main'));
      await expectNoAxeViolations(page, testInfo, 'j1 s01 save');
    });

    await test.step('S01: sign-in by keyboard; S11 again with the draft as changed and the hint (S5-20b 1a)', async () => {
      await tabTo(page, page.getByRole('main').getByLabel('E-Mail-Adresse'));
      await page.keyboard.type(address);
      await tabTo(page, page.getByRole('main').getByLabel('Passwort', { exact: true }));
      await page.keyboard.type(PASSWORD);
      await page.keyboard.press('Enter');

      await expect(page).toHaveURL(/\/kriterien\/ergebnis$/);
      const main = page.getByRole('main');
      await expect(main).toContainText('Sie sind angemeldet. Ihre Auswahl ist noch da und kann jetzt gespeichert werden.');
      // The draft as changed before the sign-in: the priority of 99.1 and the excluded 99.7.
      await expect(card(page, CHANGED.priority.title).getByRole('button', { name: 'Priorität: Hinweis' })).toBeVisible();
      await expect(card(page, CHANGED.excluded.title)).toContainText('Nicht in Ihrer Liste');
      await expect(main).not.toContainText('Ihre Auswahl wird nicht gespeichert, solange Sie nicht angemeldet sind');
      await settled(main);
      await expectNoAxeViolations(page, testInfo, 'j1 s11 signed in');
    });

    await test.step('S12: „Liste speichern“ saves the draft under a name for the organisation', async () => {
      await tabTo(page, page.getByRole('button', { name: 'Liste speichern' }));
      await page.keyboard.press('Enter');

      const dialog = page.getByRole('dialog', { name: 'Kriterienliste speichern' });
      const name = dialog.getByRole('textbox', { name: 'Name der Liste' });
      await expect(name).toBeFocused();
      await expect(dialog).toContainText(`Die Liste ist für alle Mitglieder Ihrer Organisation ${organization} sichtbar.`);
      await settled(dialog);
      await expectNoAxeViolations(page, testInfo, 'j1 s12');

      const saved = page.waitForResponse((response) => new URL(response.url()).pathname === '/api/v1/configurations');
      await page.keyboard.type(LIST_NAME);
      await page.keyboard.press('Enter');
      expect((await saved).status()).toBe(201);
    });

    await test.step('the saved list: S11 with its name as H1, the decisions as saved, editable for its Owner (S5-20 4a, S5-21b)', async () => {
      await expect(page).toHaveURL(/\/kriterienlisten\/[0-9a-f-]{36}$/);
      await expect(page.getByRole('heading', { level: 1, name: LIST_NAME })).toBeFocused();
      await expect(page).toHaveTitle(`${LIST_NAME} – Gespeicherte Kriterienlisten – ProcureAccess`);
      const main = page.getByRole('main');
      await expect(main.getByRole('heading', { level: 2 })).toHaveCount(EXPECTED_KEYS.length);
      await expect(card(page, CHANGED.priority.title).getByRole('button', { name: 'Priorität: Hinweis' })).toBeVisible();
      await expect(card(page, CHANGED.priority.title)).toContainText('Geändert. Empfohlen war: A – Mindestanforderung');
      await expect(card(page, CHANGED.excluded.title)).toContainText('Nicht in Ihrer Liste');
      await expect(card(page, CHANGED.excluded.title).getByRole('checkbox')).not.toBeChecked();
      await expect(main.getByRole('link', { name: 'Auswahl ändern' })).toBeVisible();
      await settled(main);
      await expectNoAxeViolations(page, testInfo, 'j1 saved list');
    });

    await test.step('S13 of the saved list: the same file as the draft before saving (EXP-06, M3 exit criterion)', async () => {
      const savedFile = await exportThroughS13(page, /^ProcureAccess_Kriterien_Vergabe-Personalsoftware-2027_T\.1_\d{4}-\d{2}-\d{2}\.xlsx$/);
      expect(readXlsxSheet(savedFile, 'Kriterien')).toEqual(readXlsxSheet(draftFile!, 'Kriterien'));
      // „Info“ alike but for the moment the file was made.
      const info = (file: Buffer) => readXlsxSheet(file, 'Info').filter((row) => row[0] !== 'Erstellt am');
      expect(info(savedFile)).toEqual(info(draftFile!));
    });

    await test.step('S14 through „Kriterienlisten“ of the account menu, sorted by name (R-SAV-02)', async () => {
      if (await accountMenuButton(page).isVisible()) {
        // Back up to the header: past the end of the page Firefox goes on inside `main` (#355).
        await tabTo(page, accountMenuButton(page), 'Shift+Tab');
        await page.keyboard.press('Enter');
        await arrowTo(page, page.getByRole('menu').getByRole('menuitem', { name: 'Kriterienlisten' }));
      } else {
        await tabTo(page, page.getByRole('button', { name: 'Menü', exact: true }));
        await page.keyboard.press('Enter');
        await tabTo(page, page.getByRole('dialog', { name: 'Menü' }).getByRole('link', { name: 'Kriterienlisten' }));
      }
      await page.keyboard.press('Enter');

      await expect(page).toHaveURL(/\/kriterienlisten$/);
      await expect(page.getByRole('heading', { level: 1, name: 'Gespeicherte Kriterienlisten' })).toBeFocused();
      const main = page.getByRole('main');
      await expect(main.getByRole('link', { name: LIST_NAME })).toBeVisible();
      await settled(main);
      await expectNoAxeViolations(page, testInfo, 'j1 s14');

      // mat-select keeps the focus on the combobox and points at the active option.
      const sort = main.getByRole('combobox', { name: 'Sortieren nach' });
      await tabTo(page, sort);
      await page.keyboard.press('Enter');
      const optionId = await page.getByRole('listbox').getByRole('option', { name: 'Name' }).getAttribute('id');
      for (let presses = 0; (await sort.getAttribute('aria-activedescendant')) !== optionId; presses++) {
        expect(presses, 'the option „Name“ is reached with the arrow keys').toBeLessThan(5);
        await page.keyboard.press('ArrowDown');
      }
      await page.keyboard.press('Enter');
      await expect(sort).toContainText('Name');
      await expect(page.getByRole('listbox')).toHaveCount(0);
    });

    await test.step('S14: „Liste löschen“ asks first, then deletes the list (R-SAV-06, S5-21 7a)', async () => {
      const main = page.getByRole('main');
      await tabTo(page, main.getByRole('button', { name: 'Liste löschen' }));
      await page.keyboard.press('Enter');

      const dialog = page.getByRole('dialog', { name: 'Kriterienliste löschen?' });
      await expect(dialog).toContainText(`Die Liste „${LIST_NAME}“ wird gelöscht.`);
      await settled(dialog);
      await expectNoAxeViolations(page, testInfo, 'j1 s14 delete');
      const deleted = page.waitForResponse((response) => response.request().method() === 'DELETE');
      await tabTo(page, dialog.getByRole('button', { name: 'Liste löschen' }));
      await page.keyboard.press('Enter');
      expect((await deleted).status()).toBe(204);

      await expect(dialog).toHaveCount(0);
      await expect(page.getByRole('heading', { level: 1, name: 'Gespeicherte Kriterienlisten' })).toBeFocused();
      await expect(main).toContainText('Ihre Organisation hat noch keine Kriterienlisten gespeichert.');
    });

  });
});
