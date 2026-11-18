// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { provideRouter } from '@angular/router';
import { signalStore, withMethods } from '@ngrx/signals';
import { withLoading } from '@app/shared/state/with-loading';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { FileSaver } from '@app/core/files/file-saver';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AuthService } from '@app/features/identity/services/auth.service';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { NOTE_MAX_LENGTH } from '../../models/criteria-list';
import { ExportRequest } from '../../models/export';
import { CriteriaListPage } from '../../pages/criteria-list/criteria-list';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { ExportApiService } from '../../services/api/export-api.service';
import { withConfigurator } from '../../state/with-configurator';
import { ExportDialog } from './export-dialog';
import { criteriaAnswer } from '../../testing/criteria-fixture';
import { filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { provideLoadedConfiguratorSettings } from '../../testing/configurator-settings-stub';

// The dialog reads the list S11 shows, so it runs on the S11 page with the real configurator slice.
let signedIn = false;
const ConfiguratorTestStore = signalStore(
  withLoading(),
  withConfigurator(),
  withMethods(() => ({ signedIn: () => signedIn, activeOrganization: () => null })),
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
  let getCriteria: ReturnType<typeof vi.fn>;
  let getFilterOptions: ReturnType<typeof vi.fn>;
  let focusPageHeading: ReturnType<typeof vi.fn>;

  const downloaded = { content: new Blob(['x']), fileName: 'liste.xlsx' };

  async function open({
    signedIn: asUser = false,
    api = vi.fn().mockResolvedValue(downloaded),
  }: { signedIn?: boolean; api?: ReturnType<typeof vi.fn> } = {}) {
    signedIn = asUser;
    exportList = api;
    save = vi.fn();
    announce = vi.fn().mockResolvedValue(undefined);
    getCriteria = vi.fn().mockResolvedValue(criteriaAnswer());
    getFilterOptions = vi.fn().mockResolvedValue(filterOptionsAnswer());
    focusPageHeading = vi.fn();
    const rendered = await render(CriteriaListPage, {
      providers: [
        provideLoadedConfiguratorSettings(),
        provideTestTranslations(),
        provideRouter([
          { path: 'filters', component: Blank },
          { path: 'kriterien', component: Blank },
        ]),
        { provide: ProcureAccessStore, useClass: ConfiguratorTestStore },
        {
          provide: CatalogApiService,
          useValue: {
            getFilterOptions,
            getCriteria,
          },
        },
        { provide: ConfiguratorApiService, useValue: { reportCriteriaListShown: () => Promise.resolve(null) } },
        { provide: ExportApiService, useValue: { exportList } },
        { provide: FileSaver, useValue: { save } },
        { provide: AuthService, useValue: { showLogin: vi.fn() } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: PageTitleStrategy, useValue: { focusPageHeading } },
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

  it('explains a release that is no longer active and offers „Schließen“ and „Kriterien neu laden“, which takes the focus', async () => {
    const { fixture, dialog } = await open({
      api: vi.fn().mockRejectedValue(problem(400, { type: `${base}release-mismatch`, status: 400 })),
    });

    within(dialog).getByRole('button', { name: 'Datei herunterladen' }).click();
    await fixture.whenStable();

    expect(within(dialog).getByRole('alert').textContent).toContain('weil inzwischen eine neue Katalogversion gilt');
    const buttons = within(dialog).getAllByRole('button');
    expect(buttons.map((b) => b.textContent?.trim())).toEqual(['Schließen', 'Kriterien neu laden']);
    expect(document.activeElement).toBe(buttons[1]);
    await expectNoAxeViolations();

    buttons[0].click();
    await fixture.whenStable();
    expect(screen.queryByRole('dialog')).toBeNull();
    // „Schließen“ leaves the list as it was.
    expect(getCriteria).toHaveBeenCalledTimes(1);
  });

  it('„Kriterien neu laden“ loads the list for the active release, drops the changes to single criteria and focuses the H1 (TD-39, #310)', async () => {
    const { fixture, dialog } = await open({
      api: vi.fn().mockRejectedValue(problem(400, { type: `${base}release-mismatch`, status: 400 })),
    });
    const store = fixture.debugElement.injector.get(ProcureAccessStore) as unknown as InstanceType<typeof ConfiguratorTestStore>;
    store.setCriterionIncluded('11.3.3', false);
    const selection = store.configurator.list.selection();
    expect(store.configurator.release()).toBe('T.1');
    getFilterOptions.mockResolvedValue(filterOptionsAnswer(undefined, 'T.2'));
    getCriteria.mockResolvedValue(criteriaAnswer('T.2'));

    within(dialog).getByRole('button', { name: 'Datei herunterladen' }).click();
    await fixture.whenStable();
    within(dialog).getByRole('button', { name: 'Kriterien neu laden' }).click();
    await vi.waitFor(() => expect(store.configurator.list.answer()?.release).toBe('T.2'));
    await fixture.whenStable();

    expect(screen.queryByRole('dialog')).toBeNull();
    // The options ask without a release, so the server answers with the active one, which is pinned.
    expect(getFilterOptions).toHaveBeenLastCalledWith(expect.anything(), undefined);
    expect(getCriteria).toHaveBeenLastCalledWith(expect.objectContaining({ appTypeIds: selection?.appTypeIds }), 'T.2', ['not-found']);
    expect(store.configurator.list.selection()?.useCase).toBe(selection?.useCase);
    expect(store.configurator.list.decisions()).toEqual({});
    expect(screen.getByRole('button', { name: 'Liste exportieren' })).toBeTruthy();
    // The button that opened S13 was gone while the list loaded: the focus goes to the H1.
    expect(focusPageHeading).toHaveBeenCalledTimes(1);
  });
});

describe('ExportDialog with a note that is too long (TD-40, S5-22)', () => {
  const keyboardCard = () =>
    screen.getByRole('heading', { level: 2, name: 'Alles geht mit der Tastatur' }).closest('pa-criterion-card') as HTMLElement;
  const noteOf = (card: HTMLElement) =>
    within(card).getByRole<HTMLTextAreaElement>('textbox', { name: 'Ihre Notiz zu diesem Kriterium (optional)' });

  /** Types a note one character too long into the card „Alles geht mit der Tastatur“; `exclude` removes the card from the list. */
  async function writeTooLongNote(fixture: { whenStable(): Promise<unknown> }, exclude: boolean): Promise<void> {
    within(keyboardCard()).getByRole('button', { name: 'Notiz hinzufügen' }).click();
    await fixture.whenStable();
    const note = noteOf(keyboardCard());
    note.value = 'x'.repeat(NOTE_MAX_LENGTH + 1);
    fireEvent.input(note);
    await fixture.whenStable();
    if (exclude) {
      within(keyboardCard()).getByRole('checkbox', { name: /^In Liste aufnehmen/ }).click();
      await fixture.whenStable();
    }
  }

  async function openWithTooLongNote(exclude = false) {
    signedIn = false;
    const exportList = vi.fn();
    const announce = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(CriteriaListPage, {
      providers: [
        provideLoadedConfiguratorSettings(),
        provideTestTranslations(),
        provideRouter([{ path: 'kriterien', component: Blank }]),
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
        { provide: FileSaver, useValue: { save: vi.fn() } },
        { provide: AuthService, useValue: { showLogin: vi.fn() } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
      ],
    });
    const { fixture } = rendered;
    await fixture.whenStable();
    await writeTooLongNote(fixture, exclude);
    const store = fixture.debugElement.injector.get<InstanceType<typeof ConfiguratorTestStore>>(ProcureAccessStore);
    if (exclude) {
      store.showOnlyIncludedCriteria(true);
      await fixture.whenStable();
    }
    screen.getByRole('button', { name: 'Liste exportieren' }).click();
    await fixture.whenStable();
    const dialog = screen.getByRole('dialog', { name: 'Kriterienliste exportieren' });
    const download = within(dialog).getByRole('button', { name: 'Datei herunterladen' });
    download.focus();
    download.click();
    await fixture.whenStable();
    return { fixture, dialog, download, exportList, store };
  }

  it('names the criterion instead of requesting the file, and „Zur Notiz“ replaces the download in place', async () => {
    const { dialog, download, exportList } = await openWithTooLongNote();

    expect(exportList).not.toHaveBeenCalled();
    const alert = within(dialog).getByRole('alert');
    expect(alert.textContent).toContain('Export fehlgeschlagen');
    expect(alert.textContent).toContain(
      'Die Notiz zum Kriterium „Alles geht mit der Tastatur“ ist länger als 1000 Zeichen. Bitte kürzen Sie sie, dann können Sie die Datei herunterladen.',
    );
    expect(within(dialog).getAllByRole('button').map((b) => b.textContent?.trim())).toEqual(['Abbrechen', 'Zur Notiz']);
    // The same button changed its label, so the focus stays on it.
    expect(download.textContent?.trim()).toBe('Zur Notiz');
    expect(document.activeElement).toBe(download);
    await expectNoAxeViolations();
  });

  it('„Zur Notiz“ closes the dialog, opens the note with its error and moves the focus into it', async () => {
    const { fixture, download } = await openWithTooLongNote();

    download.click();
    await fixture.whenStable();
    await vi.waitFor(() => expect(document.activeElement).toBe(noteOf(keyboardCard())));

    expect(screen.queryByRole('dialog')).toBeNull();
    expect(within(keyboardCard()).getByText('Bitte kürzen Sie den Text auf höchstens 1000 Zeichen. Derzeit sind es 1001.')).toBeTruthy();
    expect(noteOf(keyboardCard()).getAttribute('aria-invalid')).toBe('true');
  });

  it('clears a filter that hides the card first: an excluded card counts too', async () => {
    const { fixture, download, store } = await openWithTooLongNote(true);
    expect(screen.queryByRole('heading', { level: 2, name: 'Alles geht mit der Tastatur' })).toBeNull();

    download.click();
    await fixture.whenStable();
    await vi.waitFor(() => expect(document.activeElement).toBe(noteOf(keyboardCard())));

    expect(store.configurator.list.filter().onlyIncluded).toBe(false);
  });
});

describe('ExportDialog for a saved list (R-LST-12, S5-21b)', () => {
  it('exports the stored list under its own release, with its count and without the anonymous note', async () => {
    const exportSavedList = vi.fn().mockResolvedValue({ content: new Blob(['x']), fileName: 'ProcureAccess_Kriterien_Liste_T.1_2026-11-16.csv' });
    const save = vi.fn();
    const announce = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(Blank, {
      providers: [
        provideLoadedConfiguratorSettings(),
        provideTestTranslations(),
        { provide: ProcureAccessStore, useClass: ConfiguratorTestStore },
        { provide: ExportApiService, useValue: { exportSavedList } },
        { provide: CatalogApiService, useValue: {} },
        { provide: ConfiguratorApiService, useValue: {} },
        { provide: FileSaver, useValue: { save } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
      ],
    });
    ExportDialog.open(rendered.debugElement.injector.get(MatDialog), { listId: 'list-1', release: 'T.1', included: 7, tooLongNote: null });
    await rendered.fixture.whenStable();
    const dialog = screen.getByRole('dialog', { name: 'Kriterienliste exportieren' });

    expect(dialog.textContent).toContain('Exportiert werden 7 Kriterien aus Ihrer Liste');
    expect(dialog.textContent).toContain('Die Datei nennt die Katalogversion T.1.');
    await expectNoAxeViolations();
    within(dialog).getByRole('radio', { name: CSV }).click();
    await rendered.fixture.whenStable();
    within(dialog).getByRole('button', { name: 'Datei herunterladen' }).click();
    await vi.waitFor(() => expect(save).toHaveBeenCalled());

    expect(exportSavedList).toHaveBeenCalledWith('list-1', 'csv');
    expect(save).toHaveBeenCalledWith(expect.any(Blob), 'ProcureAccess_Kriterien_Liste_T.1_2026-11-16.csv');
    expect(announce).toHaveBeenCalledWith('criteria.export.success');
  });
});
