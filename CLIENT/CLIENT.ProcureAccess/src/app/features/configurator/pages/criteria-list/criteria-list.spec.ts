// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { signalStore } from '@ngrx/signals';
import { render, screen, within } from '@testing-library/angular';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { withConfigurator } from '../../state/with-configurator';
import { criteriaAnswer } from '../../testing/criteria-fixture';
import { filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { CriteriaListPage } from './criteria-list';

// The real configurator slice stands in for the root store, so the page runs the S4-14 rules.
const ConfiguratorTestStore = signalStore(withConfigurator());

@Component({ template: '' })
class Blank {}

describe('CriteriaListPage (S11, R-LST-01, R-LST-02)', () => {
  let api: { getFilterOptions: ReturnType<typeof vi.fn>; getCriteria: ReturnType<typeof vi.fn> };

  async function setUp(getCriteria: ReturnType<typeof vi.fn>) {
    api = {
      getFilterOptions: vi.fn().mockResolvedValue(filterOptionsAnswer()),
      getCriteria,
    };
    const rendered = await render(CriteriaListPage, {
      providers: [
        provideTestTranslations(),
        provideRouter([
          { path: 'filters', component: Blank },
          { path: 'kriterien', component: Blank },
        ]),
        { provide: ProcureAccessStore, useClass: ConfiguratorTestStore },
        { provide: CatalogApiService, useValue: api },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  it('shows H1, „Auswahl ändern“ back to S10 and one card per criterion in catalog order', async () => {
    await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

    expect(screen.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Auswahl ändern' }).getAttribute('href')).toBe('/kriterien');
    const cards = within(screen.getByRole('list')).getAllByRole('listitem');
    expect(cards.map((card) => within(card).getByRole('heading', { level: 2 }).textContent?.trim())).toEqual([
      'Bilder haben einen Alternativtext',
      'Alles geht mit der Tastatur',
      'Überschriften',
      'Geprüft nach BIT inklusiv Software-Test',
    ]);
    await expectNoAxeViolations();
  });

  it('asks for the criteria list on every entry, as S11 of the current draft', async () => {
    await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

    expect(api.getCriteria).toHaveBeenCalledTimes(1);
    expect(api.getCriteria.mock.calls[0]?.[2]).toEqual(['not-found']);
  });

  it('INV:CAT-02 names the matched rule with the catalog names of the draft', async () => {
    const answer = criteriaAnswer();
    const first = answer.criteria[0];
    if (first === undefined) throw new Error('fixture has criteria');
    first.resolvedPriority.matchedRule = { kind: 'appType', appType: 'web', useCase: null };
    const { fixture } = await setUp(vi.fn().mockResolvedValue(answer));
    const store = fixture.debugElement.injector.get<InstanceType<typeof ConfiguratorTestStore>>(ProcureAccessStore);
    await store.loadFilterOptions();
    await fixture.whenStable();

    screen.getAllByRole('button', { name: 'Details anzeigen' })[0]?.click();
    await fixture.whenStable();

    expect(screen.getByText('Diese Empfehlung gilt für Web, unabhängig vom Verwendungszweck.')).toBeTruthy();
  });

  it('opens the legacy filter when no release is active (404, O-S4-3)', async () => {
    const { fixture } = await setUp(
      vi.fn().mockRejectedValue(new HttpErrorResponse({ status: 404, error: { type: 'x/problems/not-found' } })),
    );
    await fixture.whenStable();

    expect(fixture.debugElement.injector.get(Router).url).toBe('/filters');
  });

  it('stays where the user went when the 404 answers after they left S11', async () => {
    let answer404: (error: unknown) => void = () => undefined;
    const pending = new Promise((_, reject) => (answer404 = reject));
    const { fixture } = await setUp(vi.fn().mockReturnValue(pending));
    const router = fixture.debugElement.injector.get(Router);

    fixture.destroy();
    answer404(new HttpErrorResponse({ status: 404, error: { type: 'x/problems/not-found' } }));
    await pending.catch(() => undefined);
    await new Promise((resolve) => setTimeout(resolve));

    expect(router.url).toBe('/');
  });

  it('keeps H1 and „Auswahl ändern“ and shows no list when loading failed (O-S4-34)', async () => {
    const { fixture } = await setUp(vi.fn().mockRejectedValue(new HttpErrorResponse({ status: 400 })));

    expect(screen.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Auswahl ändern' })).toBeTruthy();
    expect(screen.queryByRole('list')).toBeNull();
    expect(fixture.debugElement.injector.get(Router).url).toBe('/');
  });
});
