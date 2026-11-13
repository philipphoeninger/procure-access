// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST } from '@app/core/errors/problem-details';
import { LoginOverlayService } from '@app/core/session/login-overlay.service';
import { authInterceptor, RETRIED_AFTER_SIGN_IN } from './auth.interceptor';

describe('authInterceptor (401 → login overlay and retry, ARCHITECTURE §8.3)', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let requestSignIn: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    requestSignIn = vi.fn();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: API_URL, useValue: '/api' },
        { provide: LoginOverlayService, useValue: { requestSignIn } },
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  const unauthorized = { status: 401, statusText: 'Unauthorized' };

  /** Lets the interceptor's awaited overlay answer settle (a macrotask), then expects the retry. */
  async function expectRetry(url: string) {
    await new Promise((resolve) => setTimeout(resolve, 0));
    return backend.expectOne(url);
  }

  it('Request_401ThenSignIn_OpensOverlayAndRetriesTheRequestOnce INV:SEC-03', async () => {
    requestSignIn.mockResolvedValue(true);
    const body = { note: 'kept' };

    const request = firstValueFrom(http.post('/api/configurations', body));
    backend.expectOne('/api/configurations').flush('', unauthorized);
    const retry = await expectRetry('/api/configurations');

    expect(requestSignIn).toHaveBeenCalledOnce();
    expect(retry.request.body).toEqual(body);
    expect(retry.request.context.get(RETRIED_AFTER_SIGN_IN)).toBe(true);
    retry.flush({ id: 7 });
    await expect(request).resolves.toEqual({ id: 7 });
  });

  it('Request_401AndOverlayClosed_FailsWithThe401WithoutRetry INV:SEC-03', async () => {
    requestSignIn.mockResolvedValue(false);

    const request = firstValueFrom(http.get('/api/configurations'));
    backend.expectOne('/api/configurations').flush('', unauthorized);

    await expect(request).rejects.toMatchObject({ status: 401 });
    expect(requestSignIn).toHaveBeenCalledOnce();
    backend.expectNone('/api/configurations');
  });

  it('Request_RetryAnswers401Again_FailsWithoutASecondOverlay INV:SEC-03', async () => {
    requestSignIn.mockResolvedValue(true);

    const request = firstValueFrom(http.get('/api/configurations'));
    backend.expectOne('/api/configurations').flush('', unauthorized);
    (await expectRetry('/api/configurations')).flush('', unauthorized);

    await expect(request).rejects.toMatchObject({ status: 401 });
    expect(requestSignIn).toHaveBeenCalledOnce();
  });

  it('SessionProbe_401_IsIgnoredAndOpensNoOverlay INV:SEC-03', async () => {
    const request = firstValueFrom(http.get('/api/v1/auth/session'));
    backend.expectOne('/api/v1/auth/session').flush('', unauthorized);

    await expect(request).rejects.toMatchObject({ status: 401 });
    expect(requestSignIn).not.toHaveBeenCalled();
  });

  it('opens no overlay for a request the user did not ask for', async () => {
    const request = firstValueFrom(
      http.post('/api/v1/configurator/criteria-list-shown', null, {
        context: new HttpContext().set(BACKGROUND_REQUEST, true),
      }),
    );
    backend.expectOne('/api/v1/configurator/criteria-list-shown').flush('', unauthorized);

    await expect(request).rejects.toMatchObject({ status: 401 });
    expect(requestSignIn).not.toHaveBeenCalled();
  });

  it.each([403, 404, 500])('passes a %i on without an overlay', async (status) => {
    const request = firstValueFrom(http.get('/api/configurations'));
    backend.expectOne('/api/configurations').flush('', { status, statusText: 'Error' });

    await expect(request).rejects.toMatchObject({ status });
    expect(requestSignIn).not.toHaveBeenCalled();
  });
});
