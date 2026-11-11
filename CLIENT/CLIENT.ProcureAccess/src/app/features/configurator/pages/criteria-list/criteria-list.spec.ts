// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { CriteriaListPage } from './criteria-list';

describe('CriteriaListPage (S11 placeholder)', () => {
  it('shows the H1 and „Auswahl ändern“ back to S10', async () => {
    await render(CriteriaListPage, { providers: [provideTestTranslations(), provideRouter([])] });

    expect(screen.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Auswahl ändern' }).getAttribute('href')).toBe('/kriterien');
    await expectNoAxeViolations();
  });
});
