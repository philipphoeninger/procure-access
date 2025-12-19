import { Injectable } from "@angular/core";
import { MatSnackBar, MatSnackBarConfig } from "@angular/material/snack-bar";

@Injectable({ providedIn:'root' })
export class SnackbarService {
    private config: MatSnackBarConfig = {
        horizontalPosition: 'center',
        verticalPosition: 'top'
    };

    constructor(private _snackBar: MatSnackBar) { }

    showInfo(message: string) {
        this._snackBar.open(message, 'Close', this.config);
    }

    showError(message:string) {
        this._snackBar.open(message, 'Dismiss')
    }
}
