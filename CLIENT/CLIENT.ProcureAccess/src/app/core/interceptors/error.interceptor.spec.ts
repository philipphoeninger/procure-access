// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { CALLER_HANDLES_PROBLEMS, FORM_SHOWS_FIELD_ERRORS } from '../errors/problem-details';
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
});
