// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import {
  BACKGROUND_REQUEST,
  CALLER_HANDLES_PROBLEMS,
  CALLER_SHOWS_FAILURES,
  FORM_SHOWS_FIELD_ERRORS,
  problemTypeName,
} from '../errors/problem-details';
import { ConnectionStatus } from '../errors/system-errors';
import { SnackbarService } from '../services/snackbar.service';
import { errorInterceptor } from './error.interceptor';

const base = 'https://procureaccess.de/problems/';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let snackbar: { showError: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    snackbar = { showError: vi.fn() };
    router = { navigate: vi.fn().mockResolvedValue(true) };
    TestBed.configureTestingModule({
      providers: [
        provideTestTranslations(),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: SnackbarService, useValue: snackbar },
        { provide: Router, useValue: router },
        { provide: API_URL, useValue: '/api' },
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    await firstValueFrom(TestBed.inject(TranslateService).use('de'));
  });

  afterEach(() => backend.verify());

  async function failWith(body: object, context?: HttpContext) {
    const request = firstValueFrom(http.post('/api/x', {}, { context }));
    backend.expectOne('/api/x').flush(body, { status: 400, statusText: 'Bad Request' });
    await expect(request).rejects.toBeDefined();
  }

  it('shows no snackbar for a validation problem that the form shows at its fields', async () => {
    await failWith(
      { type: `${base}validation`, errors: { name: ['Required.'] } },
      new HttpContext().set(FORM_SHOWS_FIELD_ERRORS, true),
    );

    expect(snackbar.showError).not.toHaveBeenCalled();
  });

  it('shows the generic text for a validation problem of a form without field errors', async () => {
    await failWith({ type: `${base}validation`, errors: { name: ['Required.'] } });

    expect(snackbar.showError).toHaveBeenCalledWith(
      TestBed.inject(TranslateService).instant('ERROR.GENERIC'),
    );
  });

  // SEC-03: the session interceptor opens the login overlay and retries; nothing else shows.
  it('shows neither snackbar nor server page for a 401', async () => {
    const request = firstValueFrom(http.post('/api/configurations', {}));
    backend.expectOne('/api/configurations').flush({ status: 401 }, { status: 401, statusText: 'Unauthorized' });
    await expect(request).rejects.toMatchObject({ status: 401 });

    expect(snackbar.showError).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('shows no snackbar for a problem type the caller handles itself', async () => {
    const request = firstValueFrom(
      http.get('/api/x', { context: new HttpContext().set(CALLER_HANDLES_PROBLEMS, ['not-found']) }),
    );
    backend.expectOne('/api/x').flush({ type: `${base}not-found` }, { status: 404, statusText: 'Not Found' });
    await expect(request).rejects.toBeDefined();

    expect(snackbar.showError).not.toHaveBeenCalled();
  });

  it('keeps the snackbar for a problem type the caller did not name', async () => {
    await failWith(
      { type: `${base}validation`, errors: { name: ['Required.'] } },
      new HttpContext().set(CALLER_HANDLES_PROBLEMS, ['not-found']),
    );

    expect(snackbar.showError).toHaveBeenCalledOnce();
  });

  it('shows the UX text of a mapped problem type', async () => {
    await failWith({ type: `${base}email-unconfirmed`, status: 403 });

    expect(snackbar.showError).toHaveBeenCalledWith(
      'Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse. Danach können Sie Listen speichern, Organisationen anlegen, Einladungen annehmen und Beiträge veröffentlichen.',
    );
  });

  it('still maps the codes of the legacy identity endpoints', async () => {
    await failWith({ code: 'UserNotFound' });

    expect(snackbar.showError).toHaveBeenCalledWith(TestBed.inject(TranslateService).instant('ERROR.USER_NOT_FOUND'));
  });

  describe('system states (Handoff §6.5)', () => {
    const offline = () => TestBed.inject(ConnectionStatus).offline();

    async function getAndFail(status: number) {
      const request = firstValueFrom(http.get('/api/x'));
      if (status === 0) {
        backend.expectOne('/api/x').error(new ProgressEvent('error'), { status: 0, statusText: '' });
      } else {
        backend.expectOne('/api/x').flush({ status }, { status, statusText: 'Error' });
      }
      await expect(request).rejects.toBeDefined();
    }

    it.each([500, 502, 503])('shows the server error page in place for %i, without a snackbar', async (status) => {
      await getAndFail(status);

      expect(router.navigate).toHaveBeenCalledWith(['error'], { skipLocationChange: true });
      expect(snackbar.showError).not.toHaveBeenCalled();
    });

    it('shows the offline notice when a request gets no answer, without a snackbar', async () => {
      await getAndFail(0);

      expect(offline()).toBe(true);
      expect(router.navigate).not.toHaveBeenCalled();
      expect(snackbar.showError).not.toHaveBeenCalled();
    });

    it('hides the offline notice with the next answer of the server', async () => {
      await getAndFail(0);

      const request = firstValueFrom(http.get('/api/y'));
      backend.expectOne('/api/y').flush({});
      await request;

      expect(offline()).toBe(false);
    });

    it('hides the offline notice also when the next answer is an error', async () => {
      await getAndFail(0);

      await failWith({ type: `${base}validation` });

      expect(offline()).toBe(false);
    });

    it('keeps the offline notice when only a static file answers after a request to the API failed (#171)', async () => {
      const translations = firstValueFrom(http.get('/i18n/de.json'));
      await getAndFail(0);

      backend.expectOne('/i18n/de.json').flush({});
      await translations;

      expect(offline()).toBe(true);
    });

    it('counts an answer only for a path under the API address, not for one that merely starts like it', async () => {
      await getAndFail(0);

      const request = firstValueFrom(http.get('/apidocs'));
      backend.expectOne('/apidocs').flush({});
      await request;

      expect(offline()).toBe(true);
    });
  });

  describe('a request the user did not ask for (BACKGROUND_REQUEST, S4-18b)', () => {
    const background = () => new HttpContext().set(BACKGROUND_REQUEST, true);
    const offline = () => TestBed.inject(ConnectionStatus).offline();

    async function failInBackground(status: number, body: object | null = { status }) {
      const request = firstValueFrom(http.post('/api/x', null, { context: background() }));
      if (status === 0) {
        backend.expectOne('/api/x').error(new ProgressEvent('error'), { status: 0, statusText: '' });
      } else {
        backend.expectOne('/api/x').flush(body, { status, statusText: 'Error' });
      }
      await expect(request).rejects.toBeDefined();
    }

    it.each([
      ['no answer', 0],
      ['a server error', 500],
      ['a problem', 400],
      ['a rate limit', 429],
    ])('shows nothing for %s', async (_case, status) => {
      await failInBackground(status, { type: `${base}validation` });

      expect(offline()).toBe(false);
      expect(router.navigate).not.toHaveBeenCalled();
      expect(snackbar.showError).not.toHaveBeenCalled();
    });

    it('still hides the offline notice when it gets an answer', async () => {
      const request = firstValueFrom(http.get('/api/y'));
      backend.expectOne('/api/y').error(new ProgressEvent('error'), { status: 0, statusText: '' });
      await expect(request).rejects.toBeDefined();
      expect(offline()).toBe(true);

      await failInBackground(500);

      expect(offline()).toBe(false);
    });
  });

  describe('a request for a file (responseType blob, S4-21b)', () => {
    async function failWithBlob(body: Blob, context?: HttpContext): Promise<HttpErrorResponse> {
      const request = firstValueFrom(http.post('/api/file', {}, { responseType: 'blob', context }));
      backend.expectOne('/api/file').flush(body, { status: 429, statusText: 'Too Many Requests' });
      return (await request.then(
        () => Promise.reject(new Error('expected a failure')),
        (error: unknown) => error,
      )) as HttpErrorResponse;
    }

    it('reads Problem Details from the Blob, so the problem type counts', async () => {
      const problem = { type: `${base}rate-limit`, status: 429, retryAfterMinutes: 10 };
      const error = await failWithBlob(
        new Blob([JSON.stringify(problem)], { type: 'application/problem+json' }),
        new HttpContext().set(CALLER_HANDLES_PROBLEMS, ['rate-limit']),
      );

      expect(problemTypeName(error)).toBe('rate-limit');
      expect(error.error).toEqual(problem);
      expect(error.status).toBe(429);
      expect(snackbar.showError).not.toHaveBeenCalled();
    });

    it('keeps a body that is not JSON as it is', async () => {
      const body = new Blob(['oops'], { type: 'text/plain' });

      const error = await failWithBlob(body);

      expect(error.error).toBe(body);
      expect(snackbar.showError).toHaveBeenCalled();
    });
  });

  describe('a request whose caller shows every failure (CALLER_SHOWS_FAILURES, S4-21b)', () => {
    const shown = () => new HttpContext().set(CALLER_SHOWS_FAILURES, true);
    const offline = () => TestBed.inject(ConnectionStatus).offline();

    it.each([
      ['a server error', 500],
      ['a problem', 400],
      ['a rate limit', 429],
    ])('shows neither snackbar nor server page for %s', async (_case, status) => {
      const request = firstValueFrom(http.post('/api/x', null, { context: shown() }));
      backend.expectOne('/api/x').flush({ type: `${base}validation` }, { status, statusText: 'Error' });
      await expect(request).rejects.toBeDefined();

      expect(router.navigate).not.toHaveBeenCalled();
      expect(snackbar.showError).not.toHaveBeenCalled();
    });

    it('still turns the offline notice on when there is no answer', async () => {
      const request = firstValueFrom(http.post('/api/x', null, { context: shown() }));
      backend.expectOne('/api/x').error(new ProgressEvent('error'), { status: 0, statusText: '' });
      await expect(request).rejects.toBeDefined();

      expect(offline()).toBe(true);
      expect(snackbar.showError).not.toHaveBeenCalled();
    });
  });
});
