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
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { Project } from '../../models/projects';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { WorkspaceTestStore } from '../../testing/workspace-test-store';
import { EditProjectDialog, EditProjectResult } from './edit-project-dialog';

const base = 'https://procureaccess.de/problems/';
const PROJECT: Project = {
  id: 'p-1',
  organizationId: 'org-1',
  etag: '"1-1"',
  name: 'Neues Bewerbermanagement 2027',
  status: 'open',
  productCount: 0,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: null,
  criteriaListLocked: false,
  candidates: [],
};

/** S32's button, as far as the dialog needs it: opens the dialog and keeps what it closes with. */
@Component({
  template: '<button type="button" (click)="open()">open</button>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class Host {
  readonly project = input(PROJECT);
  private readonly dialog = inject(MatDialog);
  result: EditProjectResult | undefined | 'open' = 'open';

  open(): void {
    EditProjectDialog.open(this.dialog, this.project())
      .afterClosed()
      .subscribe((result) => (this.result = result));
  }
}

function problem(status: number, body: object): HttpErrorResponse {
  return new HttpErrorResponse({ status, error: body });
}

describe('dialog „Projekt bearbeiten“ of S32 (R-PRJ-05)', () => {
  let change: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;

  async function openDialog({ project = PROJECT, api = vi.fn() }: { project?: Project; api?: ReturnType<typeof vi.fn> } = {}) {
    change = api;
    announce = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(Host, {
      inputs: { project },
      providers: [
        provideTestTranslations(),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: ProjectsApiService, useValue: { change } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: ProcureAccessStore, useClass: WorkspaceTestStore },
      ],
    });
    const fixture = rendered.fixture;
    screen.getByRole('button', { name: 'open' }).click();
    await settle(fixture);
    return { fixture, host: fixture.componentInstance, dialog: screen.getByRole('dialog', { name: 'Projekt bearbeiten' }) };
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  async function save(fixture: { whenStable(): Promise<unknown> }, dialog: HTMLElement) {
    within(dialog).getByRole('button', { name: 'Speichern' }).click();
    await settle(fixture);
  }

  function rename(dialog: HTMLElement, name: string) {
    fireEvent.input(within(dialog).getByRole('textbox', { name: 'Name des Projekts' }), { target: { value: name } });
  }

  it('opens with the name in the field, focused, and the status chosen', async () => {
    const { dialog } = await openDialog();

    const field = within(dialog).getByRole<HTMLInputElement>('textbox', { name: 'Name des Projekts' });
    expect(field.value).toBe('Neues Bewerbermanagement 2027');
    expect(document.activeElement).toBe(field);
    const status = within(dialog).getByRole('radiogroup', { name: 'Stand' });
    expect(within(status).getAllByRole('radio').map((radio) => radio.closest('mat-radio-button')?.textContent?.trim())).toEqual([
      'Laufend',
      'Abgeschlossen',
      'Archiviert',
    ]);
    expect(within(status).getByRole<HTMLInputElement>('radio', { name: 'Laufend' }).checked).toBe(true);
    await expectNoAxeViolations();
  });

  it('sends the new name and status, closes with the changed project and announces it', async () => {
    const changed = { ...PROJECT, name: 'Bewerbermanagement 2028', status: 'done' as const };
    const { fixture, host, dialog } = await openDialog({ api: vi.fn().mockResolvedValue(changed) });

    rename(dialog, '  Bewerbermanagement 2028  ');
    within(dialog).getByRole('radio', { name: 'Abgeschlossen' }).click();
    await save(fixture, dialog);

    expect(change).toHaveBeenCalledWith('p-1', { name: 'Bewerbermanagement 2028', status: 'done' }, '"1-1"');
    expect(host.result).toEqual(changed);
    expect(screen.queryByRole('dialog')).toBeNull();
    expect(announce).toHaveBeenCalledWith('projects.edit.saved');
  });

  it('sends only what changed, and nothing when nothing changed', async () => {
    const { fixture, host, dialog } = await openDialog();

    await save(fixture, dialog);

    expect(change).not.toHaveBeenCalled();
    expect(host.result).toBeUndefined();
    expect(screen.queryByRole('dialog')).toBeNull();
  });

  it('refuses an empty name and a name over 120 characters before sending', async () => {
    const { fixture, dialog } = await openDialog();

    rename(dialog, '   ');
    await save(fixture, dialog);
    expect(within(dialog).getByRole('link', { name: /Name des Projekts/ })).toBeTruthy();

    rename(dialog, 'x'.repeat(121));
    await save(fixture, dialog);
    expect(dialog.textContent).toContain('121');
    expect(change).not.toHaveBeenCalled();
  });

  it('offers only the status of an archived project, with the archived note, focused on the chosen status', async () => {
    const reopened = { ...PROJECT, status: 'open' as const };
    const { fixture, dialog } = await openDialog({
      project: { ...PROJECT, status: 'archived' },
      api: vi.fn().mockResolvedValue(reopened),
    });

    expect(within(dialog).queryByRole('textbox')).toBeNull();
    expect(dialog.textContent).toContain('Dieses Projekt ist archiviert.');
    expect(document.activeElement).toBe(within(dialog).getByRole('radio', { name: 'Archiviert' }));
    await expectNoAxeViolations();

    within(dialog).getByRole('radio', { name: 'Laufend' }).click();
    await save(fixture, dialog);
    expect(change).toHaveBeenCalledWith('p-1', { status: 'open' }, '"1-1"');
  });

  it('INV:SEC-10 shows the confirmation notice for an unconfirmed address and keeps the input', async () => {
    const { fixture, dialog } = await openDialog({
      api: vi.fn().mockRejectedValue(problem(403, { type: `${base}email-unconfirmed`, status: 403 })),
    });

    rename(dialog, 'Umbenannt');
    await save(fixture, dialog);

    expect(dialog.textContent).toContain('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse.');
    expect(document.activeElement?.textContent).toContain('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse.');
    expect(within(dialog).getByRole<HTMLInputElement>('textbox', { name: 'Name des Projekts' }).value).toBe('Umbenannt');
  });

  it('explains a project archived meanwhile (409 project-archived)', async () => {
    const { fixture, dialog } = await openDialog({
      api: vi.fn().mockRejectedValue(problem(409, { type: `${base}project-archived`, status: 409 })),
    });

    rename(dialog, 'Umbenannt');
    await save(fixture, dialog);

    const alert = within(dialog).getByRole('alert');
    expect(alert.textContent).toContain('Änderungen nicht gespeichert');
    expect(alert.textContent).toContain('Dieses Projekt ist archiviert.');
  });

  it('INV:API-02 shows a field error of the server at the name', async () => {
    const { fixture, dialog } = await openDialog({
      api: vi.fn().mockRejectedValue(
        problem(400, { type: `${base}validation`, status: 400, errors: { name: ['Der Name ist zu lang.'] } }),
      ),
    });

    rename(dialog, 'Umbenannt');
    await save(fixture, dialog);

    expect(within(dialog).getByRole('link', { name: /Der Name ist zu lang\./ })).toBeTruthy();
    expect(within(dialog).queryByRole('alert', { name: /Änderungen nicht gespeichert/ })).toBeNull();
  });

  describe('409 conflict (R-CAN-11, S37)', () => {
    const conflict = () =>
      problem(409, { type: `${base}conflict`, status: 409, editorDisplayName: 'a.schmidt@example.test' });

    it('INV:API-03 opens S37 over the dialog with the other editor; after Escape „Speichern“ sends nothing and opens S37 again', async () => {
      const { fixture, host, dialog } = await openDialog({ api: vi.fn().mockRejectedValue(conflict()) });

      rename(dialog, 'Umbenannt');
      await save(fixture, dialog);

      const s37 = screen.getByRole('alertdialog', { name: 'Inzwischen geändert' });
      expect(s37.textContent).toContain('a.schmidt@example.test hat diesen Inhalt geändert');
      expect(document.activeElement).toBe(within(s37).getByRole('button', { name: 'Meine Änderungen kopieren' }));
      expect(within(dialog).queryByRole('alert')).toBeNull();

      fireEvent.keyDown(s37, { key: 'Escape', code: 'Escape', keyCode: 27 });
      await settle(fixture);
      expect(screen.queryByRole('alertdialog')).toBeNull();
      expect(screen.getByRole('dialog', { name: 'Projekt bearbeiten' })).toBeTruthy();
      expect(within(dialog).getByRole<HTMLInputElement>('textbox', { name: 'Name des Projekts' }).value).toBe('Umbenannt');

      await save(fixture, dialog);
      expect(change).toHaveBeenCalledTimes(1);
      expect(screen.getByRole('alertdialog', { name: 'Inzwischen geändert' })).toBeTruthy();
      expect(host.result).toBe('open');
    });

    it('copies the changed name and status as text', async () => {
      const writeText = vi.fn().mockResolvedValue(undefined);
      Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
      try {
        const { fixture, dialog } = await openDialog({ api: vi.fn().mockRejectedValue(conflict()) });
        rename(dialog, 'Umbenannt');
        within(dialog).getByRole('radio', { name: 'Abgeschlossen' }).click();
        await save(fixture, dialog);

        within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Meine Änderungen kopieren' }).click();
        await settle(fixture);

        expect(writeText).toHaveBeenCalledWith('Name des Projekts: Umbenannt\nStand: Abgeschlossen');
      } finally {
        Reflect.deleteProperty(navigator, 'clipboard');
      }
    });

    it('closes both dialogs with „reload“ for „Neue Fassung laden“', async () => {
      const { fixture, host, dialog } = await openDialog({ api: vi.fn().mockRejectedValue(conflict()) });

      rename(dialog, 'Umbenannt');
      await save(fixture, dialog);
      within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Neue Fassung laden' }).click();
      await settle(fixture);

      expect(screen.queryByRole('alertdialog')).toBeNull();
      expect(screen.queryByRole('dialog')).toBeNull();
      expect(host.result).toBe('reload');
    });
  });

  it('shows the error notice for any other failure and keeps the dialog open', async () => {
    const { fixture, dialog } = await openDialog({ api: vi.fn().mockRejectedValue(problem(500, { status: 500 })) });

    rename(dialog, 'Umbenannt');
    await save(fixture, dialog);

    const alert = within(dialog).getByRole('alert');
    expect(alert.textContent).toContain('Änderungen nicht gespeichert');
    expect(alert.textContent).toContain('Das Projekt konnte nicht geändert werden.');
    expect(screen.getByRole('dialog', { name: 'Projekt bearbeiten' })).toBeTruthy();
    expect(announce).not.toHaveBeenCalled();
  });
});
