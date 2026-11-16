// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { provideRouter, Router } from '@angular/router';
import { signalStore, withMethods } from '@ngrx/signals';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AuthService } from '@app/features/identity/services/auth.service';
import { OrganizationSummary } from '@app/features/organization/models/organization';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { SaveListRequest } from '../../models/saved-lists';
import { CriteriaListPage } from '../../pages/criteria-list/criteria-list';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { withConfigurator } from '../../state/with-configurator';
import { criteriaAnswer } from '../../testing/criteria-fixture';
import { filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { provideLoadedConfiguratorSettings } from '../../testing/configurator-settings-stub';

// The dialog reads the list S11 shows, so it runs on the S11 page with the real configurator slice;
// sign-in and organisation come from the test double.
let signedIn = false;
let organization: OrganizationSummary | null = null;
const SaveTestStore = signalStore(
  withConfigurator(),
  withMethods(() => ({ signedIn: () => signedIn, activeOrganization: () => organization })),
);

@Component({ template: '' })
class Blank {}

const base = 'https://procureaccess.de/problems/';
const ORG: OrganizationSummary = { id: 'org-1', name: 'Stadt Musterhausen', type: 'publicBody', role: 'editor' };

function problem(status: number, body: object): HttpErrorResponse {
  return new HttpErrorResponse({ status, error: body });
}

describe('„Liste speichern“ on S11 and the save dialog S12 (R-SAV-01)', () => {
  let save: ReturnType<typeof vi.fn>;
  let showLogin: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;

  async function openList({
    asUser = true,
    org = ORG,
    api = vi.fn().mockResolvedValue('list-1'),
  }: { asUser?: boolean; org?: OrganizationSummary | null; api?: ReturnType<typeof vi.fn> } = {}) {
    signedIn = asUser;
    organization = org;
    save = api;
    showLogin = vi.fn().mockResolvedValue(true);
    announce = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(CriteriaListPage, {
      providers: [
        provideLoadedConfiguratorSettings(),
        provideTestTranslations(),
        provideRouter([
          { path: 'filters', component: Blank },
          { path: 'kriterien', component: Blank },
          { path: 'organisation', component: Blank },
          { path: 'kriterienlisten/:id', component: Blank },
        ]),
        { provide: ProcureAccessStore, useClass: SaveTestStore },
        {
          provide: CatalogApiService,
          useValue: {
            getFilterOptions: vi.fn().mockResolvedValue(filterOptionsAnswer()),
            getCriteria: vi.fn().mockResolvedValue(criteriaAnswer()),
          },
        },
        { provide: ConfiguratorApiService, useValue: { reportCriteriaListShown: () => Promise.resolve(null), save } },
        { provide: AuthService, useValue: { showLogin } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered.fixture;
  }

  async function openDialog(options: Parameters<typeof openList>[0] = {}) {
    const fixture = await openList(options);
    screen.getByRole('button', { name: 'Liste speichern' }).click();
    await fixture.whenStable();
    return { fixture, dialog: screen.getByRole('dialog', { name: 'Kriterienliste speichern' }) };
  }

  async function submit(fixture: { whenStable(): Promise<unknown> }, dialog: HTMLElement, name: string) {
    const field = within(dialog).getByRole('textbox', { name: 'Name der Liste' });
    fireEvent.input(field, { target: { value: name } });
    within(dialog).getByRole('button', { name: 'Liste speichern' }).click();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  it('opens S01 with the reason „save“ while nobody is signed in', async () => {
    const fixture = await openList({ asUser: false });

    screen.getByRole('button', { name: 'Liste speichern' }).click();
    await fixture.whenStable();

    expect(showLogin).toHaveBeenCalledWith('save');
  });

  it('shows „Ihre Auswahl ist noch da“ after the sign-in from „Liste speichern“ until S11 is left (S5-20b 1a)', async () => {
    const fixture = await openList();
    const store = TestBed.inject(ProcureAccessStore) as unknown as InstanceType<typeof SaveTestStore>;

    store.keepDraftAfterSignIn();
    await fixture.whenStable();
    expect(screen.getByText('Sie sind angemeldet. Ihre Auswahl ist noch da und kann jetzt gespeichert werden.')).toBeTruthy();

    fixture.destroy();
    expect(store.configurator.draftKept()).toBe(false);
  });

  it('asks for an organisation without a membership and leads to S20 (S5-20b 3a)', async () => {
    const fixture = await openList({ org: null });

    screen.getByRole('button', { name: 'Liste speichern' }).click();
    await fixture.whenStable();
    const dialog = screen.getByRole('dialog', { name: 'Kriterienliste speichern' });
    expect(dialog.textContent).toContain('Um Kriterienlisten zu speichern, brauchen Sie eine Organisation.');
    within(dialog).getByRole('button', { name: 'Organisation anlegen oder beitreten' }).click();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));

    expect(TestBed.inject(Router).url).toBe('/organisation');
    expect(save).not.toHaveBeenCalled();
  });

  it('hides „Liste speichern“ from a Reader of the organisation the header shows', async () => {
    await openList({ org: { ...ORG, role: 'reader' } });

    expect(screen.queryByRole('button', { name: 'Liste speichern' })).toBeNull();
    expect(screen.getByRole('button', { name: 'Liste exportieren' })).toBeTruthy();
  });

  it('opens S12 with the focus in the name field, its hint and the organisation note', async () => {
    const { dialog } = await openDialog();

    const field = within(dialog).getByRole('textbox', { name: 'Name der Liste' });
    expect(document.activeElement).toBe(field);
    expect(dialog.textContent).toContain('Zum Beispiel der Name Ihres Vergabeverfahrens.');
    expect(dialog.textContent).toContain('Die Liste ist für alle Mitglieder Ihrer Organisation Stadt Musterhausen sichtbar.');
    await expectNoAxeViolations();
  });

  it('refuses an empty name and a name over 120 characters before sending', async () => {
    const { fixture, dialog } = await openDialog();

    await submit(fixture, dialog, '   ');
    expect(within(dialog).getAllByText('Bitte geben Sie Name der Liste an.').length).toBeGreaterThan(0);
    await submit(fixture, dialog, 'a'.repeat(121));
    expect(within(dialog).getAllByText('Bitte kürzen Sie den Text auf höchstens 120 Zeichen. Derzeit sind es 121.').length).toBeGreaterThan(0);

    expect(save).not.toHaveBeenCalled();
  });

  it('saves the draft for the organisation, opens the saved list, drops the draft and announces it (S5-20 4a)', async () => {
    const { fixture, dialog } = await openDialog();
    const store = TestBed.inject(ProcureAccessStore) as unknown as InstanceType<typeof SaveTestStore>;

    await submit(fixture, dialog, '  Vergabe Personalsoftware 2027  ');

    const request = save.mock.calls[0][0] as SaveListRequest;
    expect(request.organizationId).toBe('org-1');
    expect(request.name).toBe('Vergabe Personalsoftware 2027');
    expect(request.selection.releaseVersion).toBe('T.1');
    expect(request.criteria.length).toBe(criteriaAnswer().criteria.length);
    expect(request).not.toHaveProperty('format');
    expect(TestBed.inject(Router).url).toBe('/kriterienlisten/list-1');
    expect(store.configurator.list.answer()).toBeNull();
    expect(announce).toHaveBeenCalledWith('criteria.save.success', { name: 'Vergabe Personalsoftware 2027' });
    expect(screen.queryByRole('dialog')).toBeNull();
  });

  it('INV:SEC-10 shows the confirmation notice for an unconfirmed address and keeps the name', async () => {
    const { fixture, dialog } = await openDialog({
      api: vi.fn().mockRejectedValue(problem(403, { type: `${base}email-unconfirmed`, status: 403 })),
    });

    await submit(fixture, dialog, 'Meine Liste');

    expect(dialog.textContent).toContain('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse.');
    expect(within(dialog).getByRole<HTMLInputElement>('textbox', { name: 'Name der Liste' }).value).toBe('Meine Liste');
    expect(document.activeElement?.textContent).toContain('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse.');
    expect(TestBed.inject(Router).url).not.toContain('kriterienlisten');
  });

  it('shows the outdated release with „Schließen“ only, which takes the focus', async () => {
    const { fixture, dialog } = await openDialog({
      api: vi.fn().mockRejectedValue(problem(400, { type: `${base}release-mismatch`, status: 400, errors: {} })),
    });

    await submit(fixture, dialog, 'Meine Liste');

    const alert = within(dialog).getByRole('alert');
    expect(alert.textContent).toContain('Liste nicht gespeichert');
    expect(alert.textContent).toContain('weil inzwischen eine neue Katalogversion gilt');
    expect(within(dialog).queryByRole('button', { name: 'Liste speichern' })).toBeNull();
    expect(document.activeElement).toBe(within(dialog).getByRole('button', { name: 'Schließen' }));
  });

  it('names the role when the server refuses the user of the organisation (403)', async () => {
    const { fixture, dialog } = await openDialog({ api: vi.fn().mockRejectedValue(problem(403, { status: 403 })) });

    await submit(fixture, dialog, 'Meine Liste');

    expect(within(dialog).getByRole('alert').textContent).toContain(
      'Sie können für Stadt Musterhausen keine Listen speichern. Dafür brauchen Sie die Rolle „Bearbeiten“ oder „Verwalten“.',
    );
  });

  it('shows a server error at the name field and any other field error as „nicht gespeichert“', async () => {
    const { fixture, dialog } = await openDialog({
      api: vi
        .fn()
        .mockRejectedValueOnce(problem(400, { type: `${base}validation`, status: 400, errors: { name: ['Zu lang.'] } }))
        .mockRejectedValueOnce(problem(400, { type: `${base}validation`, status: 400, errors: { 'criteria[0].note': ['x'] } })),
    });

    await submit(fixture, dialog, 'Meine Liste');
    expect(within(dialog).getAllByText('Zu lang.', { exact: false }).length).toBeGreaterThan(0);

    await submit(fixture, dialog, 'Meine Liste 2');
    expect(within(dialog).getByRole('alert').textContent).toContain('Die Liste konnte nicht gespeichert werden.');
  });
});
