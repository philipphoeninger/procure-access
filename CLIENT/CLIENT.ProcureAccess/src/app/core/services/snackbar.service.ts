// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Injectable, inject } from "@angular/core";
import { MatSnackBar, MatSnackBarConfig } from "@angular/material/snack-bar";

@Injectable({ providedIn:'root' })
export class SnackbarService {
    private _snackBar = inject(MatSnackBar);

    private config: MatSnackBarConfig = {
        horizontalPosition: 'center',
        verticalPosition: 'top'
    };

    showInfo(message: string) {
        this._snackBar.open(message, 'Close', this.config);
    }

    showError(message:string) {
        this._snackBar.open(message, 'Dismiss')
    }
}
