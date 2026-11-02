// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { FORM_SHOWS_FIELD_ERRORS } from '../errors/problem-details';
import { SnackbarService } from '../services/snackbar.service';
import { errorInterceptor } from './error.interceptor';

const base = 'https://procureaccess.de/problems/';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let snackbar: { showError: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    snackbar = { showError: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        provideTestTranslations(),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: SnackbarService, useValue: snackbar },
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
});
