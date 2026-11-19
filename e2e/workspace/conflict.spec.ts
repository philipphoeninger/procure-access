// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import {
  createOrganizationThroughApi,
  createProjectThroughApi,
  invitationLinkOf,
  inviteThroughApi,
  registerConfirmedAccount,
} from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';

/**
 * The conflict dialog S37 (S6-21a; R-CAN-11, API-03; Handoff §5.1, Figma `2056:2519`): two members
 * of one organisation open the same project in two browser contexts. The Owner renames it first;
 * the Editor's save of the same project, read before that, is refused with 409 `conflict` and S37
 * names the Owner. „Meine Änderungen kopieren“ copies and announces, the dialog stays; Escape
 * returns to „Projekt bearbeiten“, whose „Speichern“ opens S37 again without a request; „Neue
 * Fassung laden“ closes both dialogs, reads the project and focuses its H1. Axe in both appearances
 * on S37 (A11Y-01, A11Y-08).
 */
const PASSWORD = 'e2e-Passwort-lang-genug';

const main = (page: Page) => page.getByRole('main');

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/** The XSRF token of the page's browser context, after the session probe issued it. */
async function xsrfToken(page: Page): Promise<string> {
  expect((await page.request.get('/api/v1/auth/session')).ok()).toBe(true);
  const token = (await page.context().cookies()).find((cookie) => cookie.name === 'XSRF-TOKEN')?.value;
  expect(token, 'the probe issues XSRF-TOKEN').toBeTruthy();
  return token!;
}

/** Opens „Projekt bearbeiten“ on S32 and returns the dialog with its name field focused. */
async function openEditDialog(page: Page): Promise<Locator> {
  await main(page).getByRole('button', { name: 'Projekt bearbeiten' }).press('Enter');
  const dialog = page.getByRole('dialog', { name: 'Projekt bearbeiten' });
  // MatDialog moves the focus when it has opened (S6-01)
  await expect(dialog.getByRole('textbox', { name: 'Name des Projekts' })).toBeFocused();
  return dialog;
}

test.describe('Conflict dialog S37', () => {
  test('INV:API-03 INV:A11Y-01 INV:A11Y-08 the second save of the same project gets S37 with the other editor; copy, Escape and „Neue Fassung laden“ work', async ({
    page,
    browser,
    browserName,
    request,
  }, testInfo) => {
    test.setTimeout(90_000);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const owner = `conflict-owner-${suffix}@example.test`;
    const editor = `conflict-editor-${suffix}@example.test`;
    await registerConfirmedAccount(request, owner, PASSWORD);
    await registerConfirmedAccount(request, editor, PASSWORD);

    // The Owner's organisation with the Editor as a member, and a project
    await signInThroughApi(page, owner, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    await inviteThroughApi(page, organizationId, editor, 'editor');
    const projectId = await createProjectThroughApi(page.request, organizationId, `Projekt ${suffix}`);

    const editorContext = await browser.newContext({ viewport: page.viewportSize() });
    if (browserName === 'chromium') await editorContext.grantPermissions(['clipboard-read', 'clipboard-write']);
    const other = await editorContext.newPage();
    try {
      await signInThroughApi(other, editor, PASSWORD);
      const link = await invitationLinkOf(request, editor, 2);
      const accepted = await other.request.post('/api/v1/invitations/accept', {
        headers: { 'X-XSRF-TOKEN': await xsrfToken(other) },
        data: { token: new URL(link, 'http://localhost').searchParams.get('token') },
      });
      expect(accepted.status(), 'the Editor accepts the invitation').toBe(200);

      // Both read the project
      await page.goto(`/projekte/${projectId}`);
      await other.goto(`/projekte/${projectId}`);
      await expect(main(page).getByRole('heading', { level: 1, name: `Projekt ${suffix}` })).toBeVisible();
      await expect(main(other).getByRole('heading', { level: 1, name: `Projekt ${suffix}` })).toBeVisible();

      // The Owner renames it first
      const ownerDialog = await openEditDialog(page);
      const ownerName = `Vom Owner umbenannt ${suffix}`;
      await ownerDialog.getByRole('textbox', { name: 'Name des Projekts' }).fill(ownerName);
      await ownerDialog.getByRole('button', { name: 'Speichern' }).press('Enter');
      await expect(main(page).getByRole('heading', { level: 1, name: ownerName })).toBeVisible();

      // The Editor saves the version read before: S37 names the Owner, the focus on „Meine Änderungen kopieren“
      let patches = 0;
      other.on('request', (sent) => {
        if (sent.method() === 'PATCH') patches++;
      });
      const editorDialog = await openEditDialog(other);
      const editorName = `Vom Editor umbenannt ${suffix}`;
      await editorDialog.getByRole('textbox', { name: 'Name des Projekts' }).fill(editorName);
      const refused = other.waitForResponse((response) => response.request().method() === 'PATCH');
      await editorDialog.getByRole('button', { name: 'Speichern' }).press('Enter');
      expect((await refused).status()).toBe(409);

      const s37 = other.getByRole('alertdialog', { name: 'Inzwischen geändert' });
      const copy = s37.getByRole('button', { name: 'Meine Änderungen kopieren' });
      await expect(copy).toBeFocused();
      await expect(s37).toContainText(`${owner} hat diesen Inhalt geändert, während Sie ihn bearbeitet haben.`);
      await expect(s37).toContainText('Wenn Sie „Neue Fassung laden“ wählen, gehen Ihre Änderungen verloren.');
      await settled(s37);
      await expectNoAxeViolations(other, testInfo, 's37-conflict');

      // „Meine Änderungen kopieren“: copied and announced, the dialog stays
      await copy.press('Enter');
      await expect(other.locator('.cdk-live-announcer-element')).toHaveText('Ihre Änderungen wurden kopiert.');
      await expect(s37).toBeVisible();
      if (browserName === 'chromium') {
        expect(await other.evaluate(() => navigator.clipboard.readText())).toBe(`Name des Projekts: ${editorName}`);
      }

      // Escape closes S37 only; „Speichern“ then opens it again without a request
      await other.keyboard.press('Escape');
      await expect(s37).toBeHidden();
      await expect(editorDialog.getByRole('textbox', { name: 'Name des Projekts' })).toHaveValue(editorName);
      await editorDialog.getByRole('button', { name: 'Speichern' }).press('Enter');
      await expect(copy).toBeFocused();
      expect(patches).toBe(1);

      // „Neue Fassung laden“: both dialogs close, the stored project shows, its H1 has the focus
      await s37.getByRole('button', { name: 'Neue Fassung laden' }).press('Enter');
      await expect(s37).toBeHidden();
      await expect(editorDialog).toBeHidden();
      await expect(main(other).getByRole('heading', { level: 1, name: ownerName })).toBeFocused();
      expect(patches).toBe(1);

      // Read again, the Editor's save works
      const reopened = await openEditDialog(other);
      await expect(reopened.getByRole('textbox', { name: 'Name des Projekts' })).toHaveValue(ownerName);
      await reopened.getByRole('textbox', { name: 'Name des Projekts' }).fill(editorName);
      await reopened.getByRole('button', { name: 'Speichern' }).press('Enter');
      await expect(main(other).getByRole('heading', { level: 1, name: editorName })).toBeVisible();
    } finally {
      await editorContext.close();
    }
  });
});
