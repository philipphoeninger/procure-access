// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { ShareLinksResponse } from '../../models/share-links';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { ShareDialog } from './share-dialog';

/** The page button that opens S15, so the test sees the focus come back to it. */
@Component({ template: '<button type="button" (click)="open()">Freigabelink erstellen</button>' })
class Opener {
  private readonly dialog = inject(MatDialog);

  open(): void {
    ShareDialog.open(this.dialog, { listId: 'list-1' });
  }
}

const base = 'https://procureaccess.de/problems/';

function problem(status: number, body: object): HttpErrorResponse {
  return new HttpErrorResponse({ status, error: body });
}

function linksAnswer(links: ShareLinksResponse['links'] = []): ShareLinksResponse {
  return { expiry: { earliest: '2026-11-17', default: '2027-02-14', latest: '2027-11-16' }, links };
}

const LINK_A = { id: 'link-a', createdAt: '2026-11-02T10:00:00Z', expiresAt: '2026-12-01T23:00:00Z', expiresOn: '2026-12-01' };
const LINK_B = { id: 'link-b', createdAt: '2026-11-10T10:00:00Z', expiresAt: '2027-01-31T23:00:00Z', expiresOn: '2027-01-31' };

describe('S15 Freigabelink erstellen (R-SAV-04, SD15)', () => {
  let shareLinks: ReturnType<typeof vi.fn>;
  let createShareLink: ReturnType<typeof vi.fn>;
  let revokeShareLink: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;

  async function openDialog({
    list = vi.fn().mockResolvedValue(linksAnswer()),
    create = vi.fn().mockResolvedValue({ id: 'link-new', token: 'TOKEN_43', expiresAt: '2027-02-14T23:00:00Z' }),
    revoke = vi.fn().mockResolvedValue(null),
  }: { list?: ReturnType<typeof vi.fn>; create?: ReturnType<typeof vi.fn>; revoke?: ReturnType<typeof vi.fn> } = {}) {
    shareLinks = list;
    createShareLink = create;
    revokeShareLink = revoke;
    announce = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(Opener, {
      providers: [
        provideTestTranslations(),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: ConfiguratorApiService, useValue: { shareLinks, createShareLink, revokeShareLink } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: ProcureAccessStore, useValue: { session: signal(null), resendConfirmation: vi.fn() } },
      ],
    });
    const opener = screen.getByRole('button', { name: 'Freigabelink erstellen' });
    opener.focus();
    opener.click();
    await settle(rendered.fixture);
    return { fixture: rendered.fixture, opener, dialog: screen.getByRole('dialog', { name: 'Freigabelink erstellen' }) };
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }): Promise<void> {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  const dateField = (dialog: HTMLElement) => within(dialog).getByLabelText<HTMLInputElement>(/^Gültig bis/);

  it('INV:TEN-12 offers the server\'s default, tomorrow to one year, and puts the first focus into the date', async () => {
    const { dialog } = await openDialog();

    expect(shareLinks).toHaveBeenCalledWith('list-1');
    const field = dateField(dialog);
    expect(field.type).toBe('date');
    expect(field.value).toBe('2027-02-14');
    expect(field.min).toBe('2026-11-17');
    expect(field.max).toBe('2027-11-16');
    expect(document.activeElement).toBe(field);
    expect(dialog.textContent).toContain('Wer diesen Link hat, kann die Liste lesen, auch ohne Konto, aber nicht ändern.');
    expect(dialog.textContent).toContain('Vorgabe: in 90 Tagen. Höchstens ein Jahr.');
    expect(within(dialog).getByRole('heading', { level: 3, name: 'Gültige Links' })).toBeTruthy();
    expect(dialog.textContent).toContain('Für diese Liste gibt es keinen gültigen Link.');
    await expectNoAxeViolations();
  });

  it('INV:TEN-12 refuses a missing date or one after the maximum with the error summary', async () => {
    const { fixture, dialog } = await openDialog();
    const field = dateField(dialog);

    fireEvent.input(field, { target: { value: '2027-11-17' } });
    within(dialog).getByRole('button', { name: 'Link erstellen' }).click();
    await settle(fixture);

    expect(createShareLink).not.toHaveBeenCalled();
    // The error summary takes the focus and names the field (Handoff §6.4).
    expect(document.activeElement?.textContent).toContain('Gültig bis: Bitte wählen Sie ein Datum zwischen morgen und 16.11.2027.');
    expect(field.getAttribute('aria-invalid')).toBe('true');

    fireEvent.input(field, { target: { value: '' } });
    within(dialog).getByRole('button', { name: 'Link erstellen' }).click();
    await settle(fixture);
    expect(createShareLink).not.toHaveBeenCalled();
  });

  it('creates a link, shows it once with its hint and focus, copies it and lists the links again', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    const list = vi
      .fn()
      .mockResolvedValueOnce(linksAnswer())
      .mockResolvedValueOnce(linksAnswer([{ id: 'link-new', createdAt: '2026-11-16T20:00:00Z', expiresAt: '2027-02-14T23:00:00Z', expiresOn: '2027-02-14' }]));
    const { fixture, dialog } = await openDialog({ list });

    within(dialog).getByRole('button', { name: 'Link erstellen' }).click();
    await settle(fixture);

    expect(createShareLink).toHaveBeenCalledWith('list-1', '2027-02-14');
    const link = within(dialog).getByRole<HTMLInputElement>('textbox', { name: 'Freigabelink' });
    expect(link.value).toBe(`${location.origin}/geteilt/TOKEN_43`);
    expect(link.readOnly).toBe(true);
    expect(document.activeElement).toBe(link);
    expect(dialog.textContent).toContain('Kopieren Sie den Link jetzt. Später können Sie ihn nicht mehr anzeigen lassen.');
    expect(within(dialog).getAllByRole('listitem')).toHaveLength(1);
    expect(dialog.textContent).toMatch(/gültig bis 14\.02\.2027/);

    within(dialog).getByRole('button', { name: 'Link kopieren' }).click();
    await settle(fixture);
    expect(writeText).toHaveBeenCalledWith(`${location.origin}/geteilt/TOKEN_43`);
    expect(announce).toHaveBeenCalledWith('criteria.share.copied');
    await expectNoAxeViolations();
  });

  it('selects the link and says so when the browser refuses the clipboard', async () => {
    Object.defineProperty(navigator, 'clipboard', {
      value: { writeText: vi.fn().mockRejectedValue(new Error('denied')) },
      configurable: true,
    });
    const { fixture, dialog } = await openDialog();
    within(dialog).getByRole('button', { name: 'Link erstellen' }).click();
    await settle(fixture);

    within(dialog).getByRole('button', { name: 'Link kopieren' }).click();
    await settle(fixture);

    expect(dialog.textContent).toContain('Der Link konnte nicht kopiert werden. Bitte markieren und kopieren Sie ihn selbst.');
    expect(document.activeElement).toBe(within(dialog).getByRole('textbox', { name: 'Freigabelink' }));
    expect(announce).not.toHaveBeenCalledWith('criteria.share.copied');
  });

  it('lists the active links and revokes one at once, the focus to the next link, then to the heading (S5-24 3a)', async () => {
    const { fixture, dialog } = await openDialog({ list: vi.fn().mockResolvedValue(linksAnswer([LINK_A, LINK_B])) });
    const buttons = within(dialog).getAllByRole('button', { name: 'Link ungültig machen' });
    expect(buttons).toHaveLength(2);
    expect(buttons[0].getAttribute('aria-describedby')).toBeTruthy();
    expect(document.getElementById(buttons[0].getAttribute('aria-describedby') ?? '')?.textContent).toMatch(
      /Erstellt am 02\.11\.2026, gültig bis 01\.12\.2026/,
    );

    buttons[0].click();
    await settle(fixture);

    expect(revokeShareLink).toHaveBeenCalledWith('link-a');
    expect(announce).toHaveBeenCalledWith('criteria.share.revoked');
    const left = within(dialog).getAllByRole('button', { name: 'Link ungültig machen' });
    expect(left).toHaveLength(1);
    expect(document.activeElement).toBe(left[0]);

    left[0].click();
    await settle(fixture);
    expect(document.activeElement).toBe(within(dialog).getByRole('heading', { level: 3, name: 'Gültige Links' }));
    expect(dialog.textContent).toContain('Für diese Liste gibt es keinen gültigen Link.');
  });

  it('drops the shown link when it is revoked', async () => {
    const list = vi
      .fn()
      .mockResolvedValueOnce(linksAnswer())
      .mockResolvedValueOnce(linksAnswer([{ ...LINK_A, id: 'link-new' }]));
    const { fixture, dialog } = await openDialog({ list });
    within(dialog).getByRole('button', { name: 'Link erstellen' }).click();
    await settle(fixture);

    within(dialog).getByRole('button', { name: 'Link ungültig machen' }).click();
    await settle(fixture);

    expect(within(dialog).queryByRole('textbox', { name: 'Freigabelink' })).toBeNull();
  });

  it('INV:SEC-10 shows the confirmation notice for an unconfirmed address and keeps the date', async () => {
    const { fixture, dialog } = await openDialog({
      create: vi.fn().mockRejectedValue(problem(403, { type: `${base}email-unconfirmed`, status: 403 })),
    });

    within(dialog).getByRole('button', { name: 'Link erstellen' }).click();
    await settle(fixture);

    expect(dialog.textContent).toContain('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse.');
    expect(document.activeElement?.textContent).toContain('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse.');
    expect(dateField(dialog).value).toBe('2027-02-14');
  });

  it('shows the server\'s refusal of the date at the field (TEN-12)', async () => {
    const { fixture, dialog } = await openDialog({
      create: vi.fn().mockRejectedValue(
        problem(400, { type: `${base}validation`, status: 400, errors: { expiresOn: ['The expiry date lies between …'] } }),
      ),
    });

    within(dialog).getByRole('button', { name: 'Link erstellen' }).click();
    await settle(fixture);

    expect(document.activeElement?.textContent).toContain('Bitte wählen Sie ein Datum zwischen morgen und 16.11.2027.');
    expect(dialog.textContent).not.toContain('The expiry date');
  });

  it('shows the error notice when a revocation fails, and keeps the link', async () => {
    const { fixture, dialog } = await openDialog({
      list: vi.fn().mockResolvedValue(linksAnswer([LINK_A])),
      revoke: vi.fn().mockRejectedValue(problem(500, { status: 500 })),
    });

    within(dialog).getByRole('button', { name: 'Link ungültig machen' }).click();
    await settle(fixture);

    const alert = within(dialog).getByRole('alert');
    expect(alert.textContent).toContain('Aktion fehlgeschlagen');
    expect(alert.textContent).toContain('Das hat nicht geklappt.');
    expect(within(dialog).getAllByRole('button', { name: 'Link ungültig machen' })).toHaveLength(1);
  });

  it('offers „Erneut versuchen“ when the links cannot be loaded', async () => {
    const list = vi.fn().mockRejectedValueOnce(problem(500, { status: 500 })).mockResolvedValueOnce(linksAnswer());
    const { fixture, dialog } = await openDialog({ list });

    expect(within(dialog).getByRole('alert').textContent).toContain('Aktion fehlgeschlagen');
    expect(within(dialog).queryByLabelText(/^Gültig bis/)).toBeNull();

    within(dialog).getByRole('button', { name: 'Erneut versuchen' }).click();
    await settle(fixture);

    expect(dateField(dialog).value).toBe('2027-02-14');
  });

  it('closes with „Schließen“ and returns the focus to the page button', async () => {
    const { fixture, dialog, opener } = await openDialog();

    within(dialog).getByRole('button', { name: 'Schließen' }).click();
    await settle(fixture);

    expect(screen.queryByRole('dialog')).toBeNull();
    expect(document.activeElement).toBe(opener);
  });
});
