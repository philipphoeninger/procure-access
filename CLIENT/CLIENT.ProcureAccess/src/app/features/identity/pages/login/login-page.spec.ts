// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, RouterOutlet, TitleStrategy } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PAGE_TITLE_KEY, PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { LOGIN_PATH } from '../../models/login-navigation';
import { AuthService } from '../../services/auth.service';
import { LoginPage } from './login-page';

@Component({
  template: '<h1>Ihre Kriterienliste</h1>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class ListPage {}

/** The primary page and the `login` outlet, as in the shell. */
@Component({
  imports: [RouterOutlet],
  template: '<main id="main"><router-outlet /><router-outlet name="login" /></main>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class Host {}

const base = 'https://procureaccess.de/problems/';

describe('LoginPage (S01, R-AUTH-03)', () => {
  let startSession: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;

  async function openLogin(options: { state?: object; listLoaded?: boolean; rejectWith?: unknown } = {}) {
    startSession =
      options.rejectWith === undefined ? vi.fn().mockResolvedValue(undefined) : vi.fn().mockRejectedValue(options.rejectWith);
    announce = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(Host, {
      routes: [
        { path: 'kriterien/ergebnis', component: ListPage, data: { [PAGE_TITLE_KEY]: 'pageTitle.criteria.list' } },
        { path: LOGIN_PATH, component: LoginPage, outlet: 'login', data: { [PAGE_TITLE_KEY]: 'pageTitle.login' } },
        { path: 'register', component: ListPage, outlet: 'login' },
      ],
      providers: [
        provideTestTranslations(),
        { provide: TitleStrategy, useExisting: PageTitleStrategy },
        {
          provide: ProcureAccessStore,
          useValue: {
            startSession,
            configurator: { list: { answer: () => (options.listLoaded ? { criteria: [] } : null) } },
          },
        },
        { provide: AuthService, useValue: { forgotPassword: vi.fn() } },
        { provide: A11yAnnouncerService, useValue: { announce, announcePageTitle: vi.fn().mockResolvedValue(undefined) } },
      ],
    });
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/kriterien/ergebnis');
    await router.navigate([{ outlets: { login: [LOGIN_PATH] } }], { state: options.state });
    await rendered.fixture.whenStable();
    return { ...rendered, router };
  }

  async function fillAndSubmit(fixture: { whenStable(): Promise<unknown> }) {
    fireEvent.input(screen.getByLabelText('E-Mail-Adresse'), { target: { value: 'mod@example.test' } });
    fireEvent.input(screen.getByLabelText('Passwort'), { target: { value: 'secret' } });
    screen.getByRole('button', { name: 'Anmelden' }).click();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  it('shows the H1, the form card and the link to the registration over the page that opened it', async () => {
    const { router } = await openLogin();

    expect(router.url).toBe('/kriterien/ergebnis(login:anmelden)');
    expect(screen.getByRole('heading', { level: 1, name: 'Anmelden' })).toBeTruthy();
    expect(screen.getByLabelText('E-Mail-Adresse').id).toBe('pa-login-email');
    expect(screen.queryByText(/um Ihre Kriterienliste zu speichern/)).toBeNull();
    const register = screen.getByRole('link', { name: 'Noch kein Konto? Jetzt registrieren' });
    expect(register.getAttribute('href')).toBe('/kriterien/ergebnis(login:register)');
    expect(document.title).toBe('Anmelden – ProcureAccess');
    await expectNoAxeViolations();
  });

  it('says why when S11 „Liste speichern“ opened it (auth.login.reasonSave)', async () => {
    await openLogin({ state: { loginReason: 'save' } });

    expect(screen.getByText('Melden Sie sich an, um Ihre Kriterienliste zu speichern. Ihre Auswahl bleibt dabei erhalten.')).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('closes the outlet after the sign-in and keeps the page underneath', async () => {
    const { fixture, router } = await openLogin();

    await fillAndSubmit(fixture);

    expect(router.url).toBe('/kriterien/ergebnis');
    expect(screen.queryByRole('heading', { level: 1, name: 'Anmelden' })).toBeNull();
    expect(screen.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeTruthy();
    expect(announce).not.toHaveBeenCalledWith('auth.login.draftKept');
  });

  it('announces that the list is still there when it came to save one (auth.login.draftKept)', async () => {
    const { fixture } = await openLogin({ state: { loginReason: 'save' }, listLoaded: true });

    await fillAndSubmit(fixture);

    expect(announce).toHaveBeenCalledWith('auth.login.draftKept');
  });

  it('prefixes the page title with „Fehler:“ while the error summary shows', async () => {
    const refused = new HttpErrorResponse({ status: 400, error: { type: `${base}validation`, errors: { $: ['wrong'] } } });
    const { fixture } = await openLogin({ rejectWith: refused });

    screen.getByRole('button', { name: 'Anmelden' }).click();
    await fixture.whenStable();
    await vi.waitFor(() => expect(document.title).toBe('Fehler: Anmelden – ProcureAccess'));

    await fillAndSubmit(fixture);
    expect(document.title).toBe('Fehler: Anmelden – ProcureAccess');
    expect(screen.getByText(/Anmeldung nicht möglich/)).toBeTruthy();
  });
});
