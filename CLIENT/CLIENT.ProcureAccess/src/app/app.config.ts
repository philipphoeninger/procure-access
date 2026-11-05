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
} from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { headersInterceptor } from './core/interceptors/headers.interceptor';
import { provideMarkdown } from 'ngx-markdown';
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
export const JWT_NAME = new InjectionToken<string>('JWT_NAME');
export const REFRESH_TOKEN_NAME = new InjectionToken<string>('REFRESH_TOKEN_NAME');
export const DEFAULT_LANGUAGE = EnLanguage.de;

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    // Page title, H1 focus and announcement on every route change (R-SHL-04, A11Y-03)
    { provide: TitleStrategy, useClass: PageTitleStrategy },
    // Light/dark appearance and reduced motion from the system settings (R-SET-02, SPEC §7.5)
    provideAppearance(),
    // Every mat-icon uses the self-hosted Material Symbols Outlined font (design handoff §1.8).
    provideAppInitializer(() => {
      inject(MatIconRegistry).setDefaultFontSetClass('material-symbols-outlined');
    }),
    provideHttpClient(
      withFetch(),
      withInterceptors([
        authInterceptor, 
        headersInterceptor, 
        languageInterceptor,
        errorInterceptor]),
    ),
    provideMarkdown(),
    { provide: APP_NAME, useValue: 'ProcureAccess'},
    { provide: API_URL, useValue: environment.apiUrl },
    { provide: JWT_NAME,
      useFactory: () => {
        const appName = inject(APP_NAME);
        return appName.toLowerCase() + '-token';
      }
    },
    { provide: REFRESH_TOKEN_NAME,
      useFactory: () => {
        const appName = inject(APP_NAME);
        return appName.toLowerCase() + '-refresh-token';
      }
    },
    provideTranslateService({
      loader: provideTranslateHttpLoader({
        prefix: './i18n/',
        suffix: '.json'
      })
    }),
    provideDocumentLanguage(),
  ]
};
