// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { Title } from '@angular/platform-browser';
import { provideRouter, Router, TitleStrategy } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { screen } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { PageTitleStrategy } from './core/a11y/page-title.strategy';
import { routes } from './app.routes';

/** Start page and S80 pages on the real route table (R-SHL-05, R-SHL-08). */
describe('app routes', () => {
  let harness: RouterTestingHarness;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideTestTranslations(),
        provideRouter(routes),
        { provide: TitleStrategy, useClass: PageTitleStrategy },
      ],
    });
    harness = await RouterTestingHarness.create();
  });

  async function titleAfter(url: string): Promise<string> {
    await harness.navigateByUrl(url);
    await harness.fixture.whenStable();
    return TestBed.inject(Title).getTitle();
  }

  it('shows the start page at / with the application name first in the title', async () => {
    expect(await titleAfter('/')).toBe('ProcureAccess – Barrierefreie Software beschaffen');
    expect(screen.getByRole('heading', { level: 1, name: 'Barrierefreie Software beschaffen – Schritt für Schritt' })).toBeTruthy();
  });

  it('redirects the legacy address /home to the start page', async () => {
    await harness.navigateByUrl('/home');

    expect(TestBed.inject(Router).url).toBe('/');
  });

  it.each([
    ['/gibt-es-nicht', 'Seite nicht gefunden'],
    ['/unauthorized', 'Kein Zugriff'],
    ['/error', 'Etwas ist schiefgelaufen'],
  ])('shows the S80 page for %s with the page title = H1', async (url, h1) => {
    expect(await titleAfter(url)).toBe(`${h1} – ProcureAccess`);
    expect(screen.getByRole('heading', { level: 1, name: h1 })).toBeTruthy();
  });

  it('keeps the address of an unknown page', async () => {
    await harness.navigateByUrl('/gibt/es/nicht');

    expect(TestBed.inject(Router).url).toBe('/gibt/es/nicht');
  });
});
