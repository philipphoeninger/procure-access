// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { ConflictDialog, ConflictDialogData } from './conflict-dialog';
import { changesText, EditConflict } from './edit-conflict';

/** A form's „Speichern“, as far as S37 needs it: opens the dialog and keeps what it resolves to. */
@Component({
  template: '<button type="button" (click)="open()">Speichern</button>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class Host {
  readonly data = input.required<ConflictDialogData>();
  private readonly dialog = inject(MatDialog);
  result: boolean | 'open' = 'open';

  open(): void {
    void ConflictDialog.show(this.dialog, this.data()).then((reload) => (this.result = reload));
  }
}

const CHANGES = 'Name des Projekts: Bewerbermanagement 2028';

describe('S37 „Inzwischen geändert“ (R-CAN-11, Handoff §5.1)', () => {
  let announce: ReturnType<typeof vi.fn>;
  let showError: ReturnType<typeof vi.fn>;
  let writeText: ReturnType<typeof vi.fn>;

  async function openDialog(data: ConflictDialogData = { editorDisplayName: 'a.schmidt@example.test', changes: CHANGES }) {
    announce = vi.fn().mockResolvedValue(undefined);
    showError = vi.fn();
    writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
    const rendered = await render(Host, {
      inputs: { data },
      providers: [
        provideTestTranslations(),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: SnackbarService, useValue: { showError } },
      ],
    });
    const fixture = rendered.fixture;
    const opener = screen.getByRole('button', { name: 'Speichern' });
    opener.focus();
    opener.click();
    await settle(fixture);
    return { fixture, host: fixture.componentInstance, opener, dialog: screen.getByRole('alertdialog', { name: 'Inzwischen geändert' }) };
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  afterEach(() => {
    Reflect.deleteProperty(navigator, 'clipboard');
  });

  it('INV:A11Y-01 is an alert dialog naming the other editor, with the hint, focused on „Meine Änderungen kopieren“', async () => {
    const { dialog } = await openDialog();

    const body = document.getElementById(dialog.getAttribute('aria-describedby') ?? '');
    expect(body?.textContent?.trim()).toBe(
      'a.schmidt@example.test hat diesen Inhalt geändert, während Sie ihn bearbeitet haben. Ihre Änderungen sind noch nicht gespeichert.',
    );
    expect(dialog.textContent).toContain('Wenn Sie „Neue Fassung laden“ wählen, gehen Ihre Änderungen verloren.');
    // Frame order: „Neue Fassung laden“ (text), then „Meine Änderungen kopieren“ (filled, first focus)
    expect(within(dialog).getAllByRole('button').map((button) => button.textContent?.trim())).toEqual([
      'Neue Fassung laden',
      'Meine Änderungen kopieren',
    ]);
    expect(document.activeElement).toBe(within(dialog).getByRole('button', { name: 'Meine Änderungen kopieren' }));
    await expectNoAxeViolations();
  });

  it('says „Eine andere Person“ when the server names nobody (UX copy U188)', async () => {
    const { dialog } = await openDialog({ editorDisplayName: null, changes: CHANGES });

    expect(dialog.textContent).toContain('Eine andere Person hat diesen Inhalt geändert');
  });

  it('copies the unsaved input, announces it and stays open', async () => {
    const { fixture, host, dialog } = await openDialog();

    within(dialog).getByRole('button', { name: 'Meine Änderungen kopieren' }).click();
    await settle(fixture);

    expect(writeText).toHaveBeenCalledWith(CHANGES);
    expect(announce).toHaveBeenCalledWith('conflict.copied');
    expect(screen.getByRole('alertdialog')).toBeTruthy();
    expect(host.result).toBe('open');
  });

  it('shows the general error when the browser refuses the clipboard', async () => {
    const { fixture, dialog } = await openDialog();
    writeText.mockRejectedValue(new Error('denied'));

    within(dialog).getByRole('button', { name: 'Meine Änderungen kopieren' }).click();
    await settle(fixture);

    expect(showError).toHaveBeenCalledWith(expect.stringMatching(/^Ein Fehler ist aufgetreten\./));
    expect(announce).not.toHaveBeenCalled();
    expect(screen.getByRole('alertdialog')).toBeTruthy();
  });

  it('resolves to true for „Neue Fassung laden“', async () => {
    const { fixture, host, dialog } = await openDialog();

    within(dialog).getByRole('button', { name: 'Neue Fassung laden' }).click();
    await settle(fixture);

    expect(screen.queryByRole('alertdialog')).toBeNull();
    expect(host.result).toBe(true);
  });

  it('only closes on Escape and returns the focus to the opener', async () => {
    const { fixture, host, opener, dialog } = await openDialog();

    fireEvent.keyDown(dialog, { key: 'Escape', code: 'Escape', keyCode: 27 });
    await settle(fixture);

    expect(screen.queryByRole('alertdialog')).toBeNull();
    expect(host.result).toBe(false);
    expect(document.activeElement).toBe(opener);
  });
});

describe('changesText (UX copy U189)', () => {
  it('writes one „label: value“ line per field, a heading above its group, groups apart, empty groups left out', () => {
    expect(
      changesText([
        { fields: [{ label: 'Stand', value: 'Abgeschlossen' }] },
        { heading: 'Prüfnachweis 1', fields: [] },
        {
          heading: 'Prüfnachweis 2: BIK BITV-Test (Web)',
          fields: [
            { label: 'Wer hat geprüft?', value: 'Prüfstelle Nord' },
            { label: 'Datum der Prüfung', value: '03.11.2026' },
          ],
        },
      ]),
    ).toBe('Stand: Abgeschlossen\n\nPrüfnachweis 2: BIK BITV-Test (Web)\nWer hat geprüft?: Prüfstelle Nord\nDatum der Prüfung: 03.11.2026');
  });
});

describe('EditConflict', () => {
  const base = 'https://procureaccess.de/problems/';

  it('INV:API-03 remembers a 409 conflict with the editor and blocks until cleared', () => {
    const conflict = new EditConflict();
    expect(conflict.blocked).toBe(false);

    const caught = conflict.caught(
      new HttpErrorResponse({ status: 409, error: { type: `${base}conflict`, status: 409, editorDisplayName: 'a.schmidt@example.test' } }),
    );

    expect(caught).toBe(true);
    expect(conflict.blocked).toBe(true);
    expect(conflict.editorDisplayName).toBe('a.schmidt@example.test');
    conflict.clear();
    expect(conflict.blocked).toBe(false);
    expect(conflict.editorDisplayName).toBeNull();
  });

  it('knows nobody for a null name and ignores every other problem', () => {
    const conflict = new EditConflict();

    expect(conflict.caught(new HttpErrorResponse({ status: 409, error: { type: `${base}project-archived`, status: 409 } }))).toBe(false);
    expect(conflict.blocked).toBe(false);
    expect(conflict.caught(new HttpErrorResponse({ status: 409, error: { type: `${base}conflict`, status: 409, editorDisplayName: null } }))).toBe(
      true,
    );
    expect(conflict.editorDisplayName).toBeNull();
  });
});
