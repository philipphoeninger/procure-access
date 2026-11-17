// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { of } from 'rxjs';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { FileSaver } from '@app/core/files/file-saver';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { Account } from '../../models/account';
import { AccountApiService } from '../../services/api/account-api.service';
import { AccountLoad } from './account-load';
import { AccountPage } from './account-page';

const EMAIL = 'anna@example.test';

const soleOwner = (organizations: string[]) =>
  new HttpErrorResponse({
    status: 409,
    error: { type: 'https://procureaccess.example/problems/sole-owner', organizations },
  });

describe('AccountPage (S61, R-AUTH-09, R-AUTH-10)', () => {
  let api: { export: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };
  let save: ReturnType<typeof vi.fn>;
  let open: ReturnType<typeof vi.fn>;
  let forgetUser: ReturnType<typeof vi.fn>;
  let focusPageHeading: ReturnType<typeof vi.fn>;
  let navigateByUrl: ReturnType<typeof vi.fn<Router['navigateByUrl']>>;

  /** Opens S61; `confirmed` is what the deletion dialog closes with. */
  async function openPage(options: { account?: Account | null; confirmed?: boolean } = {}) {
    api = { export: vi.fn(), delete: vi.fn().mockResolvedValue(undefined) };
    save = vi.fn();
    open = vi.fn().mockReturnValue({ afterClosed: () => of(options.confirmed ?? true) });
    forgetUser = vi.fn();
    focusPageHeading = vi.fn();
    const load = new AccountLoad();
    load.account = options.account === undefined ? { email: EMAIL, emailConfirmed: true } : options.account;
    const rendered = await render(AccountPage, {
      providers: [
        provideTestTranslations(),
        { provide: AccountLoad, useValue: load },
        { provide: AccountApiService, useValue: api },
        { provide: FileSaver, useValue: { save } },
        { provide: MatDialog, useValue: { open } },
        { provide: ProcureAccessStore, useValue: { forgetUser, session: () => ({ email: EMAIL }), resendConfirmation: vi.fn() } },
        provideRouter([]),
        { provide: PageTitleStrategy, useValue: { focusPageHeading } },
      ],
    });
    navigateByUrl = vi.fn<Router['navigateByUrl']>().mockResolvedValue(true);
    TestBed.inject(Router).navigateByUrl = navigateByUrl;
    return rendered;
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  const deleteButton = () => screen.getByRole('button', { name: 'Konto löschen' });

  it('shows the H1 and the two sections with their texts and actions', async () => {
    const { fixture } = await openPage();

    expect(screen.getByRole('heading', { level: 1, name: 'Konto' })).toBeTruthy();
    expect(screen.getAllByRole('heading', { level: 2 }).map((h) => h.textContent?.trim())).toEqual([
      'Ihre Daten herunterladen',
      'Konto löschen',
    ]);
    expect(screen.getByText(/Sie erhalten eine Datei \(JSON\)/)).toBeTruthy();
    // ⚖️ text as written, without the marker of the UX copy
    expect(screen.getByText(/^Wenn Sie Ihr Konto löschen, werden Ihre Anmeldedaten entfernt\./)).toBeTruthy();
    expect(screen.queryByText(/Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse/)).toBeNull();
    expect(screen.queryByRole('alert')).toBeNull();
    await expectNoAxeViolations(fixture.nativeElement as HTMLElement);
  });

  it('shows the confirmation notice with „erneut senden“ for an unconfirmed address', async () => {
    await openPage({ account: { email: EMAIL, emailConfirmed: false } });

    expect(screen.getByText(/Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse/)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Bestätigungs-E-Mail erneut senden' })).toBeTruthy();
    // Export and deletion need no confirmed address.
    expect(screen.getByRole('button', { name: 'Daten herunterladen' })).toBeTruthy();
    expect(deleteButton()).toBeTruthy();
  });

  it('hands the export file to the download under the name of the server', async () => {
    const { fixture } = await openPage();
    const content = new Blob(['{}'], { type: 'application/json' });
    api.export.mockResolvedValue({ content, fileName: 'ProcureAccess_Daten_2026-11-17.json' });

    screen.getByRole('button', { name: 'Daten herunterladen' }).click();
    await settle(fixture);

    expect(save).toHaveBeenCalledExactlyOnceWith(content, 'ProcureAccess_Daten_2026-11-17.json');
    expect(screen.getByRole('button', { name: 'Daten herunterladen' }).getAttribute('aria-disabled')).toBeNull();
  });

  it('a failed export saves nothing and leaves the button usable', async () => {
    const { fixture } = await openPage();
    api.export.mockRejectedValue(new HttpErrorResponse({ status: 500 }));

    screen.getByRole('button', { name: 'Daten herunterladen' }).click();
    await settle(fixture);

    expect(save).not.toHaveBeenCalled();
    expect(screen.getByRole('button', { name: 'Daten herunterladen' }).getAttribute('aria-disabled')).toBeNull();
  });

  it('asks with the account address and deletes nothing when the dialog is closed', async () => {
    const { fixture } = await openPage({ confirmed: false });

    deleteButton().click();
    await settle(fixture);

    expect(open.mock.calls[0][1]).toMatchObject({ data: { email: EMAIL } });
    expect(api.delete).not.toHaveBeenCalled();
    expect(forgetUser).not.toHaveBeenCalled();
  });

  it('deletes the account, forgets the user and opens the start page with the focus on its H1', async () => {
    const { fixture } = await openPage();

    deleteButton().click();
    await settle(fixture);

    expect(api.delete).toHaveBeenCalledTimes(1);
    expect(forgetUser).toHaveBeenCalledTimes(1);
    expect(navigateByUrl).toHaveBeenCalledExactlyOnceWith('/');
    expect(focusPageHeading).toHaveBeenCalledTimes(1);
    expect(forgetUser.mock.invocationCallOrder[0]).toBeLessThan(navigateByUrl.mock.invocationCallOrder[0]);
  });

  it('INV:TEN-11 a refused deletion names every organisation in an error notice and keeps the user signed in', async () => {
    const { fixture } = await openPage();
    api.delete.mockRejectedValue(soleOwner(['Ämterverbund', 'Organisation A']));

    deleteButton().click();
    await settle(fixture);

    const alert = screen.getByRole('alert');
    expect(alert.textContent).toContain('Konto nicht gelöscht');
    expect(Array.from(alert.querySelectorAll('.pa-notice__text p')).map((p) => p.textContent?.trim())).toEqual([
      'Sie sind das einzige Mitglied mit der Rolle „Verwalten“ in Ämterverbund. Geben Sie diese Rolle zuerst einem anderen Mitglied, bevor Sie Ihr Konto löschen.',
      'Sie sind das einzige Mitglied mit der Rolle „Verwalten“ in Organisation A. Geben Sie diese Rolle zuerst einem anderen Mitglied, bevor Sie Ihr Konto löschen.',
    ]);
    expect(forgetUser).not.toHaveBeenCalled();
    expect(navigateByUrl).not.toHaveBeenCalled();
    expect(deleteButton().getAttribute('aria-disabled')).toBeNull();
    await expectNoAxeViolations(fixture.nativeElement as HTMLElement);

    // A new attempt clears the notice before the dialog opens.
    open.mockReturnValue({ afterClosed: () => of(false) });
    deleteButton().click();
    await settle(fixture);
    expect(screen.queryByRole('alert')).toBeNull();
  });

  it('another failure shows no notice of the page and keeps the user signed in', async () => {
    const { fixture } = await openPage();
    api.delete.mockRejectedValue(new HttpErrorResponse({ status: 500 }));

    deleteButton().click();
    await settle(fixture);

    expect(screen.queryByRole('alert')).toBeNull();
    expect(forgetUser).not.toHaveBeenCalled();
  });

  it('shows the S80 server text when the account could not be read', async () => {
    await openPage({ account: null });

    expect(screen.queryByRole('heading', { level: 1, name: 'Konto' })).toBeNull();
    expect(screen.queryByRole('button', { name: 'Konto löschen' })).toBeNull();
  });
});
