// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { TranslatePipe } from '@ngx-translate/core';

/**
 * Placeholder of the export dialog S13, opened by „Liste exportieren“ in the S11 action bar
 * (R-LST-10): the title `criteria.export.title` and „Schließen“ only. S4-21 replaces it with S13
 * (R-LST-11: format, scope, notes, the export itself).
 */
@Component({
  selector: 'pa-export-placeholder-dialog',
  imports: [MatButtonModule, MatDialogModule, TranslatePipe],
  template: `
    <h2 mat-dialog-title>{{ 'criteria.export.title' | translate }}</h2>
    <mat-dialog-actions>
      <button matButton="filled" type="button" mat-dialog-close>{{ 'common.actions.close' | translate }}</button>
    </mat-dialog-actions>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExportPlaceholderDialog {}
