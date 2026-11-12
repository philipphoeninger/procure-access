// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { provideRouter } from '@angular/router';
import { signalStore, withMethods, withState } from '@ngrx/signals';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { FileSaver } from '@app/core/files/file-saver';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { ExportRequest } from '../../models/export';
import { CriteriaListPage } from '../../pages/criteria-list/criteria-list';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { ExportApiService } from '../../services/api/export-api.service';
import { withConfigurator } from '../../state/with-configurator';
import { criteriaAnswer } from '../../testing/criteria-fixture';
import { filterOptionsAnswer } from '../../testing/filter-options-fixture';

// The dialog reads the list S11 shows, so it runs on the S11 page with the real configurator slice.
let signedIn = false;
const ConfiguratorTestStore = signalStore(
  withConfigurator(),
  withState<{ user: { id: string } | null }>({ user: null }),
  withMethods(() => ({ isAuthenticated: () => signedIn })),
);

@Component({ template: '' })
class Blank {}

const XLSX = 'Excel-Datei (.xlsx) – für Vergabeunterlagen';
const CSV = 'CSV-Datei – für andere Programme';
const base = 'https://procureaccess.de/problems/';

function problem(status: number, body: object): HttpErrorResponse {
  return new HttpErrorResponse({ status, error: body });
}

describe('ExportDialog (S13, R-LST-11)', () => {
  let exportList: ReturnType<typeof vi.fn>;
  let save: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;

  const downloaded = { content: new Blob(['x']), fileName: 'liste.xlsx' };

  async function open({
    signedIn: asUser = false,
    api = vi.fn().mockResolvedValue(downloaded),
  }: { signedIn?: boolean; api?: ReturnType<typeof vi.fn> } = {}) {
    signedIn = asUser;
    exportList = api;
    save = vi.fn();
    announce = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(CriteriaListPage, {
      providers: [
        provideTestTranslations(),
        provideRouter([
          { path: 'filters', component: Blank },
          { path: 'kriterien', component: Blank },
        ]),
        { provide: ProcureAccessStore, useClass: ConfiguratorTestStore },
        {
          provide: CatalogApiService,
          useValue: {
            getFilterOptions: vi.fn().mockResolvedValue(filterOptionsAnswer()),
            getCriteria: vi.fn().mockResolvedValue(criteriaAnswer()),
          },
        },
        { provide: ConfiguratorApiService, useValue: { reportCriteriaListShown: () => Promise.resolve(null) } },
        { provide: ExportApiService, useValue: { exportList } },
        { provide: FileSaver, useValue: { save } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
      ],
    });
    await rendered.fixture.whenStable();
    screen.getByRole('button', { name: 'Liste exportieren' }).click();
    await rendered.fixture.whenStable();
    return { fixture: rendered.fixture, dialog: screen.getByRole('dialog', { name: 'Kriterienliste exportieren' }) };
  }

  it('focuses the preselected XLSX format first and shows scope, release, anonymous and legal note', async () => {
    const { dialog } = await open();

    const xlsx = within(dialog).getByRole('radio', { name: XLSX });
    expect((xlsx as HTMLInputElement).checked).toBe(true);
    expect(document.activeElement).toBe(xlsx);
    expect(within(dialog).getByRole('group', { name: 'Dateiformat' })).toBeTruthy();
    // Every card of the fixture is in the list: four, „Geprüft nach …“ included (as the action bar counts).
    expect(dialog.textContent).toContain('Exportiert werden 4 Kriterien aus Ihrer Liste');
    expect(dialog.textContent).toContain('Die Datei nennt die Katalogversion T.1.');
    expect(dialog.textContent).toContain('ProcureAccess speichert davon nichts.');
    expect(dialog.textContent).toContain('Kriterien in Vergabeunterlagen können rechtlich verbindlich werden.');
    await expectNoAxeViolations();
  });

  it('leaves the anonymous note out for a signed-in user', async () => {
    const { dialog } = await open({ signedIn: true });

    expect(dialog.textContent).not.toContain('ProcureAccess speichert davon nichts.');
  });

  it('downloads the chosen format, announces it and closes', async () => {
    const { fixture, dialog } = await open({
      api: vi.fn().mockResolvedValue({ content: new Blob(['a;b']), fileName: 'ProcureAccess_Kriterien_T.1_2026-11-12.csv' }),
    });

    within(dialog).getByRole('radio', { name: CSV }).click();
    await fixture.whenStable();
    within(dialog).getByRole('button', { name: 'Datei herunterladen' }).click();
    await fixture.whenStable();

    const request = exportList.mock.calls[0][0] as ExportRequest;
    expect(request.format).toBe('csv');
    expect(request.selection.releaseVersion).toBe('T.1');
    expect(request.criteria).toHaveLength(4);
    expect(save).toHaveBeenCalledWith(expect.any(Blob), 'ProcureAccess_Kriterien_T.1_2026-11-12.csv');
    expect(announce).toHaveBeenCalledWith('criteria.export.success');
    expect(screen.queryByRole('dialog')).toBeNull();
  });

  it('keeps the label and marks the button aria-disabled while the file is requested; a second click does nothing', async () => {
    let finish: (value: unknown) => void = () => undefined;
    const { fixture, dialog } = await open({ api: vi.fn(() => new Promise((resolve) => (finish = resolve))) });
    const button = within(dialog).getByRole('button', { name: 'Datei herunterladen' });

    button.click();
    await fixture.whenStable();
    button.click();
    await fixture.whenStable();

    expect(button.getAttribute('aria-disabled')).toBe('true');
    expect(button.textContent?.trim()).toBe('Datei herunterladen');
    expect(exportList).toHaveBeenCalledTimes(1);
    finish(downloaded);
    await vi.waitFor(() => expect(screen.queryByRole('dialog')).toBeNull());
  });

  it('shows a failure above the actions as an alert and retries with „Erneut versuchen“', async () => {
    const { fixture, dialog } = await open({
      api: vi.fn().mockRejectedValueOnce(problem(500, { status: 500 })).mockResolvedValue(downloaded),
    });

    within(dialog).getByRole('button', { name: 'Datei herunterladen' }).click();
    await fixture.whenStable();

    const alert = within(dialog).getByRole('alert');
    expect(alert.textContent).toContain('Export fehlgeschlagen');
    expect(alert.textContent).toContain('Die Datei konnte nicht erstellt werden. Bitte versuchen Sie es erneut.');
    expect(within(dialog).queryByRole('button', { name: 'Datei herunterladen' })).toBeNull();
    expect(within(dialog).getByRole('button', { name: 'Abbrechen' })).toBeTruthy();
    await expectNoAxeViolations();

    within(dialog).getByRole('button', { name: 'Erneut versuchen' }).click();
    await fixture.whenStable();

    expect(exportList).toHaveBeenCalledTimes(2);
    expect(screen.queryByRole('dialog')).toBeNull();
  });

  it.each([
    [1, 'Bitte warten Sie 1 Minute und versuchen Sie es dann erneut.'],
    [10, 'Bitte warten Sie 10 Minuten und versuchen Sie es dann erneut.'],
  ])('names the wait of a rate limit of %i minutes in the same notice', async (minutes, text) => {
    const { fixture, dialog } = await open({
      api: vi.fn().mockRejectedValue(problem(429, { type: `${base}rate-limit`, status: 429, retryAfterMinutes: minutes })),
    });

    within(dialog).getByRole('button', { name: 'Datei herunterladen' }).click();
    await fixture.whenStable();

    const alert = within(dialog).getByRole('alert');
    expect(alert.textContent).toContain('Export fehlgeschlagen');
    expect(alert.textContent).toContain(text);
    expect(within(dialog).getByRole('button', { name: 'Erneut versuchen' })).toBeTruthy();
  });

  it('explains a release that is no longer active and offers only „Schließen“, which takes the focus', async () => {
    const { fixture, dialog } = await open({
      api: vi.fn().mockRejectedValue(problem(400, { type: `${base}release-mismatch`, status: 400 })),
    });

    within(dialog).getByRole('button', { name: 'Datei herunterladen' }).click();
    await fixture.whenStable();

    expect(within(dialog).getByRole('alert').textContent).toContain('weil inzwischen eine neue Katalogversion gilt');
    const buttons = within(dialog).getAllByRole('button');
    expect(buttons.map((b) => b.textContent?.trim())).toEqual(['Schließen']);
    expect(document.activeElement).toBe(buttons[0]);
    await expectNoAxeViolations();

    buttons[0].click();
    await fixture.whenStable();
    expect(screen.queryByRole('dialog')).toBeNull();
  });
});
