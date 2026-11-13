// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ApplicationConfig, inject, InjectionToken, provideAppInitializer, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { provideRouter, TitleStrategy } from '@angular/router';

import { routes } from './app.routes';
import {
  provideHttpClient,
  withFetch,
  withInterceptors,
  withXsrfConfiguration,
} from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { environment } from '../environments/environment';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { EnLanguage } from './core/models/language.enum';
import { languageInterceptor } from './core/interceptors/language.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { provideAppearance } from './core/a11y/appearance';
import { provideDocumentLanguage } from './core/a11y/document-language';
import { PageTitleStrategy } from './core/a11y/page-title.strategy';

export const APP_NAME = new InjectionToken<string>('APP_NAME');
export const API_URL = new InjectionToken<string>('API_URL');
export const DEFAULT_LANGUAGE = EnLanguage.de;

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    // Page title, H1 focus and announcement on every route change (R-SHL-04, A11Y-03)
    { provide: TitleStrategy, useExisting: PageTitleStrategy },
    // Light/dark appearance and reduced motion from the system settings (R-SET-02, SPEC §7.5)
    provideAppearance(),
    // Every mat-icon uses the self-hosted Material Symbols Outlined font (design handoff §1.8).
    provideAppInitializer(() => {
      inject(MatIconRegistry).setDefaultFontSetClass('material-symbols-outlined');
    }),
    provideHttpClient(
      withFetch(),
      // Antiforgery (SEC-02, SEC-09; ARCHITECTURE §8.3): the cookie XSRF-TOKEN of the session probe
      // goes back as X-XSRF-TOKEN with every state-changing request to a relative URL. `apiUrl` is
      // relative in every environment (the dev server proxies /api, #317), so this is the only
      // place that handles the token; nothing in JavaScript holds a credential (SEC-01).
      withXsrfConfiguration({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' }),
      withInterceptors([authInterceptor, languageInterceptor, errorInterceptor]),
    ),
    { provide: APP_NAME, useValue: 'ProcureAccess'},
    { provide: API_URL, useValue: environment.apiUrl },
    provideTranslateService({
      loader: provideTranslateHttpLoader({
        prefix: './i18n/',
        suffix: '.json'
      })
    }),
    provideDocumentLanguage(),
  ]
};
