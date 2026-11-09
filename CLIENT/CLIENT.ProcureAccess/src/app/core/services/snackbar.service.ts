// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Injectable, inject } from "@angular/core";
import { MatSnackBar, MatSnackBarConfig } from "@angular/material/snack-bar";
import { TranslateService } from "@ngx-translate/core";

/** Text of the snackbar's only button (owner decision O-S3-9, #160). */
const CLOSE_KEY = 'common.actions.close';

@Injectable({ providedIn:'root' })
export class SnackbarService {
    private _snackBar = inject(MatSnackBar);
    private translate = inject(TranslateService);

    private config: MatSnackBarConfig = {
        horizontalPosition: 'center',
        verticalPosition: 'top'
    };

    showInfo(message: string) {
        this.open(message, this.config);
    }

    showError(message:string) {
        this.open(message);
    }

    // `get`, not `instant`: the app does not wait for de.json at startup.
    private open(message: string, config?: MatSnackBarConfig) {
        this.translate.get(CLOSE_KEY).subscribe((close: string) => {
            this._snackBar.open(message, close, config);
        });
    }
}
