import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { SnackbarService } from '../services/snackbar.service';
import { ERROR_TRANSLATIONS } from '../models/error-translation.map';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const translate = inject(TranslateService);
  const snackbar = inject(SnackbarService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Extract backend error code safely
      const errorCode =
        error?.error?.code ||
        error?.error?.errors?.[0]?.code ||
        'Default';

      const key =
        ERROR_TRANSLATIONS[errorCode] ||
        ERROR_TRANSLATIONS['Default'];

      return translate.get(key).pipe(
        switchMap(message => {
          snackbar.showError(message);

          return throwError(() => error); // keep error flow
        })
      );
    })
  );
};
