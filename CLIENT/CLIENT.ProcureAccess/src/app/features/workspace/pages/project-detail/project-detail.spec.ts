// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { render, screen } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { OrganizationRole } from '@app/features/organization/models/organization';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { Project } from '../../models/projects';
import { ProjectLoad, ProjectView } from './project-detail-load';
import { ProjectDetailPage } from './project-detail';

const PROJECT: Project = {
  id: 'p-1',
  organizationId: 'org-1',
  name: 'Bewerbermanagement',
  status: 'open',
  productCount: 0,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: null,
};

const WITH_LIST: Project = { ...PROJECT, criteriaList: { id: 'copy-1', name: 'HR-Software 2027', releaseVersion: 'T.1' } };

describe('ProjectDetailPage (S32 placeholder of S6-13)', () => {
  let setPageTitleState: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let takeCriteriaAdded: ReturnType<typeof vi.fn>;

  async function openPage(view: ProjectView, { role = 'editor', added = 0 }: { role?: OrganizationRole; added?: number } = {}) {
    setPageTitleState = vi.fn();
    announce = vi.fn().mockResolvedValue(undefined);
    takeCriteriaAdded = vi.fn().mockReturnValue(added);
    const load = new ProjectLoad();
    load.view = view;
    const rendered = await render(ProjectDetailPage, {
      routes: [{ path: 'projekte/:id/kriterienliste', children: [] }],
      providers: [
        provideTestTranslations(),
        { provide: ProjectLoad, useValue: load },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        {
          provide: ProcureAccessStore,
          useValue: {
            organizations: signal([{ id: 'org-1', name: 'Stadt Musterhausen', type: 'publicBody', role }]),
            takeCriteriaAdded,
          },
        },
      ],
    });
    await rendered.fixture.whenStable();
  }

  it('shows the name as H1, the status and the number of products, and fills the page title', async () => {
    await openPage({ kind: 'project', project: PROJECT });

    expect(screen.getByRole('heading', { level: 1, name: 'Bewerbermanagement' })).toBeTruthy();
    expect(screen.getByText('Stand: Laufend · noch keine Produkte · nur für Ihre Organisation sichtbar')).toBeTruthy();
    expect(screen.queryByText(/archiviert/)).toBeNull();
    expect(setPageTitleState).toHaveBeenCalledWith({ params: { project: 'Bewerbermanagement' } });
    await expectNoAxeViolations();
  });

  it('shows the archived note for an archived project (R-PRJ-05)', async () => {
    await openPage({ kind: 'project', project: { ...PROJECT, status: 'archived', productCount: 1 } });

    expect(screen.getByText('Stand: Archiviert · 1 Produkt · nur für Ihre Organisation sichtbar')).toBeTruthy();
    expect(screen.getByText(/^Dieses Projekt ist archiviert\./)).toBeTruthy();
  });

  describe('„Kriterienliste“ (R-PRJ-06; S6-12, owner decision D8 (a))', () => {
    it('links the copy of the project by its name and offers „Kriterienliste anpassen“ to an Editor', async () => {
      await openPage({ kind: 'project', project: WITH_LIST });

      expect(screen.getByRole('heading', { level: 2, name: 'Kriterienliste' })).toBeTruthy();
      expect(screen.getByRole('link', { name: 'HR-Software 2027' }).getAttribute('href')).toBe('/projekte/p-1/kriterienliste');
      expect(screen.getByRole('link', { name: 'Kriterienliste anpassen' }).getAttribute('href')).toBe('/projekte/p-1/kriterienliste');
      expect(screen.getByText('T.1')).toBeTruthy();
      expect(screen.queryByText(/neu in der Liste/)).toBeNull();
      await expectNoAxeViolations();
    });

    it('offers no „Kriterienliste anpassen“ to a Reader or on an archived project, the name still links the list', async () => {
      await openPage({ kind: 'project', project: WITH_LIST }, { role: 'reader' });
      expect(screen.queryByRole('link', { name: 'Kriterienliste anpassen' })).toBeNull();
      expect(screen.getByRole('link', { name: 'HR-Software 2027' })).toBeTruthy();
    });

    it('offers no „Kriterienliste anpassen“ on an archived project (R-PRJ-05)', async () => {
      await openPage({ kind: 'project', project: { ...WITH_LIST, status: 'archived' } }, { role: 'owner' });
      expect(screen.queryByRole('link', { name: 'Kriterienliste anpassen' })).toBeNull();
    });

    it('says when the project has no list yet', async () => {
      await openPage({ kind: 'project', project: PROJECT });

      expect(screen.getByText(/^Diesem Projekt ist noch keine Kriterienliste zugeordnet\./)).toBeTruthy();
      expect(screen.queryByRole('link', { name: 'Kriterienliste anpassen' })).toBeNull();
    });

    it('INV:WSP-03 shows and announces the criteria S11 of the copy took into the list, once', async () => {
      await openPage({ kind: 'project', project: WITH_LIST }, { added: 2 });

      expect(takeCriteriaAdded).toHaveBeenCalledWith('p-1');
      expect(screen.getByText('2 Kriterien neu in der Liste. Sie sind bei allen Produkten noch nicht eingeschätzt.')).toBeTruthy();
      expect(announce).toHaveBeenCalledWith('projects.detail.criteriaAdded', { count: 2 });
      await expectNoAxeViolations();
    });

    it('uses the singular for one added criterion', async () => {
      await openPage({ kind: 'project', project: WITH_LIST }, { added: 1 });

      expect(screen.getByText('1 Kriterium neu in der Liste. Es ist bei allen Produkten noch nicht eingeschätzt.')).toBeTruthy();
      expect(announce).toHaveBeenCalledWith('projects.detail.criteriaAdded.one', { count: 1 });
    });
  });

  it('shows „Seite nicht gefunden“ in place for a project the user cannot see', async () => {
    await openPage({ kind: 'notFound' });

    expect(screen.getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeTruthy();
    expect(setPageTitleState).not.toHaveBeenCalled();
  });
});
