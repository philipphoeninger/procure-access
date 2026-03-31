import { ChangeDetectionStrategy, Component, computed, inject, model, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { MatIconModule } from "@angular/material/icon";
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { ResetPasswordModel } from '../models/reset-password.model';
import { finalize, map } from 'rxjs/operators';

@Component({
  selector: 'pa-reset-password',
  imports: [
    MatButtonModule, 
    MatIconModule,
    RouterModule,
    MatFormFieldModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResetPassword {
    private route = inject(ActivatedRoute);

    protected email = signal("");
    protected token = signal("");

    protected password = model("");
    protected confirmPassword = model("");

    resetDisabled = computed(() => this.password() !== this.confirmPassword());

    constructor(
        private router: Router,
        protected authService: AuthService,
        protected snackbarService: SnackbarService
    ) {
        this.route.queryParams.subscribe((params) => {
            const email: string = params['email'];
            const token: string = params['token'];
            if (email == null || token == null) return; //gate
            this.email.set(email);
            this.token.set(token);
        });
    }

    onSubmit(form: NgForm, event: Event) {
        event.preventDefault();

        let resetCommand = new ResetPasswordModel(
            this.email(),
            this.token(),
            this.password()
        );

        // TODO: start spinner
        this.authService
            .register(resetCommand)
            .pipe(
                map((response: { succeeded: boolean }) => {
                if (response.succeeded) {
                    this.router.navigateByUrl('/(login:auth)');
                    this.snackbarService.showInfo('The password has been reset successfully. Please login with your new password.');
                } else {
                    this.snackbarService.showInfo('Something went wrong resetting your password. Please contact the support via email.');
                }
                }),
                finalize(() => {
                    // TODO: stop spinner
                }),
            )
            .subscribe();
    }
}
