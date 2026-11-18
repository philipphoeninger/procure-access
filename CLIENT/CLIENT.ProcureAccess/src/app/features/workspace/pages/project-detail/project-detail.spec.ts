// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
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
};

describe('ProjectDetailPage (S32 placeholder of S6-13)', () => {
  let setPageTitleState: ReturnType<typeof vi.fn>;

  async function openPage(view: ProjectView) {
    setPageTitleState = vi.fn();
    const load = new ProjectLoad();
    load.view = view;
    const rendered = await render(ProjectDetailPage, {
      providers: [
        provideTestTranslations(),
        { provide: ProjectLoad, useValue: load },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
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

  it('shows „Seite nicht gefunden“ in place for a project the user cannot see', async () => {
    await openPage({ kind: 'notFound' });

    expect(screen.getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeTruthy();
    expect(setPageTitleState).not.toHaveBeenCalled();
  });
});
