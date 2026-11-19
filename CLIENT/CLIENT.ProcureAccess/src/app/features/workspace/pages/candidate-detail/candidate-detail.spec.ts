// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, inject, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { render, screen, within } from '@testing-library/angular';
import { of } from 'rxjs';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { CandidateTab } from '../../models/candidates';
import { candidateView, TEST_CANDIDATE, TEST_PROJECT } from '../../testing/candidate-fixtures';
import { CandidateDetailPage, candidateTabTitle } from './candidate-detail';
import { CandidateLoad, CandidateView } from './candidate-load';

@Component({ selector: 'pa-test-overview', template: '' })
class OverviewStub {}

/** A tab without content: titles the page as every tab does. */
@Component({ selector: 'pa-test-tab', template: '' })
class TabStub {
  constructor() {
    candidateTabTitle(inject(ActivatedRoute).snapshot.data['tab'] as CandidateTab);
  }
}

describe('CandidateDetailPage (S34 with its tabs as routes, R-CAN-03)', () => {
  let setPageTitleState: ReturnType<typeof vi.fn>;

  async function openPage(view: CandidateView = candidateView(), { url = '/projekte/p-1/produkte/c-1', wide = true } = {}) {
    setPageTitleState = vi.fn();
    const load = new CandidateLoad();
    load.view.set(view);
    const rendered = await render(CandidateDetailPage, {
      routes: [
        {
          path: 'projekte/:id/produkte/:pid',
          children: [
            { path: '', pathMatch: 'full', component: OverviewStub },
            { path: 'pruefnachweis', component: TabStub, data: { tab: 'evidence' } },
            { path: 'einschaetzung', component: TabStub, data: { tab: 'assessment' } },
          ],
        },
      ],
      providers: [
        provideTestTranslations(),
        { provide: CandidateLoad, useValue: load },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: ProcureAccessStore, useValue: { organizations: signal([]) } },
        { provide: BreakpointObserver, useValue: { observe: () => of({ matches: wide }), isMatched: () => wide } },
      ],
    });
    await TestBed.inject(Router).navigateByUrl(url);
    await rendered.fixture.whenStable();
    return rendered;
  }

  const tabLinks = () => screen.getAllByRole('tab');

  it('shows the product as H1, the vendor line and the three tabs as routes, Überblick selected, the tab in its panel', async () => {
    await openPage();

    expect(screen.getByRole('heading', { level: 1, name: 'BewerberPro' })).toBeTruthy();
    expect(screen.getByText('Anbieter: Beispiel Software GmbH')).toBeTruthy();
    // The path of the frame (owner decision S6-14 D6 (a)): Projekte › project › product
    const path = screen.getByRole('navigation', { name: 'Sie sind hier' });
    expect(within(path).getAllByRole('link').map((link) => [link.textContent?.trim(), link.getAttribute('href')])).toEqual([
      ['Projekte', '/projekte'],
      ['Neues Bewerbermanagement 2027', '/projekte/p-1'],
    ]);
    expect(within(path).getByText('BewerberPro').getAttribute('aria-current')).toBe('page');
    const links = tabLinks();
    expect(links.map((link) => link.textContent?.trim())).toEqual(['Überblick', 'Prüfnachweis', 'Einschätzung nach Kriterien']);
    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      '/projekte/p-1/produkte/c-1',
      '/projekte/p-1/produkte/c-1/pruefnachweis',
      '/projekte/p-1/produkte/c-1/einschaetzung',
    ]);
    expect(links.map((link) => link.getAttribute('aria-selected'))).toEqual(['true', 'false', 'false']);
    const panel = screen.getByRole('tabpanel', { name: 'Überblick' });
    expect(panel.querySelector('pa-test-overview')).not.toBeNull();
    await expectNoAxeViolations();
  });

  it('selects the tab of the address and gives its page its own title (R-CAN-03)', async () => {
    await openPage(candidateView(), { url: '/projekte/p-1/produkte/c-1/einschaetzung' });

    expect(tabLinks().map((link) => link.getAttribute('aria-selected'))).toEqual(['false', 'false', 'true']);
    expect(setPageTitleState).toHaveBeenCalledWith({
      params: { tab: 'Einschätzung nach Kriterien', product: 'BewerberPro', project: 'Neues Bewerbermanagement 2027' },
    });
  });

  it('shows the short label „Einschätzung“ below 600 px; the page title keeps the full name (U31)', async () => {
    await openPage(candidateView(), { url: '/projekte/p-1/produkte/c-1/einschaetzung', wide: false });

    expect(tabLinks().map((link) => link.textContent?.trim())).toEqual(['Überblick', 'Prüfnachweis', 'Einschätzung']);
    const [state] = setPageTitleState.mock.lastCall as [{ params: { tab: string } }];
    expect(state.params.tab).toBe('Einschätzung nach Kriterien');
  });

  it('leaves out the vendor line without a vendor and shows the archived note (R-PRJ-05)', async () => {
    await openPage(candidateView({ project: { ...TEST_PROJECT, status: 'archived' }, candidate: { ...TEST_CANDIDATE, vendor: null } }));

    expect(screen.queryByText(/^Anbieter:/)).toBeNull();
    expect(screen.getByText(/^Dieses Projekt ist archiviert\./)).toBeTruthy();
  });

  it('INV:TEN-01 shows „Seite nicht gefunden“ in place, without tabs', async () => {
    await openPage({ kind: 'notFound' });

    expect(screen.getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeTruthy();
    expect(screen.queryByRole('tablist')).toBeNull();
  });
});
