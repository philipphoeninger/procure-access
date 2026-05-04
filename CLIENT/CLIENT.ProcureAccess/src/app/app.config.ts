import { ApplicationConfig, inject, InjectionToken, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
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
    provideAnimationsAsync(),
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
    })
  ]
};
