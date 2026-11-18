// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  RedirectCommand,
  Router,
  RouterOutlet,
  RouterStateSnapshot,
} from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { LOGIN_PATH, LoginNavigationState } from '@features/identity/models/login-navigation';
import { authGuard } from './auth.guard';

@Component({ selector: 'pa-test-start', template: '<h1>Start</h1>', changeDetection: ChangeDetectionStrategy.OnPush })
class StartPage {}

@Component({ selector: 'pa-test-account', template: '<h1>Konto</h1>', changeDetection: ChangeDetectionStrategy.OnPush })
class AccountPage {}

/** Stands in for S01: records the router state of the navigation that opened it, read as S01 reads it. */
@Component({ selector: 'pa-test-login', template: '<h1>Anmelden</h1>', changeDetection: ChangeDetectionStrategy.OnPush })
class LoginProbe {
  static seen: LoginNavigationState | undefined;
  constructor() {
    const router = inject(Router);
    const navigation = router.currentNavigation() ?? router.lastSuccessfulNavigation();
    LoginProbe.seen = navigation?.extras.state;
  }
}

@Component({
  imports: [RouterOutlet],
  template: '<router-outlet /><router-outlet name="login" />',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class Host {}

describe('authGuard (S6-03, TD-65)', () => {
  let signedIn: boolean;

  beforeEach(() => {
    signedIn = false;
    LoginProbe.seen = undefined;
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: '', component: StartPage },
          { path: 'konto', component: AccountPage, canActivate: [authGuard] },
          { path: LOGIN_PATH, component: LoginProbe, outlet: 'login' },
        ]),
        {
          provide: ProcureAccessStore,
          useValue: { sessionReady: () => Promise.resolve(), signedIn: () => signedIn },
        },
      ],
    });
  });

  const run = (url: string) =>
    TestBed.runInInjectionContext(() => authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot));

  it('lets a signed-in user through', async () => {
    signedIn = true;

    expect(await run('/konto')).toBe(true);
  });

  it('sends a signed-out user to S01 over the start page with the page they opened as returnUrl', async () => {
    const result = await run('/konto?tab=daten');

    expect(result).toBeInstanceOf(RedirectCommand);
    const command = result as RedirectCommand;
    expect(TestBed.inject(Router).serializeUrl(command.redirectTo)).toBe('/(login:anmelden)');
    expect(command.navigationBehaviorOptions?.state).toEqual({ returnUrl: '/konto?tab=daten' });
  });

  it('hands the returnUrl to S01 through the router state, not the URL, while the shell is open', async () => {
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const router = TestBed.inject(Router);

    await router.navigateByUrl('/konto');
    await fixture.whenStable();

    expect(router.url).toBe('/(login:anmelden)');
    expect(LoginProbe.seen).toEqual(expect.objectContaining({ returnUrl: '/konto' }));
  });

  it('hands it over too on a deep link, when the outlet only exists after the first navigation', async () => {
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/konto');

    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();

    expect(router.url).toBe('/(login:anmelden)');
    expect(LoginProbe.seen).toEqual(expect.objectContaining({ returnUrl: '/konto' }));
  });
});
