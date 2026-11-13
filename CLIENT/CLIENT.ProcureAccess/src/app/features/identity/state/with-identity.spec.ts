// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { signalStore } from '@ngrx/signals';

import { API_URL } from '@app/app.config';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { SessionResponse } from '../models/session';
import { IdentityApiService } from '../services/identity-api.service';
import { withIdentity } from './with-identity';

const IdentityTestStore = signalStore(withIdentity());

const PROBE = '/api/v1/auth/session';
const LOGIN = '/api/v1/auth/login';
const LOGOUT = '/api/v1/auth/logout';
const ME = '/api/me';

const moderator: SessionResponse = {
  user: { id: 'u1', email: 'mod@example.test', roles: ['Moderator'], permissions: ['moderation:review'] },
  expiresAt: '2026-11-13T10:30:00+00:00',
  absoluteExpiresAt: '2026-11-13T18:00:00+00:00',
};
const anonymous: SessionResponse = { user: null };
const legacyUser = { id: 'u1', email: 'mod@example.test', uiCustomization: { language: 'de' } };

let backend: HttpTestingController;
let router: { navigateByUrl: ReturnType<typeof vi.fn> };

function setUp(apiService: Partial<IdentityApiService> = {}) {
  router = { navigateByUrl: vi.fn().mockResolvedValue(true) };
  TestBed.configureTestingModule({
    providers: [
      IdentityTestStore,
      provideHttpClient(),
      provideHttpClientTesting(),
      { provide: API_URL, useValue: '/api' },
      { provide: Router, useValue: router },
      { provide: IdentityApiService, useValue: apiService },
      { provide: SnackbarService, useValue: { showInfo: vi.fn() } },
    ],
  });
  backend = TestBed.inject(HttpTestingController);
  return TestBed.inject(IdentityTestStore);
}

/** Lets the store's awaited promises settle (a macrotask), then expects the one request. */
async function expectRequest(url: string): Promise<TestRequest> {
  await new Promise((resolve) => setTimeout(resolve, 0));
  return backend.expectOne(url);
}

/** Answers the probe and, for a signed-in user, the legacy user request. */
async function answerProbe(response: SessionResponse): Promise<void> {
  (await expectRequest(PROBE)).flush(response);
  if (response.user) {
    (await expectRequest(ME)).flush(legacyUser);
  }
}

afterEach(() => backend.verify());

describe('withIdentity (session from GET /auth/session, ARCHITECTURE §8.3)', () => {
  it('is signed out until the first probe answered', () => {
    const store = setUp();

    expect(store.sessionStatus()).toBe('unknown');
    expect(store.signedIn()).toBe(false);
    expect(store.permissions()).toEqual([]);
  });

  it('holds the session, the expiry times and the legacy user after a probe with a user', async () => {
    const store = setUp();

    const loading = store.loadSession();
    await answerProbe(moderator);
    await loading;

    expect(store.signedIn()).toBe(true);
    expect(store.session()).toEqual(moderator.user);
    expect(store.sessionExpiresAt()).toBe(moderator.expiresAt);
    expect(store.sessionAbsoluteExpiresAt()).toBe(moderator.absoluteExpiresAt);
    expect(store.hasPermission('moderation:review')).toBe(true);
    expect(store.hasPermission('catalog:edit')).toBe(false);
    expect(store.user()).toEqual(legacyUser);
    expect(store.sessionStatus()).toBe('ready');
  });

  it('is signed out after an anonymous probe and loads no legacy user', async () => {
    const store = setUp();

    const loading = store.loadSession();
    await answerProbe(anonymous);
    await loading;

    expect(store.signedIn()).toBe(false);
    expect(store.user()).toBeNull();
    expect(store.sessionStatus()).toBe('ready');
  });

  it('counts a failed probe as „nobody signed in“ and never rejects', async () => {
    const store = setUp();

    const loading = store.loadSession();
    (await expectRequest(PROBE)).flush('', { status: 503, statusText: 'Service Unavailable' });
    await expect(loading).resolves.toBeNull();

    expect(store.signedIn()).toBe(false);
    expect(store.sessionStatus()).toBe('ready');
  });

  it('sessionReady() probes once for every caller and starts the probe itself', async () => {
    const store = setUp();

    const first = store.sessionReady();
    const second = store.sessionReady();
    await answerProbe(moderator);
    await Promise.all([first, second]);

    expect(store.signedIn()).toBe(true);
    backend.expectNone(PROBE);
  });

  it('signIn() posts the credentials, then reloads the session and the legacy user', async () => {
    const store = setUp();

    const signingIn = store.signIn('mod@example.test', 'secret');
    const login = await expectRequest(LOGIN);
    expect(login.request.body).toEqual({ email: 'mod@example.test', password: 'secret' });
    login.flush(null, { status: 204, statusText: 'No Content' });
    await answerProbe(moderator);
    await expect(signingIn).resolves.toEqual(legacyUser);

    expect(store.signedIn()).toBe(true);
  });

  it('signIn() rejects with the refused login and stays signed out', async () => {
    const store = setUp();

    const signingIn = store.signIn('mod@example.test', 'wrong');
    (await expectRequest(LOGIN)).flush(
      { type: 'https://procureaccess.de/problems/validation', errors: { $: ['wrong'] } },
      { status: 400, statusText: 'Bad Request' },
    );

    await expect(signingIn).rejects.toMatchObject({ status: 400 });
    expect(store.signedIn()).toBe(false);
  });

  it('signOut() posts the logout and forgets the session, also when the request fails', async () => {
    const store = setUp();
    const loading = store.loadSession();
    await answerProbe(moderator);
    await loading;

    const signingOut = store.signOut();
    (await expectRequest(LOGOUT)).flush('', { status: 503, statusText: 'Service Unavailable' });
    await expect(signingOut).rejects.toMatchObject({ status: 503 });

    expect(store.signedIn()).toBe(false);
    expect(store.user()).toBeNull();
    expect(store.permissions()).toEqual([]);
  });

  // SEC-01 (FE part): no token is written to or read from browser storage, and no request carries a
  // credential of its own — the session is the HttpOnly cookie, which the browser sends itself.
  it('Session_StorageBlocked_SignsInAndOutWithoutTokenOrAuthorizationHeader INV:SEC-01', async () => {
    const blocked = () => {
      throw new DOMException('Storage is blocked', 'SecurityError');
    };
    const storage = ['getItem', 'setItem', 'removeItem', 'clear'].map((method) =>
      vi.spyOn(Storage.prototype, method as 'getItem').mockImplementation(blocked),
    );
    try {
      const store = setUp();
      const requests: TestRequest[] = [];

      const loading = store.loadSession();
      requests.push(await expectRequest(PROBE));
      requests.at(-1)!.flush(anonymous);
      await loading;

      const signingIn = store.signIn('mod@example.test', 'secret');
      requests.push(await expectRequest(LOGIN));
      requests.at(-1)!.flush(null, { status: 204, statusText: 'No Content' });
      requests.push(await expectRequest(PROBE));
      requests.at(-1)!.flush(moderator);
      requests.push(await expectRequest(ME));
      requests.at(-1)!.flush(legacyUser);
      await signingIn;
      expect(store.signedIn()).toBe(true);

      const signingOut = store.signOut();
      requests.push(await expectRequest(LOGOUT));
      requests.at(-1)!.flush(null, { status: 204, statusText: 'No Content' });
      await signingOut;
      expect(store.signedIn()).toBe(false);

      for (const request of requests) {
        expect(request.request.headers.has('Authorization')).toBe(false);
      }
      for (const spy of storage) {
        expect(spy).not.toHaveBeenCalled();
      }
    } finally {
      vi.restoreAllMocks();
    }
  });

  // #168: a failed request must not leave the loading bar on (pa-loading-bar, R-SHL-09).
  it('stops loading when deleting the account fails', async () => {
    const failure = new Error('request failed');
    const store = setUp({ deleteAccount: () => Promise.reject(failure) });

    await expect(store.deleteAccount()).rejects.toBe(failure);

    expect(store.isLoading()).toBe(false);
  });

  it('ends the session and opens the start page after the account was deleted', async () => {
    const store = setUp({ deleteAccount: () => Promise.resolve(null) });

    const deleting = store.deleteAccount();
    (await expectRequest(LOGOUT)).flush(null, { status: 204, statusText: 'No Content' });
    await deleting;

    expect(store.signedIn()).toBe(false);
    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });
});
