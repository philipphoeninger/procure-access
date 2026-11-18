// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';
import { withLoading } from '@app/shared/state/with-loading';

import { API_URL } from '@app/app.config';
import { OrganizationSummary } from '@app/features/organization/models/organization';
import { SessionLimits } from '@app/core/session/session-limits';
import { SessionResponse } from '../models/session';
import { withIdentity } from './with-identity';

const IdentityTestStore = signalStore(withLoading(), withIdentity());

const PROBE = '/api/v1/auth/session';
const LOGIN = '/api/v1/auth/login';
const LOGOUT = '/api/v1/auth/logout';
const ME = '/api/me';
const ORGANIZATIONS = '/api/v1/organizations';

const moderator: SessionResponse = {
  user: { id: 'u1', email: 'mod@example.test', roles: ['Moderator'], permissions: ['moderation:review'] },
  expiresAt: '2026-11-13T10:30:00+00:00',
  absoluteExpiresAt: '2026-11-13T18:00:00+00:00',
  warningLeadSeconds: 300,
};
const anonymous: SessionResponse = { user: null };
const legacyUser = { id: 'u1', email: 'mod@example.test' };
const amt: OrganizationSummary = { id: 'org-a', name: 'Ämter Musterhausen', type: 'publicBody', role: 'owner' };
const verein: OrganizationSummary = { id: 'org-v', name: 'Verein Beispiel', type: 'association', role: 'reader' };

let backend: HttpTestingController;
function setUp() {
  TestBed.configureTestingModule({
    providers: [
      IdentityTestStore,
      provideHttpClient(),
      provideHttpClientTesting(),
      { provide: API_URL, useValue: '/api' },
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

/** Answers the probe and, for a signed-in user, the legacy user and the organisations requests. */
async function answerProbe(response: SessionResponse, organizations: OrganizationSummary[] = []): Promise<void> {
  (await expectRequest(PROBE)).flush(response);
  if (response.user) {
    (await expectRequest(ME)).flush(legacyUser);
    backend.expectOne(ORGANIZATIONS).flush(organizations);
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
    expect(store.sessionWarningLeadSeconds()).toBe(300);
    expect(store.hasPermission('moderation:review')).toBe(true);
    expect(store.hasPermission('catalog:edit')).toBe(false);
    expect(store.user()).toEqual(legacyUser);
    expect(store.sessionStatus()).toBe('ready');
  });

  it('shows the limits the expiry headers moved, and none once nobody is signed in (SEC-07)', async () => {
    const store = setUp();
    const limits = TestBed.inject(SessionLimits);
    const loading = store.loadSession();
    await answerProbe(moderator);
    await loading;

    // What the session interceptor does with the headers of a later answer.
    limits.set('2026-11-13T10:50:00Z', '2026-11-13T18:00:00Z');
    expect(store.sessionExpiresAt()).toBe('2026-11-13T10:50:00Z');
    expect(store.sessionAbsoluteExpiresAt()).toBe('2026-11-13T18:00:00Z');

    const signingOut = store.signOut();
    (await expectRequest(LOGOUT)).flush(null, { status: 204, statusText: 'No Content' });
    await signingOut;
    expect(store.sessionExpiresAt()).toBeNull();
    expect(store.sessionAbsoluteExpiresAt()).toBeNull();
    expect(store.sessionWarningLeadSeconds()).toBeNull();

    // A late answer to a request sent before the sign-out.
    limits.set('2026-11-13T10:55:00Z', '2026-11-13T18:00:00Z');
    expect(store.sessionExpiresAt()).toBeNull();
    expect(store.sessionAbsoluteExpiresAt()).toBeNull();
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

  it('resendConfirmation() posts the resend without a body and rejects with its problem (R-AUTH-02)', async () => {
    const store = setUp();

    const sending = store.resendConfirmation();
    const request = await expectRequest('/api/v1/auth/confirm-email/resend');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush({ type: 'https://procureaccess.de/problems/email-delivery' }, { status: 503, statusText: 'Service Unavailable' });

    await expect(sending).rejects.toMatchObject({ status: 503 });
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
      requests.push(backend.expectOne(ORGANIZATIONS));
      requests.at(-1)!.flush([]);
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

  it('forgets the session of a deleted account without a sign-out request (R-AUTH-10)', async () => {
    const store = setUp();
    const loading = store.loadSession();
    await answerProbe(moderator, [amt]);
    await loading;

    store.forgetSession();

    expect(store.signedIn()).toBe(false);
    expect(store.user()).toBeNull();
    expect(store.organizations()).toEqual([]);
    // backend.verify(): no request went out.
  });

  describe('refreshSession() (the session warning, R-AUTH-06; S5-30)', () => {
    async function signedIn() {
      const store = setUp();
      const loading = store.loadSession();
      await answerProbe(moderator, [amt]);
      await loading;
      return store;
    }

    it('takes the limits of a session that is still there and answers signedIn', async () => {
      const store = await signedIn();

      const checking = store.refreshSession();
      (await expectRequest(PROBE)).flush({ ...moderator, expiresAt: '2026-11-13T10:45:00+00:00' });

      expect(await checking).toBe('signedIn');
      expect(store.sessionExpiresAt()).toBe('2026-11-13T10:45:00+00:00');
      expect(store.organizations()).toEqual([amt]);
      // backend.verify(): the same user reads neither the legacy user nor the organisations again.
    });

    it('forgets a session the server ended and answers signedOut', async () => {
      const store = await signedIn();

      const checking = store.refreshSession();
      (await expectRequest(PROBE)).flush(anonymous);

      expect(await checking).toBe('signedOut');
      expect(store.signedIn()).toBe(false);
      expect(store.sessionExpiresAt()).toBeNull();
    });

    it('keeps the session when the probe fails and answers unknown (offline is not signed out)', async () => {
      const store = await signedIn();

      const checking = store.refreshSession();
      (await expectRequest(PROBE)).error(new ProgressEvent('error'), { status: 0 });

      expect(await checking).toBe('unknown');
      expect(store.signedIn()).toBe(true);
      expect(store.sessionExpiresAt()).toBe(moderator.expiresAt);
    });
  });

  describe('organisations (R-SHL-02, R-SHL-03; S5-18)', () => {
    async function signedInWith(organizations: OrganizationSummary[]) {
      const store = setUp();
      const loading = store.loadSession();
      await answerProbe(moderator, organizations);
      await loading;
      return store;
    }

    it('reads the organisations with the session; without a membership there is no active one', async () => {
      const store = await signedInWith([]);

      expect(store.organizations()).toEqual([]);
      expect(store.activeOrganization()).toBeNull();
    });

    it('starts with the first organisation by name and switches to another one of the user', async () => {
      const store = await signedInWith([amt, verein]);
      expect(store.activeOrganization()).toEqual(amt);

      store.switchOrganization(verein.id);

      expect(store.activeOrganization()).toEqual(verein);
    });

    it('ignores a switch to an organisation that is not the user\'s', async () => {
      const store = await signedInWith([amt, verein]);

      store.switchOrganization('foreign');

      expect(store.activeOrganization()).toEqual(amt);
    });

    it('falls back to the first organisation when the chosen one is gone after reading them again', async () => {
      const store = await signedInWith([amt, verein]);
      store.switchOrganization(verein.id);

      const reading = store.loadOrganizations();
      backend.expectOne(ORGANIZATIONS).flush([amt]);
      await reading;

      expect(store.activeOrganization()).toEqual(amt);
    });

    it('keeps the organisations when reading them again fails (background request)', async () => {
      const store = await signedInWith([amt, verein]);

      const reading = store.loadOrganizations();
      backend.expectOne(ORGANIZATIONS).flush('', { status: 503, statusText: 'Service Unavailable' });
      await reading;

      expect(store.organizations()).toEqual([amt, verein]);
    });

    it('forgets the organisations and the choice on sign-out', async () => {
      const store = await signedInWith([amt, verein]);
      store.switchOrganization(verein.id);

      const signingOut = store.signOut();
      (await expectRequest(LOGOUT)).flush(null, { status: 204, statusText: 'No Content' });
      await signingOut;

      expect(store.organizations()).toEqual([]);
      expect(store.activeOrganization()).toBeNull();
    });

    it('drops an answer that arrives after the user signed out', async () => {
      const store = await signedInWith([]);

      const reading = store.loadOrganizations();
      const pending = backend.expectOne(ORGANIZATIONS);
      const signingOut = store.signOut();
      (await expectRequest(LOGOUT)).flush(null, { status: 204, statusText: 'No Content' });
      await signingOut;
      pending.flush([amt]);
      await reading;

      expect(store.organizations()).toEqual([]);
    });

    it('reads nothing while nobody is signed in', async () => {
      const store = setUp();

      await store.loadOrganizations();

      backend.expectNone(ORGANIZATIONS);
    });
  });
});
