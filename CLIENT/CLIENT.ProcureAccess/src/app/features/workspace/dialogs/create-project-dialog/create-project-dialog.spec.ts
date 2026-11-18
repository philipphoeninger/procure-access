// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { Router } from '@angular/router';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { SavedListSummary } from '@app/features/configurator/models/saved-lists';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';
import { OrganizationSummary } from '@app/features/organization/models/organization';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { provideLayout } from '@app/testing/shell-test-doubles';

import { Project } from '../../models/projects';
import { ProjectsPage } from '../../pages/projects/projects-page';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { testActiveOrganization, WorkspaceTestStore } from '../../testing/workspace-test-store';

@Component({ template: '' })
class Blank {}

const base = 'https://procureaccess.de/problems/';
const ORG: OrganizationSummary = { id: 'org-1', name: 'Stadt Musterhausen', type: 'publicBody', role: 'editor' };
const CREATED: Project = {
  id: 'p-2',
  organizationId: 'org-1',
  name: 'Neues Bewerbermanagement 2027',
  status: 'open',
  productCount: 0,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: null,
};

function savedList(id: string, name: string): SavedListSummary {
  return { id, name, releaseVersion: 'T.1', createdAt: '2026-11-10T09:00:00Z', modifiedAt: '2026-11-10T09:00:00Z' };
}

function problem(status: number, body: object): HttpErrorResponse {
  return new HttpErrorResponse({ status, error: body });
}

describe('„Projekt anlegen“ on S30 and the create dialog S31 (R-PRJ-02)', () => {
  let create: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let assignCriteriaList: ReturnType<typeof vi.fn>;
  let listSavedLists: ReturnType<typeof vi.fn>;
  let showError: ReturnType<typeof vi.fn>;

  async function openDialog({
    api = vi.fn().mockResolvedValue(CREATED),
    lists = [],
    assign = vi.fn().mockResolvedValue({ id: 'copy-1', name: 'Alpha', releaseVersion: 'T.1' }),
  }: { api?: ReturnType<typeof vi.fn>; lists?: SavedListSummary[] | 'failed'; assign?: ReturnType<typeof vi.fn> } = {}) {
    create = api;
    assignCriteriaList = assign;
    listSavedLists = lists === 'failed' ? vi.fn().mockRejectedValue(problem(500, { status: 500 })) : vi.fn().mockResolvedValue(lists);
    showError = vi.fn();
    announce = vi.fn().mockResolvedValue(undefined);
    testActiveOrganization.set(ORG);
    const rendered = await render(ProjectsPage, {
      routes: [{ path: 'projekte/:id', component: Blank }],
      providers: [
        provideTestTranslations(),
        provideLayout(false),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: ProjectsApiService, useValue: { list: vi.fn().mockResolvedValue([]), create, assignCriteriaList } },
        { provide: ConfiguratorApiService, useValue: { list: listSavedLists } },
        { provide: SnackbarService, useValue: { showError } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: ProcureAccessStore, useClass: WorkspaceTestStore },
      ],
    });
    const fixture = rendered.fixture;
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
    screen.getByRole('button', { name: 'Projekt anlegen' }).click();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
    return { fixture, dialog: screen.getByRole('dialog', { name: 'Beschaffungsprojekt anlegen' }) };
  }

  async function submit(fixture: { whenStable(): Promise<unknown> }, dialog: HTMLElement, name: string) {
    const field = within(dialog).getByRole('textbox', { name: 'Name des Projekts' });
    fireEvent.input(field, { target: { value: name } });
    within(dialog).getByRole('button', { name: 'Projekt anlegen' }).click();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  it('opens S31 with the focus in the name field and its hint', async () => {
    const { dialog } = await openDialog();

    const field = within(dialog).getByRole('textbox', { name: 'Name des Projekts' });
    expect(document.activeElement).toBe(field);
    expect(dialog.textContent).toContain('Zum Beispiel „Neues Bewerbermanagement 2027“.');
    expect(within(dialog).getByRole('button', { name: 'Abbrechen' })).toBeTruthy();
    // Without saved lists there is nothing to choose: the list is assigned later.
    expect(within(dialog).queryByRole('combobox', { name: 'Kriterienliste (optional)' })).toBeNull();
    await expectNoAxeViolations();
  });

  it('refuses an empty name and a name over 120 characters before sending', async () => {
    const { fixture, dialog } = await openDialog();

    await submit(fixture, dialog, '   ');
    expect(within(dialog).getAllByText('Bitte geben Sie Name des Projekts an.').length).toBeGreaterThan(0);
    await submit(fixture, dialog, 'a'.repeat(121));
    expect(within(dialog).getAllByText('Bitte kürzen Sie den Text auf höchstens 120 Zeichen. Derzeit sind es 121.').length).toBeGreaterThan(0);

    expect(create).not.toHaveBeenCalled();
  });

  it('creates the project for the organisation, opens S32 and announces it', async () => {
    const { fixture, dialog } = await openDialog();
    const store = TestBed.inject(ProcureAccessStore) as unknown as InstanceType<typeof WorkspaceTestStore>;

    await submit(fixture, dialog, '  Neues Bewerbermanagement 2027  ');

    expect(create).toHaveBeenCalledWith('org-1', 'Neues Bewerbermanagement 2027');
    expect(TestBed.inject(Router).url).toBe('/projekte/p-2');
    expect(announce).toHaveBeenCalledWith('projects.created', { name: 'Neues Bewerbermanagement 2027' });
    expect(screen.queryByRole('dialog')).toBeNull();
    expect(store.workspace.projects().items.map((project) => project.id)).toEqual(['p-2']);
  });

  it('INV:SEC-10 shows the confirmation notice for an unconfirmed address and keeps the name', async () => {
    const { fixture, dialog } = await openDialog({
      api: vi.fn().mockRejectedValue(problem(403, { type: `${base}email-unconfirmed`, status: 403 })),
    });

    await submit(fixture, dialog, 'Mein Projekt');

    expect(dialog.textContent).toContain('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse.');
    expect(within(dialog).getByRole<HTMLInputElement>('textbox', { name: 'Name des Projekts' }).value).toBe('Mein Projekt');
    expect(document.activeElement?.textContent).toContain('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse.');
    expect(TestBed.inject(Router).url).not.toContain('projekte');
  });

  it('names the role when the server refuses the user of the organisation (403)', async () => {
    const { fixture, dialog } = await openDialog({ api: vi.fn().mockRejectedValue(problem(403, { status: 403 })) });

    await submit(fixture, dialog, 'Mein Projekt');

    const alert = within(dialog).getByRole('alert');
    expect(alert.textContent).toContain('Projekt nicht angelegt');
    expect(alert.textContent).toContain(
      'Sie können für Stadt Musterhausen keine Projekte anlegen. Dafür brauchen Sie die Rolle „Bearbeiten“ oder „Verwalten“.',
    );
  });

  it('INV:API-02 shows a server error at the name field and any other failure as „nicht angelegt“', async () => {
    const { fixture, dialog } = await openDialog({
      api: vi
        .fn()
        .mockRejectedValueOnce(problem(400, { type: `${base}validation`, status: 400, errors: { name: ['Zu lang.'] } }))
        .mockRejectedValueOnce(problem(500, { status: 500 })),
    });

    await submit(fixture, dialog, 'Mein Projekt');
    expect(within(dialog).getAllByText('Zu lang.', { exact: false }).length).toBeGreaterThan(0);

    await submit(fixture, dialog, 'Mein Projekt 2');
    expect(within(dialog).getByRole('alert').textContent).toContain('Das Projekt konnte nicht angelegt werden.');
  });

  describe('„Kriterienliste (optional)“ (R-PRJ-02, S6-12)', () => {
    const LISTS = [savedList('list-b', 'Beta'), savedList('list-a', 'Alpha')];

    async function choose(fixture: { whenStable(): Promise<unknown> }, dialog: HTMLElement, name: string) {
      within(dialog).getByRole('combobox', { name: 'Kriterienliste (optional)' }).click();
      await fixture.whenStable();
      screen.getByRole('option', { name }).click();
      await fixture.whenStable();
    }

    it('offers „Keine Liste“, preselected, and the saved lists of the organisation by name, with the hint', async () => {
      const { fixture, dialog } = await openDialog({ lists: LISTS });

      expect(listSavedLists).toHaveBeenCalledWith('org-1');
      const select = within(dialog).getByRole('combobox', { name: 'Kriterienliste (optional)' });
      expect(select.textContent).toContain('Keine Liste');
      expect(dialog.textContent).toContain('Das Projekt erhält eine eigene Kopie der Liste');
      await expectNoAxeViolations();

      select.click();
      await fixture.whenStable();
      expect(screen.getAllByRole('option').map((option) => option.textContent.trim())).toEqual(['Keine Liste', 'Alpha', 'Beta']);
    });

    it('gives the new project its own copy of the chosen list, then opens S32 (WSP-02, tagged with S6-18)', async () => {
      const { fixture, dialog } = await openDialog({ lists: LISTS });

      await choose(fixture, dialog, 'Beta');
      await submit(fixture, dialog, 'Mit Liste');

      expect(create).toHaveBeenCalledWith('org-1', 'Mit Liste');
      expect(assignCriteriaList).toHaveBeenCalledWith('p-2', 'list-b');
      expect(TestBed.inject(Router).url).toBe('/projekte/p-2');
      expect(announce).toHaveBeenCalledWith('projects.created', { name: 'Mit Liste' });
      expect(showError).not.toHaveBeenCalled();
    });

    it('creates the project without a copy for „Keine Liste“', async () => {
      const { fixture, dialog } = await openDialog({ lists: LISTS });

      await choose(fixture, dialog, 'Alpha');
      await choose(fixture, dialog, 'Keine Liste');
      await submit(fixture, dialog, 'Ohne Liste');

      expect(create).toHaveBeenCalledWith('org-1', 'Ohne Liste');
      expect(assignCriteriaList).not.toHaveBeenCalled();
    });

    it('opens S32 of the created project with the general notice when the list was not copied (owner decision S6-12 D5 (b))', async () => {
      const { fixture, dialog } = await openDialog({ lists: LISTS, assign: vi.fn().mockRejectedValue(problem(500, { status: 500 })) });

      await choose(fixture, dialog, 'Alpha');
      await submit(fixture, dialog, 'Liste fehlt');

      expect(TestBed.inject(Router).url).toBe('/projekte/p-2');
      expect(screen.queryByRole('dialog')).toBeNull();
      expect(announce).toHaveBeenCalledWith('projects.created', { name: 'Liste fehlt' });
      expect(showError).toHaveBeenCalledWith(
        'Ein Fehler ist aufgetreten. Bitte versuchen Sie es zu einem späteren Zeitpunkt erneut oder kontaktieren Sie den Support.',
      );
    });

    it('leaves the field out when the saved lists cannot be read: the list is assigned later', async () => {
      const { fixture, dialog } = await openDialog({ lists: 'failed' });

      expect(within(dialog).queryByRole('combobox', { name: 'Kriterienliste (optional)' })).toBeNull();
      await submit(fixture, dialog, 'Trotzdem');
      expect(create).toHaveBeenCalledWith('org-1', 'Trotzdem');
      expect(assignCriteriaList).not.toHaveBeenCalled();
    });
  });
});
