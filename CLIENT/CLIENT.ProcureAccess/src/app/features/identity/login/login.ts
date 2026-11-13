// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject, model, signal } from '@angular/core';
import { FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { finalize } from 'rxjs/operators';

import { SnackbarService } from '@app/core/services/snackbar.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ForgotPasswordDialog } from '@features/identity/dialogs/forgot-password-dialog/forgot-password-dialog';
import { AuthService } from '@features/identity/services/auth.service';

/**
 * Legacy sign-in page in the `login` outlet (replaced by S01 in S5-09). Since S5-08 it signs in
 * through the cookie session (`withIdentity().signIn()`): no token is stored, and the UI settings
 * come from the legacy user the store loads after the sign-in.
 */
@Component({
  selector: 'pa-login',
  imports: [
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    TranslatePipe
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Login {
  protected readonly store = inject(ProcureAccessStore);
  protected readonly authService = inject(AuthService);
  protected readonly snackbarService = inject(SnackbarService);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  readonly dialog = inject(MatDialog);

  /** `auth.login.error.invalid` after a refused sign-in; the template shows it above the buttons. */
  protected readonly loginErrorMessage = signal<string | null>(null);
  public showLanguageSelection = true;

  protected logoPath = '/werte_it_logo.jpg';

  protected email = model('');
  protected password = model('');
  public keepSignedIn = model(false);

  openForgotPwDialog(): void {
    const dialogRef = this.dialog.open(ForgotPasswordDialog, {
      data: { email: this.email() },
      width: '420px'
    });

    dialogRef.afterClosed().subscribe((email: string | undefined) => {
      if (email !== undefined) {
        this.store.incrementLoadingCount();
        this.authService
          .forgotPassword(email)
          .pipe(finalize(() => this.store.decrementLoadingCount()))
          .subscribe();
      }
    });
  }

  async onSubmit(form: NgForm, event: Event): Promise<void> {
    event.preventDefault();
    if (form.invalid) {
      return;
    }
    this.loginErrorMessage.set(null);
    this.store.incrementLoadingCount();
    try {
      const user = await this.store.signIn(this.email(), this.password());
      if (user) {
        // The legacy settings of the user (S60 replaces them in S5-25).
        this.store.setUICustomization(user.uiCustomization);
        this.store.setLanguage(user.uiCustomization.language);
        this.store.removeSettingsFromLocalStorage();
      }
      void this.store.loadProposals();
      await this.router.navigateByUrl('/');
    } catch {
      // 400 validation for the body as a whole: wrong address or password, locked or deleted (SPEC §8.8).
      this.loginErrorMessage.set(this.translate.instant('auth.login.error.invalid') as string);
    } finally {
      this.store.decrementLoadingCount();
    }
  }
}
