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
import { environment } from '../environments/environment';

export const APP_NAME = new InjectionToken<string>('APP_NAME');
export const API_URL = new InjectionToken<string>('API_URL');
export const JWT_NAME = new InjectionToken<string>('JWT_NAME');

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor, headersInterceptor]),
    ),
    { provide: APP_NAME, useValue: 'ProcureAccess'},
    { provide: API_URL, useValue: environment.apiUrl },
    { provide: JWT_NAME,
      useFactory: () => {
        const appName = inject(APP_NAME);
        return appName.toLowerCase() + '-token';
      }
    }
  ]
};
