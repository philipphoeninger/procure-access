// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Location } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, RouterOutlet, TitleStrategy } from '@angular/router';
import { render, screen } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PAGE_TITLE_KEY, PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { CONFIRM_EMAIL_PATH } from '../../models/login-navigation';
import { AuthService } from '../../services/auth.service';
import { CONFIRM_EMAIL_ROUTES } from './confirm-email.routing';

@Component({
  template: '<h1>Startseite</h1>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HomePage {}

@Component({
  template: '<h1>Anmelden</h1>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class LoginStub {}

@Component({
  imports: [RouterOutlet],
  template: '<main id="main"><router-outlet /><router-outlet name="login" /></main>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class Host {}

const base = 'https://procureaccess.de/problems/';

describe('Confirmation page of S02 (R-AUTH-02, /registrieren/bestaetigen)', () => {
  let calls: string[];
  let confirmEmail: ReturnType<typeof vi.fn>;
  let store: {
    sessionReady: ReturnType<typeof vi.fn>;
    incrementLoadingCount: ReturnType<typeof vi.fn>;
    decrementLoadingCount: ReturnType<typeof vi.fn>;
  };

  async function openLink(url: string, answer: { rejectWith?: unknown } = {}) {
    calls = [];
    const reply: () => Promise<void> =
      answer.rejectWith === undefined ? vi.fn().mockResolvedValue(undefined) : vi.fn().mockRejectedValue(answer.rejectWith);
    confirmEmail = vi.fn((token: string) => {
      calls.push(`confirm:${token}`);
      return reply();
    });
    store = {
      sessionReady: vi.fn(() => {
        calls.push('probe');
        return Promise.resolve();
      }),
      incrementLoadingCount: vi.fn(() => calls.push('loading+')),
      decrementLoadingCount: vi.fn(() => calls.push('loading-')),
    };
    const rendered = await render(Host, {
      routes: [
        { path: '', component: HomePage, data: { [PAGE_TITLE_KEY]: 'pageTitle.home' } },
        { path: CONFIRM_EMAIL_PATH, children: CONFIRM_EMAIL_ROUTES },
        { path: 'anmelden', component: LoginStub, outlet: 'login' },
      ],
      providers: [
        provideTestTranslations(),
        { provide: TitleStrategy, useExisting: PageTitleStrategy },
        { provide: ProcureAccessStore, useValue: store },
        { provide: AuthService, useValue: { confirmEmail } },
        { provide: A11yAnnouncerService, useValue: { announce: vi.fn(), announcePageTitle: vi.fn().mockResolvedValue(undefined) } },
      ],
    });
    const router = TestBed.inject(Router);
    await router.navigateByUrl(url);
    await rendered.fixture.whenStable();
    return { ...rendered, router, location: TestBed.inject(Location) };
  }

  it('takes the token out of the address before it sends anything, then confirms (S5-12a R4)', async () => {
    const { router, location } = await openLink('/registrieren/bestaetigen?token=abc_DEF-123');

    expect(router.url).toBe('/registrieren/bestaetigen');
    expect(location.path()).toBe('/registrieren/bestaetigen');
    expect(confirmEmail).toHaveBeenCalledExactlyOnceWith('abc_DEF-123');
  });

  it('waits for the session probe (the antiforgery token) and shows the loading bar meanwhile', async () => {
    await openLink('/registrieren/bestaetigen?token=t1');

    expect(calls).toEqual(['loading+', 'probe', 'confirm:t1', 'loading-']);
  });

  it('shows „E-Mail-Adresse bestätigt“ like S80 with the same page title when the server confirms', async () => {
    await openLink('/registrieren/bestaetigen?token=t1');

    expect(screen.getByRole('heading', { level: 1, name: 'E-Mail-Adresse bestätigt' })).toBeTruthy();
    expect(screen.getByText('Ihre E-Mail-Adresse ist bestätigt. Sie können jetzt alle Funktionen nutzen.')).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Zur Startseite' }).getAttribute('href')).toBe('/');
    await vi.waitFor(() => expect(document.title).toBe('E-Mail-Adresse bestätigt – ProcureAccess'));
    await expectNoAxeViolations();
  });

  it('shows „Link nicht mehr gültig“ when the server refuses the token', async () => {
    const refused = new HttpErrorResponse({ status: 400, error: { type: `${base}validation`, errors: { token: ['expired'] } } });
    await openLink('/registrieren/bestaetigen?token=old', { rejectWith: refused });

    expect(screen.getByRole('heading', { level: 1, name: 'Link nicht mehr gültig' })).toBeTruthy();
    expect(screen.getByText(/Melden Sie sich an und fordern Sie einen neuen Link an/)).toBeTruthy();
    await vi.waitFor(() => expect(document.title).toBe('Link nicht mehr gültig – ProcureAccess'));
    await expectNoAxeViolations();
  });

  it('shows „Link nicht mehr gültig“ without a request when the address has no token (e.g. a reload)', async () => {
    await openLink('/registrieren/bestaetigen');

    expect(screen.getByRole('heading', { level: 1, name: 'Link nicht mehr gültig' })).toBeTruthy();
    expect(confirmEmail).not.toHaveBeenCalled();
  });

  it('shows the S80 server text, not „nicht mehr gültig“, when the answer says nothing about the link', async () => {
    await openLink('/registrieren/bestaetigen?token=t1', { rejectWith: new HttpErrorResponse({ status: 503 }) });

    expect(screen.getByRole('heading', { level: 1, name: 'Etwas ist schiefgelaufen' })).toBeTruthy();
    expect(screen.queryByText(/nicht mehr gültig/)).toBeNull();
    expect(store.decrementLoadingCount).toHaveBeenCalledOnce();
  });

  it('keeps the result when S01 opens over the page and closes again', async () => {
    const { router, fixture } = await openLink('/registrieren/bestaetigen?token=t1');

    await router.navigate([{ outlets: { login: ['anmelden'] } }]);
    await router.navigate([{ outlets: { login: null } }]);
    await fixture.whenStable();

    expect(confirmEmail).toHaveBeenCalledOnce();
    expect(screen.getByRole('heading', { level: 1, name: 'E-Mail-Adresse bestätigt' })).toBeTruthy();
  });

  it('confirms a second link opened on the page', async () => {
    const { router, fixture } = await openLink('/registrieren/bestaetigen?token=first', {
      rejectWith: new HttpErrorResponse({ status: 400, error: { type: `${base}validation` } }),
    });
    confirmEmail.mockResolvedValue(undefined);

    await router.navigateByUrl('/registrieren/bestaetigen?token=second');
    await fixture.whenStable();

    expect(confirmEmail).toHaveBeenLastCalledWith('second');
    expect(router.url).toBe('/registrieren/bestaetigen');
    expect(screen.getByRole('heading', { level: 1, name: 'E-Mail-Adresse bestätigt' })).toBeTruthy();
  });
});
